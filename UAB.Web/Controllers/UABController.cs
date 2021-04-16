﻿using System;
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
        private int mUserId;
        private string mUserRole;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UABController(IHttpContextAccessor httpContextAccessor)
        {
            mUserId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value);
            mUserRole = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Coding
        public IActionResult CodingSummary()
        {
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
            ChartSummaryDTO chartSummary = new ChartSummaryDTO();
            chartSummary = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummary.ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            #endregion
            if (chartSummary.CodingDTO.ClinicalCaseID == 0)
            {
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("CodingSummary");
            }
            return View("Coding", chartSummary);
        }
        [HttpGet]
        public IActionResult GetBlockedChartsList(string Role, string ChartType, int ProjectID)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            lst = clinicalcaseOperations.GetBlockNext(Role, ChartType, ProjectID);
            ViewBag.Role = Role;
            return PartialView("_BlockedList", lst);
        }

        [HttpGet]
        public IActionResult ViewHistory(string ccid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var reslut = clinicalcaseOperations.GetWorkflowHistories(ccid);
            return PartialView("_ViewHistory", reslut);
        }
        [HttpGet]
        public IActionResult BlockHistory(string name, string remarks, string createdate)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            BlockDTO dt = new BlockDTO()
            {
                Name = name,
                Remarks = remarks,
                CreateDate = Convert.ToDateTime(createdate)
            };
            return PartialView("_BlockHistory", dt);
        }


        [HttpGet]
        public IActionResult GetCodingBlockedCharts(string Role, string ChartType, int ProjectID, string ProjectName, string ccid, string plusorminus)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> chartSummaryDTOlst = new List<ChartSummaryDTO>();

            chartSummaryDTOlst = clinicalcaseOperations.GetBlockNext(Role, ChartType, ProjectID);

            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            switch (plusorminus)
            {
                case "Next":
                    chartSummaryDTO = chartSummaryDTOlst.SkipWhile(x => !x.CodingDTO.ClinicalCaseID.Equals(Convert.ToInt32(ccid))).Skip(1).FirstOrDefault();
                    if (chartSummaryDTO == null)
                    {
                        chartSummaryDTO = chartSummaryDTOlst.Where(c => c.CodingDTO.ClinicalCaseID == Convert.ToInt32(ccid)).FirstOrDefault();
                    }
                    break;
                case "Previous":
                    chartSummaryDTO = chartSummaryDTOlst.TakeWhile(x => !x.CodingDTO.ClinicalCaseID.Equals(Convert.ToInt32(ccid))).Skip(1).LastOrDefault();
                    if (chartSummaryDTO == null)
                    {
                        chartSummaryDTO = chartSummaryDTOlst.Where(c => c.CodingDTO.ClinicalCaseID == Convert.ToInt32(ccid)).FirstOrDefault();
                    }
                    break;

                default:
                    chartSummaryDTO = chartSummaryDTOlst.Where(c => c.CodingDTO.ClinicalCaseID == Convert.ToInt32(ccid)).FirstOrDefault();
                    break;
            }

            ViewBag.IsBlocked = "1";
            ViewBag.Postionindex = 0;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion

            if (Role == Roles.QA.ToString())
                return View("QA", chartSummaryDTO);
            else if (Role == "ShadowQA")
                return View("ShadowQA", chartSummaryDTO);
            else
                return View("Coding", chartSummaryDTO);

        }

        public IActionResult GetCodingBlockedChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
            chartSummaryDTO.ProjectName = ProjectName;
            ViewBag.IsBlocked = "1";

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            if (Role == "QA")
                return View("QA", chartSummaryDTO);
            else if (Role == "ShadowQA")
                return View("ShadowQA", chartSummaryDTO);
            else
                return View("Coding", chartSummaryDTO);

        }
        [HttpGet]
        public IActionResult BlockClinicalcase(string ccid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.BlockCategories = clinicalcaseOperations.GetBlockCategories();
            ViewBag.ccid = Convert.ToInt32(ccid);
            ViewBag.statusid = clinicalcaseOperations.GetStatusId(ccid);
            return PartialView("_BlockCategory");
        }
        [HttpPost]
        public IActionResult BlockClinicalcase(string ccid, string bid, string remarks)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            if (ccid != null && bid != null && remarks != null)
            {
                clinicalcaseOperations.BlockClinicalcase(ccid, bid, remarks);
            }
            return RedirectToAction("CodingSummary");
        }

        public IActionResult GetCodingIncorrectChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lstchartSummary = new List<ChartSummaryDTO>();
            lstchartSummary = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            lstchartSummary.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion

            return View("IncorrectCharts", lstchartSummary);
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
            if (chartSummaryDTO.CodingDTO.ClinicalCaseID == 0)
            {
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("CodingSummary");
            }
            return View("ReadyForPostingChart", chartSummaryDTO);
        }

        private void PrepareClaim(string basicParams, string dx, string cpt, int rno, ref DataTable dtClaim, ref DataTable dtCpt)
        {
            string[] lstbasicParams = basicParams.Split("^");
            dtClaim.Rows.Add(rno, lstbasicParams[0], lstbasicParams[1], lstbasicParams[2], lstbasicParams[3], dx);
            PrepareCpt(cpt, dtCpt, rno);
        }

        private void PrepareCpt(string cpt, DataTable dtCPT, int rno)
        {
            string[] lstcpts = cpt.Split("|");
            foreach (var item in lstcpts)
            {
                string[] lstcptrow = item.Split("^");
                dtCPT.Rows.Add(rno, lstcptrow[0], lstcptrow[1], lstcptrow[2], lstcptrow[3]);
            }
        }
        public IActionResult SubmitCodingAvailableChart(ChartSummaryDTO chartSummaryDTO, string codingSubmitAndGetNext, string submitAndPost, string submitOnly)
        {
            string submitType = Request.Form["hdnSubmitAndPost"];
            string hdnIsAuditRequired = Request.Form["hdnIsAuditRequired"];

            if (submitType == "submitAndPost")
                chartSummaryDTO.SubmitAndPostAlso = true;
            else
                chartSummaryDTO.SubmitAndPostAlso = false;

            if (hdnIsAuditRequired == "true")
                chartSummaryDTO.IsAuditRequired = true;
            else
                chartSummaryDTO.IsAuditRequired = false;

            if (string.IsNullOrEmpty(codingSubmitAndGetNext))
                codingSubmitAndGetNext = Request.Form["hdnButtonType"];

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            string hdnDxCodes = Request.Form["hdnDxCodes"].ToString();
            chartSummaryDTO.Dx = hdnDxCodes;
            string hdnCptCodes = Request.Form["hdnCptCodes"].ToString();
            chartSummaryDTO.CPTCode = hdnCptCodes;

            string hdnClaim1 = Request.Form["hdnClaim2"].ToString();
            string hdnDxClaim1 = Request.Form["hdnDxCodes1"].ToString();
            string hdnCptClaim1 = Request.Form["hdnCptCodes1"].ToString();

            DataTable dtClaim = new DataTable();
            dtClaim.Columns.Add("RNO", typeof(int));
            dtClaim.Columns.Add("ProviderId", typeof(int));
            dtClaim.Columns.Add("PayorId", typeof(int));
            dtClaim.Columns.Add("NoteTitle", typeof(string));
            dtClaim.Columns.Add("ProviderFeedbackId", typeof(string));
            dtClaim.Columns.Add("Dx", typeof(string));

            DataTable dtCpt = new DataTable();
            dtCpt.Columns.Add("RNO", typeof(int));
            dtCpt.Columns.Add("CPTCode", typeof(string));
            dtCpt.Columns.Add("Mod", typeof(string));
            dtCpt.Columns.Add("Qty", typeof(string));
            dtCpt.Columns.Add("Links", typeof(string));
            if (!string.IsNullOrEmpty(hdnClaim1))
                PrepareClaim(hdnClaim1, hdnDxClaim1, hdnCptClaim1, 1, ref dtClaim, ref dtCpt);

            string hdnClaim2 = Request.Form["hdnClaim3"].ToString();
            string hdnDxClaim2 = Request.Form["hdnDxCodes2"].ToString();
            string hdnCptClaim2 = Request.Form["hdnCptCodes2"].ToString();

            if (!string.IsNullOrEmpty(hdnClaim2))
                PrepareClaim(hdnClaim2, hdnDxClaim2, hdnCptClaim2, 2, ref dtClaim, ref dtCpt);

            string hdnClaim3 = Request.Form["hdnClaim4"].ToString();
            string hdnDxClaim3 = Request.Form["hdnDxCodes3"].ToString();
            string hdnCptClaim3 = Request.Form["hdnCptCodes3"].ToString();
            if (!string.IsNullOrEmpty(hdnClaim3))
                PrepareClaim(hdnClaim3, hdnDxClaim3, hdnCptClaim3, 3, ref dtClaim, ref dtCpt);

            if (codingSubmitAndGetNext == "codingSubmit")
                clinicalcaseOperations.SubmitCodingAvailableChart(chartSummaryDTO, dtClaim, dtCpt);
            else
            {
                clinicalcaseOperations.SubmitCodingAvailableChart(chartSummaryDTO, dtClaim, dtCpt);
                return RedirectToAction("GetCodingAvailableChart", new { Role = Roles.Coder.ToString(), ChartType = "Available", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }
            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());
            TempData["Success"] = "Chart Details submitted successfully !";
            return View("CodingSummary", lstDto);
        }
        public IActionResult codingSubmitPopup(ChartSummaryDTO chartSummaryDTO, string buttonType)
        {
            ViewBag.buttonType = buttonType;
            return PartialView("_CodingSubmitPopup", chartSummaryDTO);
        }
        public IActionResult GetAuditDetails(string chartType, int projectId)
        {
            bool auditFlag = IsAuditRequired(chartType, projectId);
            return new JsonResult(auditFlag);
        }
        public bool IsAuditRequired(string chartType, int projectId)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            int samplePercentage = clinicalcaseOperations.GetSamplingPercentage(mUserId, chartType, projectId);

            if (samplePercentage > 0)
            {
                string auditCookie = _httpContextAccessor.HttpContext.Request.Cookies[chartType + mUserId];

                if (auditCookie == null || auditCookie.Split(",")[0] != DateTime.Now.ToString("MM/dd/yyyy"))
                {
                    CookieOptions option = new CookieOptions();
                    option.Expires = new DateTimeOffset(DateTime.Now.AddYears(10));
                    Response.Cookies.Append(chartType + mUserId, DateTime.Now.ToString("MM/dd/yyyy") + ",1-10:1~11-20:0~21-30:0~31-40:0~41-50:0~51-60:0~61-70:0,1", option);
                    return true;
                }
                else
                {
                    if (auditCookie.Split(",")[0] == DateTime.Now.ToString("MM/dd/yyyy"))
                    {
                        int currentChart = Convert.ToInt32(auditCookie.Split(",")[2]) + 1;

                        string[] arrAuditDetails = auditCookie.Split(",")[1].Split("~");

                        foreach (var item in arrAuditDetails)
                        {
                            int auditedCharts = Convert.ToInt32(item.Substring(item.IndexOf(":") + 1));
                            int startIndex = Convert.ToInt32(item.Substring(0, item.IndexOf("-")));
                            int lastIndex = Convert.ToInt32(item.Substring(item.IndexOf("-") + 1, item.IndexOf(":") - item.IndexOf("-") - 1));

                            if (currentChart >= startIndex && currentChart <= lastIndex)
                            {
                                if (samplePercentage / 10 > auditedCharts)
                                {
                                    Random r = new Random();
                                    int genRand = r.Next(startIndex, lastIndex);
                                    if (currentChart >= genRand)
                                    {
                                        //1-10:1
                                        string cookieValue = auditCookie.Split(",")[1].Replace(startIndex + "-" + lastIndex + ":" + auditedCharts, startIndex + "-" + lastIndex + ":" + auditedCharts + 1);

                                        CookieOptions option = new CookieOptions();
                                        int nextChart = currentChart;
                                        option.Expires = new DateTimeOffset(DateTime.Now.AddYears(10));
                                        Response.Cookies.Append(chartType + mUserId, DateTime.Now.ToString("MM/dd/yyyy") + "," + cookieValue + "," + nextChart, option);
                                        return true;
                                    }
                                }
                                CookieOptions option1 = new CookieOptions();
                                int nextChart1 = currentChart;
                                option1.Expires = new DateTimeOffset(DateTime.Now.AddYears(10));
                                Response.Cookies.Append(chartType + mUserId, DateTime.Now.ToString("MM/dd/yyyy") + "," + auditCookie.Split(",")[1] + "," + nextChart1, option1);
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        void PrepareAudit(string rejectedFields, DataTable dtAudit)
        {
            string[] items = rejectedFields.Split("^");
            int claimId = Convert.ToInt32(items[0].Split(",")[1]);
            for (int i = 1; i < items.Count(); i++)
            {
                dtAudit.Rows.Add(items[i].Split(",")[0], items[i].Split(",")[1], items[i].Split(",")[2], claimId);
            }

        }
        public IActionResult SubmitCodingIncorrectChart(ChartSummaryDTO chartSummaryDTO)
        {
            var hdnPayorID = Request.Form["hdnPayorID"].ToString();
            var hdnProviderID = Request.Form["hdnProviderID"].ToString();
            var hdnProviderFeedbackID = Request.Form["hdnProviderFeedbackID"].ToString();
            var hdnStatusId = Request.Form["hdnStatusId"].ToString();
            string hdnClaim2 = Request.Form["hdnClaim2"].ToString();
            string hdnClaim3 = Request.Form["hdnClaim3"].ToString();
            string hdnClaim4 = Request.Form["hdnClaim4"].ToString();

            // Default Claim 

            var hdnRejectedDxRemarks = Request.Form["hdnRejectedDxRemarks"].ToString();
            chartSummaryDTO.RevisedDXRemarks = hdnRejectedDxRemarks;

            var hdnRejectedDxCodes = Request.Form["hdnRejectedDxCodes"].ToString();
            chartSummaryDTO.RejectedDx = hdnRejectedDxCodes;

            var hdnDxCodes = Request.Form["hdnDxCodes"].ToString();
            chartSummaryDTO.Dx = hdnDxCodes;

            var hdnCptCodes = Request.Form["hdnCptCodes"].ToString();

            var hdnRejectedCptRemarks = Request.Form["hdnRejectedCptRemarks"].ToString();
            chartSummaryDTO.RevisedCPTRemarks = hdnRejectedCptRemarks;

            var hdnRejectedCptCodes = Request.Form["hdnRejectedCptCodes"].ToString();
            chartSummaryDTO.RejectedCpt = hdnRejectedCptCodes;

            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ClaimId", typeof(int));

            DataTable dtCpt = new DataTable();
            dtCpt.Columns.Add("CPTCode", typeof(string));
            dtCpt.Columns.Add("Mod", typeof(string));
            dtCpt.Columns.Add("Qty", typeof(string));
            dtCpt.Columns.Add("Links", typeof(string));
            dtCpt.Columns.Add("ClaimId", typeof(int));

            // Claim 1

            var hdnDxCodes1 = Request.Form["hdnDxCodes1"].ToString();
            var hdnRejectedDxRemarks1 = Request.Form["hdnRejectedDxRemarks1"].ToString();
            var hdnRejectedDxCodes1 = Request.Form["hdnRejectedDxCodes1"].ToString();

            var hdnCptCodes1 = Request.Form["hdnCptCodes1"].ToString();
            var hdnRejectedCptRemarks1 = Request.Form["hdnRejectedCptRemarks1"].ToString();
            var hdnRejectedCptCodes1 = Request.Form["hdnRejectedCptCodes1"].ToString();

            // Claim 2

            var hdnDxCodes2 = Request.Form["hdnDxCodes2"].ToString();
            var hdnRejectedDxRemarks2 = Request.Form["hdnRejectedDxRemarks2"].ToString();
            var hdnRejectedDxCodes2 = Request.Form["hdnRejectedDxCodes2"].ToString();

            var hdnCptCodes2 = Request.Form["hdnCptCodes2"].ToString();
            var hdnRejectedCptRemarks2 = Request.Form["hdnRejectedCptRemarks2"].ToString();
            var hdnRejectedCptCodes2 = Request.Form["hdnRejectedCptCodes2"].ToString();

            // Claim 3

            var hdnDxCodes3 = Request.Form["hdnDxCodes3"].ToString();
            var hdnRejectedDxRemarks3 = Request.Form["hdnRejectedDxRemarks3"].ToString();
            var hdnRejectedDxCodes3 = Request.Form["hdnRejectedDxCodes3"].ToString();

            var hdnCptCodes3 = Request.Form["hdnCptCodes3"].ToString();
            var hdnRejectedCptRemarks3 = Request.Form["hdnRejectedCptRemarks3"].ToString();
            var hdnRejectedCptCodes3 = Request.Form["hdnRejectedCptCodes3"].ToString();

            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            if (!string.IsNullOrEmpty(hdnRejectedCptCodes1))
                dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes1, hdnRejectedCptRemarks1, Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnRejectedCptCodes2))
                dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes2, hdnRejectedCptRemarks2, Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnRejectedCptCodes3))
                dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes3, hdnRejectedCptRemarks3, Convert.ToInt32(hdnClaimId4));

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes1))
                dtAudit.Rows.Add("Dx", hdnRejectedDxCodes1, hdnRejectedDxRemarks1, Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes2))
                dtAudit.Rows.Add("Dx", hdnRejectedDxCodes2, hdnRejectedDxRemarks2, Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes3))
                dtAudit.Rows.Add("Dx", hdnRejectedDxCodes3, hdnRejectedDxRemarks3, Convert.ToInt32(hdnClaimId4));


            if (!string.IsNullOrEmpty(hdnClaim2))
                PrepareAudit(hdnClaim2, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim3))
                PrepareAudit(hdnClaim3, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim4))
                PrepareAudit(hdnClaim4, dtAudit);

            if (!string.IsNullOrEmpty(hdnCptCodes1))
                PrepareCpt(hdnCptCodes1, dtCpt, Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnCptCodes2))
                PrepareCpt(hdnCptCodes2, dtCpt, Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnCptCodes3))
                PrepareCpt(hdnCptCodes3, dtCpt, Convert.ToInt32(hdnClaimId4));

            if (!string.IsNullOrEmpty(hdnPayorID))
                chartSummaryDTO.PayorID = Convert.ToInt32(hdnPayorID);

            if (!string.IsNullOrEmpty(hdnProviderID))
                chartSummaryDTO.ProviderID = Convert.ToInt32(hdnProviderID);

            if (!string.IsNullOrEmpty(hdnCptCodes))
                chartSummaryDTO.CPTCode = hdnCptCodes;

            //if (!string.IsNullOrEmpty(hdnDx))
            //    chartSummaryDTO.Dx = hdnDx;

            if (!string.IsNullOrEmpty(hdnProviderFeedbackID))
                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(hdnProviderFeedbackID);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitCodingIncorrectChart(chartSummaryDTO, Convert.ToInt16(hdnStatusId), dtAudit);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("CodingSummary", lstDto);
        }
        public IActionResult SubmitCodingReadyForPostingChart(ChartSummaryDTO chartSummaryDTO, string postingSubmitAndGetNext)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            if (string.IsNullOrEmpty(postingSubmitAndGetNext))
                clinicalcaseOperations.SubmitCodingReadyForPostingChart(chartSummaryDTO);
            else
            {
                clinicalcaseOperations.SubmitCodingReadyForPostingChart(chartSummaryDTO);
                return RedirectToAction("GetCodingReadyForPostingChart", new { Role = Roles.Coder.ToString(), ChartType = "ReadyForPosting", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }
            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            TempData["Success"] = "Chart Details posted successfully !";
            return View("CodingSummary", lstDto);
        }


        [HttpGet]
        public IActionResult AddNewCliam(int cliamID)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.CliamId = cliamID;
            if (cliamID == 2)
            {
                ViewBag.DxCliam = "txtDx2Cliam" + "_1";
                ViewBag.CptCliam = "txt2Cpt" + "_1";
                ViewBag.CptModCliam = "txt2mod" + "_1";
                ViewBag.CptQtyCliam = "txt2qty" + "_1";
                ViewBag.CptLinksCliam = "txt2links" + "_1";

                ViewBag.dx2index = "dx2index";
                ViewBag.Dx2Cliam = "txt2Dx";
                ViewBag.Div2DxRow = "Div2DxRow_1";

                ViewBag.cpt2index = "cpt2index";
                ViewBag.Cpt2Link = "txt2Link";
                ViewBag.Div2CptRow = "Div2CptRow_1";
            }
            if (cliamID == 3)
            {
                ViewBag.DxCliam = "txtDx3Cliam" + "_1";
                ViewBag.CptCliam = "txt3Cpt" + "_1";
                ViewBag.CptModCliam = "txt3mod" + "_1";
                ViewBag.CptQtyCliam = "txt3qty" + "_1";
                ViewBag.CptLinksCliam = "txt3links" + "_1";

                ViewBag.dx2index = "dx3index";
                ViewBag.Dx2Cliam = "txt3Dx";
                ViewBag.Div2DxRow = "Div3DxRow_1";

                ViewBag.cpt2index = "cpt3index";
                ViewBag.Cpt2Link = "txt3Link";
                ViewBag.Div2CptRow = "Div3CptRow_1";
            }
            if (cliamID == 4)
            {
                ViewBag.DxCliam = "txtDx4Cliam" + "_1";
                ViewBag.CptCliam = "txt4Cpt" + "_1";
                ViewBag.CptModCliam = "txt4mod" + "_1";
                ViewBag.CptQtyCliam = "txt4qty" + "_1";
                ViewBag.CptLinksCliam = "txt4links" + "_1";

                ViewBag.dx2index = "dx4index";
                ViewBag.Dx2Cliam = "txt4Dx";
                ViewBag.Div2DxRow = "Div4DxRow_1";

                ViewBag.cpt2index = "cpt4index";
                ViewBag.Cpt2Link = "txt4Link";
                ViewBag.Div2CptRow = "Div4CptRow_1";
            }

            return PartialView("_CodingCliam");
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
            List<ChartSummaryDTO> lstchartSummary = new List<ChartSummaryDTO>();
            lstchartSummary = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            lstchartSummary.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            if (lstchartSummary.FirstOrDefault().CodingDTO.ClinicalCaseID == 0)
            {
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("QASummary");
            }
            return View("QA", lstchartSummary.OrderBy(a => a.ClaimId).ToList());
        }

        public IActionResult GetQARebuttalChartsOfCoder(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lstchartSummary = new List<ChartSummaryDTO>();
            lstchartSummary = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            lstchartSummary.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            return View("QARebuttalChartsOfCoder", lstchartSummary.OrderBy(a => a.ClaimId).ToList());
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
        //public IActionResult GetQAOnHoldChart(string Role, string ChartType, int ProjectID, string ProjectName)
        //{
        //    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
        //    ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
        //    chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
        //    chartSummaryDTO.ProjectName = ProjectName;

        //    #region binding data
        //    ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
        //    ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
        //    ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
        //    #endregion

        //    return View("OnHold", chartSummaryDTO);
        //}
        public IActionResult SubmitQAAvailableChart(ChartSummaryDTO chartSummaryDTO, string SubmitAndGetNext)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            var hdnQADx = Request.Form["hdnQADxCodes"].ToString();
            var hdnQADxRemarks = Request.Form["hdnQADxRemarks"].ToString();
            chartSummaryDTO.QADx = hdnQADx;
            chartSummaryDTO.QADxRemarks = hdnQADxRemarks;

            var hdnQACptCodes = Request.Form["hdnQACptCodes"].ToString();
            var hdnQACptRemarks = Request.Form["hdnQACptRemarks"].ToString();
            chartSummaryDTO.QACPTCode = hdnQACptCodes;
            chartSummaryDTO.QACPTCodeRemarks = hdnQACptRemarks;

            bool audit = IsAuditRequired("QA", chartSummaryDTO.ProjectID);
            chartSummaryDTO.IsAuditRequired = audit;

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
            var hdnDxRemarks = Request.Form["hdnDxRemarks"].ToString();
            var hdnCptRemarks = Request.Form["hdnCptRemarks"].ToString();
            chartSummaryDTO.QADxRemarks = hdnDxRemarks;
            chartSummaryDTO.QACPTCodeRemarks = hdnCptRemarks;
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
            var hdnCpt = Request.Form["hdnCPT"].ToString();
            var hdnMod = Request.Form["hdnMod"].ToString();
            var hdnDx = Request.Form["hdnDx"].ToString();
            var hdnDxRemarks = Request.Form["hdnDxRemarks"].ToString();
            chartSummaryDTO.QADxRemarks = hdnDxRemarks;
            var hdnCPTRemarks = Request.Form["hdnCPTRemarks"].ToString();
            chartSummaryDTO.QACPTCodeRemarks = hdnCPTRemarks;
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

            if (chartSummaryDTO.CodingDTO.ClinicalCaseID == 0)
            {
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("ShadowQASummary");
            }
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

            var hdnShadowQADxCodes = Request.Form["hdnShadowQADxCodes"].ToString();
            var hdnShadowQADxRemarks = Request.Form["hdnShadowQADxRemarks"].ToString();
            chartSummaryDTO.ShadowQADx = hdnShadowQADxCodes;
            chartSummaryDTO.ShadowQADxRemarks = hdnShadowQADxRemarks;

            var hdnShadowQACptCodes = Request.Form["hdnShadowQACptCodes"].ToString();
            var hdnShadowQACptRemarks = Request.Form["hdnShadowQACptRemarks"].ToString();
            chartSummaryDTO.ShadowQACPTCode = hdnShadowQACptCodes;
            chartSummaryDTO.ShadowQACPTCodeRemarks = hdnShadowQACptRemarks;


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

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.ShadowQA.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("ShadowQASummary", lstDto);
        }
        #endregion

        #region Settings

        public List<BindDTO> BindErrorType()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ErrorType> lstErrorType = clinicalcaseOperations.GetErrorTypes();

            List<BindDTO> lstDto = new List<BindDTO>();

            foreach (var err in lstErrorType)
            {
                lstDto.Add(new BindDTO() { ID = err.ErrorTypeId, Name = err.Name });
            }
            return lstDto;
        }

        [HttpGet]
        public IActionResult AssignClinicalCaseToUser(string ccid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            string assignedusername = clinicalcaseOperations.GetAssignedusername(ccid);
            var assignusers = clinicalcaseOperations.GetAssignedToUsers(ccid);



            ViewBag.assignusers = assignusers;
            ViewBag.ccid = ccid;
            SearchResultDTO SearchResultDTO = new SearchResultDTO();
            SearchResultDTO.ClinicalCaseId = ccid;
            SearchResultDTO.AssignFromUserEmail = assignedusername;
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
        public IActionResult SettingsSearch(string ccid, string fname, string lname, string mrn, DateTime dosfrom, DateTime dosto, string statusname, string projectname, string providername, bool includeblocked)
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
                ProviderName = providername,
                IncludeBlocked = includeblocked
            };
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId, mUserRole);
            var searchData = clinicalcaseOperations.GetSearchData(searchParametersDTO);
            return PartialView("_SettingsSearchResults", searchData);
        }
        [HttpPost]

        [HttpGet]
        public IActionResult SettingsBlockCategories()
        {
            List<BlockCategory> lstblock = new List<BlockCategory>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstblock = clinicalcaseOperations.GetBlockCategories();
            ViewBag.lstblock = lstblock;
            return View();
        }
        [HttpGet]
        public ActionResult Add_EditBlockCategories(int id = 0)
        {
            BlockCategory obj = new BlockCategory();
            if (id != 0)
            {
                List<BlockCategory> lstProvider = new List<BlockCategory>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstProvider = clinicalcaseOperations.GetBlockCategories();
                var res = lstProvider.Where(a => a.BlockCategoryId == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_AddEditBlockCategory", obj);
        }
        [HttpPost]
        public IActionResult AddSettingsBlockCategory(BlockCategory category)
        {
            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                List<BlockCategory> lstblock = clinicalcaseOperations.GetBlockCategories();
                if (lstblock.Where(a => !a.Name.Contains(category.Name)).Any())
                {
                    if (category.BlockCategoryId == 0)
                    {
                        clinicalcaseOperations.AddBlockCategory(category);
                        TempData["Success"] = "Block Category \"" + category.Name + "\" Added Successfully!";
                    }
                    else
                    {
                        clinicalcaseOperations.UpdateBlockCategory(category); // Update
                        TempData["Success"] = "Block Category \"" + category.Name + "\" Updated Successfully!";
                    }
                }
                else
                {
                    TempData["Error"] = "The Block Category \"" + category.Name + "\" is already present in our Block categories list!";
                }
            }
            return RedirectToAction("SettingsBlockCategories");
        }
        [HttpGet]
        public IActionResult DeleteBlockCategory(int id)
        {
            BlockCategory obj = new BlockCategory();
            if (id != 0)
            {
                List<BlockCategory> lstblock = new List<BlockCategory>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstblock = clinicalcaseOperations.GetBlockCategories();
                var res = lstblock.Where(a => a.BlockCategoryId == id).FirstOrDefault();
                obj = res;
            }
            return PartialView("_DeleteBlockCategory", obj);
        }

        [HttpPost]
        public IActionResult DeleteBlockCategory(BlockCategory blockCategory)
        {
            try
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (blockCategory.BlockCategoryId != 0)
                    clinicalcaseOperations.DeletetBlockCategory(blockCategory.BlockCategoryId); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("SettingsBlockCategories");
        }
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
                string fileName = files.FileName;
                string filePath = Path.Combine(uploadedFile, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    files.CopyTo(stream);
                    clinicalcaseOperations.UploadAndSave(stream, projectid, fileName);
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
