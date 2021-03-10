using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UAB.Models;
using Microsoft.EntityFrameworkCore;
using UAB.DAL;
using UAB.DTO;
using UAB.DAL.Models;
using UAB.enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using Microsoft.AspNetCore.Http;
using ExcelDataReader;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UAB.Controllers
{
    [Authorize]
    public class UABController : Controller
    {
        private static int mUserId;
        #region Coding
        public IActionResult CodingSummary()
        {
            var identity = (ClaimsIdentity)User.Identity;
            mUserId = Convert.ToInt32(identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);

            List<DashboardDTO> lstDto = new List<DashboardDTO>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            List<int> lstStatus = new List<int> { (int)StatusType.ReadyForCoding, (int)StatusType.QARejected, (int)StatusType.ShadowQARejected,
                (int)StatusType.PostingCompleted };

            lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            return View(lstDto);
        }

        public IActionResult GetCodingAvailableChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            #endregion

            return View("Coding", chartSummaryDTO);
        }
        [HttpGet]
        public IActionResult BlockClinicalcase(string ccid )
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.BlockCategories = clinicalcaseOperations.GetBlockCategories();
            ViewBag.ccid = Convert.ToInt32(ccid);
            return PartialView("_BlockCategory");
        }
        [HttpPost]
        public IActionResult BlockClinicalcase(string ccid,string bid,string remarks)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            if (ccid!=null&&bid!=null&&remarks!=null)
            {
                clinicalcaseOperations.BlockClinicalcase(ccid, bid, remarks);
            }
            return RedirectToAction("CodingSummary");
        }

        public IActionResult GetCodingIncorrectChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion

            return View("IncorrectCharts", chartSummaryDTO);
        }

        public IActionResult GetCodingReadyForPostingChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            return View("ReadyForPostingChart", chartSummaryDTO);
        }

        [HttpPost]
        public IActionResult SubmitCodingAvailableChart(ChartSummaryDTO chartSummaryDTO, string codingSubmitAndGetNext)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            if (string.IsNullOrEmpty(codingSubmitAndGetNext))
                clinicalcaseOperations.SubmitCodingAvailableChart(chartSummaryDTO);
            else
            {
                clinicalcaseOperations.SubmitCodingAvailableChart(chartSummaryDTO);
                return RedirectToAction("GetCodingAvailableChart", new { Role = Roles.Coder.ToString(), ChartType = "Available", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }
            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());
            TempData["Success"] = "Chart Details submitted successfully !";
            return View("CodingSummary", lstDto);
        }
        public IActionResult SubmitCodingIncorrectChart(ChartSummaryDTO chartSummaryDTO)
        {
            var hdnPayorID = Request.Form["hdnPayorID"].ToString();
            var hdnProviderID = Request.Form["hdnProviderID"].ToString();
            var hdnCpt = Request.Form["hdnCpt"].ToString();
            var hdnMod = Request.Form["hdnMod"].ToString();
            var hdnDx = Request.Form["hdnDx"].ToString();
            var hdnProviderFeedbackID = Request.Form["hdnProviderFeedbackID"].ToString();
            int statusId = 0;

            if (!string.IsNullOrEmpty(hdnPayorID) || !string.IsNullOrEmpty(hdnProviderID)
                || !string.IsNullOrEmpty(hdnCpt) || !string.IsNullOrEmpty(hdnMod)
                || !string.IsNullOrEmpty(hdnDx) || !string.IsNullOrEmpty(hdnProviderFeedbackID))
                statusId = 15;
            else
                statusId = 12;

            if (!string.IsNullOrEmpty(hdnPayorID))
                chartSummaryDTO.PayorID = Convert.ToInt32(hdnPayorID);

            if (!string.IsNullOrEmpty(hdnProviderID))
                chartSummaryDTO.ProviderID = Convert.ToInt32(hdnProviderID);

            if (!string.IsNullOrEmpty(hdnCpt))
                chartSummaryDTO.CPTCode = hdnCpt;

            if (!string.IsNullOrEmpty(hdnMod))
                chartSummaryDTO.Mod = hdnMod;

            if (!string.IsNullOrEmpty(hdnDx))
                chartSummaryDTO.Dx = hdnDx;

            if (!string.IsNullOrEmpty(hdnProviderFeedbackID))
                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(hdnProviderFeedbackID);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitCodingIncorrectChart(chartSummaryDTO, statusId);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("CodingSummary", lstDto);
        }
        public IActionResult SubmitCodingReadyForPostingChart(ChartSummaryDTO chartSummaryDTO)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            clinicalcaseOperations.SubmitCodingReadyForPostingChart(chartSummaryDTO);
            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            TempData["Success"] = "Chart Details posted successfully !";
            return View("CodingSummary", lstDto);
        }
        #endregion

        #region QA
        public IActionResult QASummary()
        {
            var identity = (ClaimsIdentity)User.Identity;
            mUserId = Convert.ToInt32(identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            return View(lstDto);
        }
        public IActionResult GetQAAvailableChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            return View("QA", chartSummaryDTO);
        }

        public IActionResult GetQARebuttalChartsOfCoder(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            return View("QARebuttalChartsOfCoder", chartSummaryDTO);
        }

        public IActionResult GetQARejectedChartsOfShadowQA(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            return View("QARejectedChartsOfShadowQA", chartSummaryDTO);
        }
        public IActionResult GetQAOnHoldChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            #endregion

            return View("OnHold", chartSummaryDTO);
        }
        public IActionResult SubmitQAAvailableChart(ChartSummaryDTO chartSummaryDTO, string SubmitAndGetNext)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            if (string.IsNullOrEmpty(SubmitAndGetNext))
                clinicalcaseOperations.SubmitQAAvailableChart(chartSummaryDTO);
            else
            {
                clinicalcaseOperations.SubmitQAAvailableChart(chartSummaryDTO);
                return RedirectToAction("GetQAAvailableChart", new { Role = Roles.QA.ToString(), ChartType = "Available", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }
            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("QASummary", lstDto);
        }
        public IActionResult SubmitQARebuttalChartsOfCoder(ChartSummaryDTO chartSummaryDTO, string SubmitAndGetNext)
        {
            var hdnPayorID = Request.Form["hdnPayorID"].ToString();
            var hdnProviderID = Request.Form["hdnProviderID"].ToString();
            var hdnCpt = Request.Form["hdnCpt"].ToString();
            var hdnMod = Request.Form["hdnMod"].ToString();
            var hdnDx = Request.Form["hdnDx"].ToString();
            var hdnProviderFeedbackID = Request.Form["hdnProviderFeedbackID"].ToString();

            if (!string.IsNullOrEmpty(hdnPayorID))
                chartSummaryDTO.PayorID = Convert.ToInt32(hdnPayorID);
            else
                chartSummaryDTO.PayorID = 0;

            if (!string.IsNullOrEmpty(hdnProviderID))
                chartSummaryDTO.ProviderID = Convert.ToInt32(hdnProviderID);
            else
                chartSummaryDTO.ProviderID = 0;

            if (!string.IsNullOrEmpty(hdnCpt))
                chartSummaryDTO.CPTCode = hdnCpt;
            else
                chartSummaryDTO.CPTCode = "";

            if (!string.IsNullOrEmpty(hdnMod))
                chartSummaryDTO.Mod = hdnMod;
            else
                chartSummaryDTO.Mod = "";

            if (!string.IsNullOrEmpty(hdnDx))
                chartSummaryDTO.Dx = hdnDx;
            else
                chartSummaryDTO.Dx = "";

            if (!string.IsNullOrEmpty(hdnProviderFeedbackID))
                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(hdnProviderFeedbackID);
            else
                chartSummaryDTO.ProviderFeedbackID = 0;

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            if (string.IsNullOrEmpty(SubmitAndGetNext))
                clinicalcaseOperations.SubmitQARebuttalChartsOfCoder(chartSummaryDTO);
            else
            {
                clinicalcaseOperations.SubmitQARebuttalChartsOfCoder(chartSummaryDTO);
                return RedirectToAction("GetQARebuttalChartsOfCoder", new { Role = Roles.QA.ToString(), ChartType = "RebuttalOfCoder", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("QASummary", lstDto);
        }
        public IActionResult SubmitQARejectedChartsOfShadowQA(ChartSummaryDTO chartSummaryDTO)
        {
            var hdnPayorID = Request.Form["hdnPayorID"].ToString();
            var hdnProviderID = Request.Form["hdnProviderID"].ToString();
            var hdnCpt = Request.Form["hdnCpt"].ToString();
            var hdnMod = Request.Form["hdnMod"].ToString();
            var hdnDx = Request.Form["hdnDx"].ToString();
            var hdnProviderFeedbackID = Request.Form["hdnProviderFeedbackID"].ToString();

            var hdnPayorIDReject = Request.Form["hdnPayorIDReject"].ToString();
            var hdnProviderIDReject = Request.Form["hdnProviderIDReject"].ToString();
            var hdnCptReject = Request.Form["hdnCptReject"].ToString();
            var hdnModReject = Request.Form["hdnModReject"].ToString();
            var hdnDxReject = Request.Form["hdnDxReject"].ToString();
            var hdnProviderFeedbackIDReject = Request.Form["hdnProviderFeedbackIDReject"].ToString();

            if (!string.IsNullOrEmpty(hdnPayorID))
                chartSummaryDTO.PayorID = Convert.ToInt32(hdnPayorID);

            if (!string.IsNullOrEmpty(hdnProviderID))
                chartSummaryDTO.ProviderID = Convert.ToInt32(hdnProviderID);

            if (!string.IsNullOrEmpty(hdnCpt))
                chartSummaryDTO.CPTCode = hdnCpt;

            if (!string.IsNullOrEmpty(hdnMod))
                chartSummaryDTO.Mod = hdnMod;

            if (!string.IsNullOrEmpty(hdnDx))
                chartSummaryDTO.Dx = hdnDx;

            if (!string.IsNullOrEmpty(hdnProviderFeedbackID))
                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(hdnProviderFeedbackID);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitQARejectedChartsOfShadowQA(chartSummaryDTO, hdnPayorIDReject, hdnProviderIDReject, hdnCptReject, hdnModReject, hdnDxReject, hdnProviderFeedbackIDReject);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("QASummary", lstDto);
        }

        public IActionResult SubmitQAOnHoldChart(ChartSummaryDTO chartSummaryDTO)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitQAOnHoldChart(chartSummaryDTO);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("QASummary", lstDto);
        }

        #endregion

        #region Shadow QA
        public IActionResult ShadowQASummary()
        {
            var identity = (ClaimsIdentity)User.Identity;
            mUserId = Convert.ToInt32(identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.ShadowQA.ToString());

            return View(lstDto);
        }

        public IActionResult GetShadowQAAvailableChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            return View("ShadowQA", chartSummaryDTO);
        }

        public IActionResult GetShadowQARebuttalChartsOfQA(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            return View("ShadowQARebuttalChartsOfQA", chartSummaryDTO);
        }

        [HttpPost]
        public IActionResult SubmitShadowQAAvailableChart(ChartSummaryDTO chartSummaryDTO, bool hdnIsQAAgreed, string SubmitAndGetNext)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            bool isQAAgreed = hdnIsQAAgreed;// Convert.ToBoolean(Request.Form["hdnIsQAAgreed"]);

            if (string.IsNullOrEmpty(SubmitAndGetNext))
                clinicalcaseOperations.SubmitShadowQAAvailableChart(chartSummaryDTO, isQAAgreed);
            else
            {
                clinicalcaseOperations.SubmitShadowQAAvailableChart(chartSummaryDTO, isQAAgreed);
                return RedirectToAction("GetShadowQAAvailableChart", new { Role = Roles.ShadowQA.ToString(), ChartType = "Available", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }
            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.ShadowQA.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("ShadowQASummary", lstDto);
        }

        public IActionResult SubmitShadowQARebuttalChartsOfQA(ChartSummaryDTO chartSummaryDTO)
        {
            var hdnPayorID = Request.Form["hdnPayorID"].ToString();
            var hdnProviderID = Request.Form["hdnProviderID"].ToString();
            var hdnCpt = Request.Form["hdnCpt"].ToString();
            var hdnMod = Request.Form["hdnMod"].ToString();
            var hdnDx = Request.Form["hdnDx"].ToString();
            var hdnProviderFeedbackID = Request.Form["hdnProviderFeedbackID"].ToString();

            var hdnPayorIDReject = Request.Form["hdnPayorIDReject"].ToString();
            var hdnProviderIDReject = Request.Form["hdnProviderIDReject"].ToString();
            var hdnCptReject = Request.Form["hdnCptReject"].ToString();
            var hdnModReject = Request.Form["hdnModReject"].ToString();
            var hdnDxReject = Request.Form["hdnDxReject"].ToString();
            var hdnProviderFeedbackIDReject = Request.Form["hdnProviderFeedbackIDReject"].ToString();

            if (!string.IsNullOrEmpty(hdnPayorID))
                chartSummaryDTO.PayorID = Convert.ToInt32(hdnPayorID);

            if (!string.IsNullOrEmpty(hdnProviderID))
                chartSummaryDTO.ProviderID = Convert.ToInt32(hdnProviderID);

            if (!string.IsNullOrEmpty(hdnCpt))
                chartSummaryDTO.CPTCode = hdnCpt;

            if (!string.IsNullOrEmpty(hdnMod))
                chartSummaryDTO.Mod = hdnMod;

            if (!string.IsNullOrEmpty(hdnDx))
                chartSummaryDTO.Dx = hdnDx;

            if (!string.IsNullOrEmpty(hdnProviderFeedbackID))
                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(hdnProviderFeedbackID);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitShadowQARebuttalChartsOfQA(chartSummaryDTO, hdnPayorIDReject, hdnProviderIDReject, hdnCptReject, hdnModReject, hdnDxReject, hdnProviderFeedbackIDReject);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("ShadowQASummary", lstDto);
        }
        #endregion

        #region Settings
       
        public List<BindDTO> BindErrorType()
        {
            List<BindDTO> lstDto = new List<BindDTO>();
            lstDto.Add(new BindDTO()
            {
                ID = 1,
                Name = "CC not Supported"
            });
            lstDto.Add(new BindDTO()
            {
                ID = 2,
                Name = "Consult not Supported",
            });
            lstDto.Add(new BindDTO()
            {
                ID = 3,
                Name = "Mod Error",
            });
            return lstDto;
        }

        [HttpGet]
        public IActionResult AssignClinicalCaseToUser(string ccid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var assignusers = clinicalcaseOperations.GetManageUsers();
            var workitem = clinicalcaseOperations.GetWorkItem(ccid);


            ViewBag.assignusers = assignusers;
            ViewBag.ccid = ccid;
            SearchResultDTO SearchResultDTO = new SearchResultDTO();
            SearchResultDTO.ClinicalCaseId = ccid;
            SearchResultDTO.AssignFromUserEmail = workitem.AssignedTo.ToString();
            return PartialView("_AssignClinicalCaseToUser", SearchResultDTO);

        }
        [HttpPost]
        public IActionResult AssignClinicalCaseToUser(string ccid, string AssignedTo, string IsPriority)
        {
            SearchResultDTO SearchResultDTO = new SearchResultDTO();
            SearchResultDTO.ClinicalCaseId = ccid;
            SearchResultDTO.AssignToUserEmail = AssignedTo;
            SearchResultDTO.IsPriority = Convert.ToBoolean(IsPriority);


            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            clinicalcaseOperations.AssignClinicalcase(SearchResultDTO);

            TempData["Success"] = "Clinical case AssignedSuccessfully";

            return RedirectToAction("SettingsSearch");

        }
        [HttpGet]
        public IActionResult SettingsSearch()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();
            ViewBag.Status = clinicalcaseOperations.GetStatusList();

            return View();
        }
        [HttpPost]
        public IActionResult SettingsSearch(string ccid, string fname, string lname, string mrn, DateTime dosfrom, DateTime dosto, string statusname, string projectname, string providername)
        {

            SearchParametersDTO searchParametersDTO = new SearchParametersDTO()
            {
                ClinicalCaseId = ccid,
                FirstName = fname,
                LastName = lname,
                MRN = mrn,
                DoSFrom = dosfrom,
                DoSTo = dosto,
                StatusName = statusname,
                ProjectName = projectname,
                ProviderName = providername
            };
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var searchData = clinicalcaseOperations.GetSearchData(searchParametersDTO);
            return PartialView("_SettingsSearchResults", searchData);
        }
        [HttpPost]
        public IActionResult AddSettingsProvider(Provider provider)
        {
            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                List<string> lstProvider = clinicalcaseOperations.GetProviderNames();
                if (!lstProvider.Contains(provider.Name.ToLower()))
                {
                    if (provider.ProviderId == 0)
                    {
                        clinicalcaseOperations.AddProvider(provider);
                        TempData["Success"] = "Provider \"" + provider.Name + "\" Added Successfully!";
                    }
                    else
                    {
                        clinicalcaseOperations.UpdateProvider(provider); // Update
                        TempData["Success"] = "Provider \"" + provider.Name + "\" Updated Successfully!";
                    }
                }
                else
                {
                    TempData["Error"] = "The Provider \"" + provider.Name + "\" is already present in our Provider list!";
                }
            }
            return RedirectToAction("SettingsProvider");
        }

        [HttpGet]
        public IActionResult SettingsProvider()
        {
            List<Provider> lstProvider = new List<Provider>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstProvider = clinicalcaseOperations.GetProviders();
            ViewBag.lstProvider = lstProvider;
            return View();
        }

        [HttpGet]
        public ActionResult Add_EditProvider(int id = 0)
        {
            Provider obj = new Provider();
            if (id != 0)
            {
                List<Provider> lstProvider = new List<Provider>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstProvider = clinicalcaseOperations.GetProviders();
                var res = lstProvider.Where(a => a.ProviderId == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_AddEditProvider", obj);
        }

        [HttpGet]
        public IActionResult DeleteProvider(int id)
        {
            Provider obj = new Provider();
            if (id != 0)
            {
                List<Provider> lstProvider = new List<Provider>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstProvider = clinicalcaseOperations.GetProviders();
                var res = lstProvider.Where(a => a.ProviderId == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_DeleteProvider", obj);
        }

        [HttpPost]
        public IActionResult DeleteProvider(Provider provider)
        {
            try
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (provider.ProviderId != 0)
                    clinicalcaseOperations.DeleteProvider(provider); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("SettingsProvider");
        }


        [HttpPost]
        public IActionResult AddSettingsPayor(Payor payor)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<string> lstPayor = clinicalcaseOperations.GetPayorNames();
            if (!lstPayor.Contains(payor.Name.ToLower()))
            {
                if (payor.PayorId == 0)
                {
                    clinicalcaseOperations.AddPayor(payor);
                    TempData["Success"] = "Payor \"" + payor.Name + "\" Added Successfully!";
                }
                else
                {
                    clinicalcaseOperations.UpdatePayor(payor); // Update
                    TempData["Success"] = "Payor \"" + payor.Name + "\" Updated Successfully!";
                }
            }
            else
            {
                TempData["Error"] = "The Payor \"" + payor.Name + "\" is already present in our Payor list!";
            }
            return RedirectToAction("SettingsPayor");
        }


        [HttpGet]
        public ActionResult Add_EditPayor(int id = 0)
        {
            Payor obj = new Payor();
            if (id != 0)
            {
                List<Payor> lstPayor = new List<Payor>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstPayor = clinicalcaseOperations.GetPayors();
                var res = lstPayor.Where(a => a.PayorId == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_AddEditPayor", obj);
        }

        [HttpGet]
        public IActionResult DeletePayor(int id)
        {
            Payor obj = new Payor();
            if (id != 0)
            {
                List<Payor> lstPayor = new List<Payor>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstPayor = clinicalcaseOperations.GetPayors();
                var res = lstPayor.Where(a => a.PayorId == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_DeletePayor", obj);
        }

        [HttpPost]
        public IActionResult DeletePayor(Payor payor)
        {
            try
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (payor.PayorId != 0)
                    clinicalcaseOperations.DeletePayor(payor); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("SettingsPayor");
        }

        [HttpGet]
        public IActionResult SettingsPayor()
        {
            List<Payor> lstPayor = new List<Payor>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstPayor = clinicalcaseOperations.GetPayors();
            ViewBag.lstPayor = lstPayor;
            return View();
        }

        [HttpPost]
        public IActionResult AddSettingsErrorType(ErrorType errorType)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<string> lstErrorType = clinicalcaseOperations.GetErrorTypeNames();
            if (!lstErrorType.Contains(errorType.Name.ToLower()))
            {
                if (errorType.ErrorTypeId == 0)
                {
                    clinicalcaseOperations.AddErrorType(errorType);
                    TempData["Success"] = "Error Type \"" + errorType.Name + "\" Added Successfully!";
                }
                else
                {
                    clinicalcaseOperations.UpdateErrorType(errorType); // Update
                    TempData["Success"] = "Error Type \"" + errorType.Name + "\" Updated Successfully!";
                }
            }
            else
            {
                TempData["Error"] = "The Error Type \"" + errorType.Name + "\" is already present in our Error Type list!";
            }
            return RedirectToAction("SettingsErrorType");
        }


        [HttpGet]
        public ActionResult Add_EditErrorType(int id = 0)
        {
            ErrorType obj = new ErrorType();
            if (id != 0)
            {
                List<ErrorType> lstErrorType = new List<ErrorType>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstErrorType = clinicalcaseOperations.GetErrorTypes();
                var res = lstErrorType.Where(a => a.ErrorTypeId == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_AddEditErrorType", obj);
        }

        [HttpGet]
        public IActionResult DeleteErrorType(int id)
        {
            ErrorType obj = new ErrorType();
            if (id != 0)
            {
                List<ErrorType> lstErrorType = new List<ErrorType>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstErrorType = clinicalcaseOperations.GetErrorTypes();
                var res = lstErrorType.Where(a => a.ErrorTypeId == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_DeleteErrorType", obj);
        }

        [HttpPost]
        public IActionResult DeleteErrorType(ErrorType errorType)
        {
            try
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (errorType.ErrorTypeId != 0)
                    clinicalcaseOperations.DeleteErrorType(errorType); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("SettingsErrorType");
        }

        [HttpGet]
        public IActionResult SettingsErrorType()
        {
            List<ErrorType> lstErrorType = new List<ErrorType>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstErrorType = clinicalcaseOperations.GetErrorTypes();
            ViewBag.lstErrorType = lstErrorType;
            return View();
        }

        [HttpPost]
        public IActionResult AddSettingsProviderFeedback(BindDTO providerFeedback)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<string> lstFeedback = clinicalcaseOperations.GetProviderFeedbackNames();
            if (!lstFeedback.Contains(providerFeedback.Name.ToLower()))
            {
                if (providerFeedback.ID == 0)
                {
                    clinicalcaseOperations.AddProviderFeedback(providerFeedback);
                    TempData["Success"] = "Provider Feedback \"" + providerFeedback.Name + "\" Added Successfully!";
                }
                else
                {
                    clinicalcaseOperations.UpdateProviderFeedback(providerFeedback); // Update
                    TempData["Success"] = "Provider Feedback \"" + providerFeedback.Name + "\" Updated Successfully!";
                }
            }
            else
            {
                TempData["Error"] = "The Provider Feedback \"" + providerFeedback.Name + "\" is already present in our Provider feedback list!";
            }
            return RedirectToAction("SettingsProviderFeedback");
        }
        [HttpGet]
        public IActionResult SettingsProviderFeedback()
        {
            List<BindDTO> lstProviderFeedback = new List<BindDTO>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstProviderFeedback = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.lstProviderFeedback = lstProviderFeedback;
            return View();
        }

        [HttpGet]
        public ActionResult Add_EditProviderFeedback(int id = 0)
        {
            BindDTO obj = new BindDTO();
            if (id != 0)
            {
                List<BindDTO> lstproviderFeedback = new List<BindDTO>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstproviderFeedback = clinicalcaseOperations.GetProviderFeedbacksList();
                var res = lstproviderFeedback.Where(a => a.ID == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_AddEditProviderFeedback", obj);
        }

        [HttpGet]
        public IActionResult DeleteProviderFeedback(int id)
        {
            BindDTO obj = new BindDTO();
            if (id != 0)
            {
                List<BindDTO> lstproviderFeedback = new List<BindDTO>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstproviderFeedback = clinicalcaseOperations.GetProviderFeedbacksList();
                var res = lstproviderFeedback.Where(a => a.ID == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_DeleteProviderFeedback", obj);
        }

        [HttpPost]
        public IActionResult DeleteProviderFeedback(BindDTO providerFeedback)
        {
            try
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (providerFeedback.ID != 0)
                    clinicalcaseOperations.DeleteProviderFeedback(providerFeedback); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("SettingsProviderFeedback");
        }

        [HttpGet]
        public IActionResult SettingsProject()
        {
            List<ApplicationProject> lstProject = new List<ApplicationProject>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstProject = clinicalcaseOperations.GetProjects();
            ViewBag.lstProject = lstProject;
            return View();
        }

        [HttpPost]
        public IActionResult AddSettingsProject(ApplicationProject project)
        {
            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                List<string> lstProvider = clinicalcaseOperations.GetProjectNames();

                if (project.ProjectId == 0)
                {
                    if (!lstProvider.Contains(project.Name.ToLower()))
                    {
                        clinicalcaseOperations.AddProject(project);
                        TempData["Success"] = "Provider \"" + project.Name + "\" Added Successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "The Provider \"" + project.Name + "\" is already present in our Provider list!";
                    }
                }
                else
                {
                    clinicalcaseOperations.UpdateProject(project); // Update
                    TempData["Success"] = "Provider \"" + project.Name + "\" Updated Successfully!";
                }
            }
            return RedirectToAction("SettingsProject");
        }

        [HttpGet]
        public ActionResult Add_EditProject(int id = 0)
        {
            ApplicationProject obj = new ApplicationProject();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Clients = clinicalcaseOperations.GetClientList();
            ViewBag.ProjectTypes = clinicalcaseOperations.GetProjectTypeList();
            if (id != 0)
            {
                List<ApplicationProject> lstProject = new List<ApplicationProject>();
                lstProject = clinicalcaseOperations.GetProjects();
                var res = lstProject.Where(a => a.ProjectId == id).FirstOrDefault();
                obj = res;
                return PartialView("_AddEditProject", obj);
            }
            return PartialView("_AddEditProject", obj);
        }

        [HttpPost]
        public IActionResult DeleteProject(ApplicationProject project)
        {
            try
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (project.ProjectId != 0)
                    clinicalcaseOperations.DeleteProject(project);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("SettingsProject");
        }

        [HttpGet]
        public IActionResult DeleteProject(int id)
        {
            ApplicationProject obj = new ApplicationProject();
            if (id != 0)
            {
                List<ApplicationProject> lstProject = new List<ApplicationProject>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstProject = clinicalcaseOperations.GetProjects();
                var res = lstProject.Where(a => a.ProjectId == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_DeleteProject", obj);
        }

        [HttpGet]
        public IActionResult SettingsFileUpload()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();
            return View();
        }

        [HttpPost]
        public IActionResult SettingsFileUpload(IFormFile files, int projectid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            try
            {
                string uploadedFile = Path.Combine(@"D:\\");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + files.FileName;
                string filePath = Path.Combine(uploadedFile, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    files.CopyTo(stream);
                    clinicalcaseOperations.UploadAndSave(stream, projectid);
                }
                TempData["Success"] = "Data uploaded Successfully!";
                ViewBag.Projects = clinicalcaseOperations.GetProjectsList();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Projects = clinicalcaseOperations.GetProjectsList();
                TempData["error"] = ex.Message;
                return View();
            }
        }

        #endregion
    }
}
