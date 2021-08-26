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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

namespace UAB.Controllers
{
    [Authorize]
    public class UABController : Controller
    {
        private int mUserId;
        private string timeZoneCookie;
        private string mUserRole;
        private ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UABController(IHttpContextAccessor httpContextAccessor, ILogger<UABController> logger)
        {
            mUserId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value != null ? Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value) : 0;
            mUserRole = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            timeZoneCookie = _httpContextAccessor.HttpContext.Request.Cookies["UAB_TimeZoneOffset"];
        }

        #region Coding
        public IActionResult CodingSummary()
        {
            _logger.LogInformation("Loading Started for CodingSummary for User: " + mUserId);
            List<DashboardDTO> lstDto = new List<DashboardDTO>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            List<int> lstStatus = new List<int> { (int)StatusType.ReadyForCoding, (int)StatusType.QARejected, (int)StatusType.ShadowQARejected,
                (int)StatusType.PostingCompleted };

            lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            _logger.LogInformation("Loading Ended for CodingSummary for User: " + mUserId);

            return View(lstDto);
        }

        public IActionResult GetCodingAvailableChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetCodingAvailableChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummary = new ChartSummaryDTO();


            chartSummary = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID, timeZoneCookie);
            chartSummary.ProjectName = ProjectName;

            AuditDTO auditDTO = clinicalcaseOperations.GetAuditInfoForCPTAndProvider(ProjectID);
            chartSummary.auditDTO = auditDTO;

            #region binding data
            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));
            #endregion

            _logger.LogInformation("Loading Ended for GetCodingAvailableChart for User: " + mUserId);

            if (chartSummary.CodingDTO.ClinicalCaseID == 0)
            {
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("CodingSummary");
            }
            var res = clinicalcaseOperations.GetBlockResponseBycid(chartSummary.CodingDTO.ClinicalCaseID);
            if (res != null)
            {
                chartSummary.BlockResponseDTO = res;
            }
            return View("Coding", chartSummary);
        }
        [HttpGet]
        public IActionResult GetBlockedChartsList(string Role, int ProjectID)
        {
            _logger.LogInformation("Loading Started for GetBlockedChartsList for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();

            lst = clinicalcaseOperations.GetBlockedChartsList(Role, ProjectID, timeZoneCookie);

            ViewBag.Role = Role;
            ViewBag.CCIDs = lst.Count > 0 ? lst.FirstOrDefault().CCIDs : null;

            _logger.LogInformation("Loading Ended for GetBlockedChartsList for User: " + mUserId);

            return PartialView("_BlockedList", lst);
        }

        [HttpGet]
        public IActionResult ViewHistory(string ccid)
        {
            _logger.LogInformation("Loading Started for ViewHistory for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var reslut = clinicalcaseOperations.GetWorkflowHistories(ccid, timeZoneCookie);

            _logger.LogInformation("Loading Ended for ViewHistory for User: " + mUserId);

            return PartialView("_ViewHistory", reslut);
        }
        [HttpPost]
        public IActionResult BlockHistoryFromAging([FromBody] List<BlockDTO> historyDto)
        {
            _logger.LogInformation("Loading Started for BlockHistory for User: " + mUserId);

            int CCId = Convert.ToInt32(Request.Form["hdnCCID"]);

            _logger.LogInformation("Loading Ended for BlockHistory for User: " + mUserId);

            return PartialView("_BlockHistory", historyDto);
        }
        [HttpPost]
        public IActionResult BlockHistory([FromBody] List<BlockDTO> historyDto)
        {
            _logger.LogInformation("Loading Started for BlockHistory for User: " + mUserId);



            _logger.LogInformation("Loading Ended for BlockHistory for User: " + mUserId);

            return PartialView("_BlockHistory", historyDto);
        }
        public IActionResult GetBlockedChart(string Role, string ChartType, int ProjectID, string ccids, string ProjectName, int CurrCCId = 0, string Previous = "", string Next = "", string showAll = "")
        {
            _logger.LogInformation("Loading Started for GetBlockedChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            List<ChartSummaryDTO> lstChartSummaryDTO = new List<ChartSummaryDTO>();

            #region binding data
            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));
            ViewBag.ErrorTypes = BindErrorType();
            ViewBag.IsBlocked = "1";

            #endregion

            //chartSummaryDTO.ProjectName = ProjectName;

            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();

            int PrevOrNextCCId = 0;
            ViewBag.EnableNext = "false";
            ViewBag.EnablePrevious = "false";

            if (showAll != "" && ccids != null)
            {
                List<int> BlockedCCIds = ccids.Split(",").Select(int.Parse).ToList();
                int CurrentIndex = BlockedCCIds.IndexOf(CurrCCId);

                if (BlockedCCIds.Count == 1 || CurrentIndex == 0)
                {
                    Previous = "";
                    Next = "";
                }
                else
                {
                    Previous = "";
                    Next = "1";
                    CurrCCId = BlockedCCIds[CurrentIndex - 1];
                }
            }
            if (ccids != null)
            {
                List<int> CCIds = ccids.Split(",").Select(int.Parse).ToList();

                int CurrentIndex = CCIds.IndexOf(CurrCCId);

                if (Previous == "" && Next == "")
                {
                    PrevOrNextCCId = CCIds[0];
                    if (CCIds.Count > 1)
                        ViewBag.EnableNext = "true";
                }
                else if (Previous != "")
                {
                    ViewBag.EnableNext = "true";
                    PrevOrNextCCId = CCIds[CurrentIndex - 1];

                    if ((CurrentIndex - 1) == 0)
                        ViewBag.EnablePrevious = "false";
                    else
                        ViewBag.EnablePrevious = "true";
                }
                else if (Next != "")
                {
                    ViewBag.EnablePrevious = "true";

                    PrevOrNextCCId = CCIds[CurrentIndex + 1];

                    if ((CurrentIndex + 1) == (CCIds.Count - 1))
                        ViewBag.EnableNext = "false";
                    else
                        ViewBag.EnableNext = "true";
                }
            }

            switch (Role)
            {
                case "Coder":

                    chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID, timeZoneCookie, PrevOrNextCCId);

                    if (chartSummaryDTO == null)
                    {
                        _logger.LogInformation("Loading Ended for GetBlockedChart for User: " + mUserId);
                        TempData["Toast"] = "There are no charts available";
                        return RedirectToAction("CodingSummary");
                    }
                    else
                    {
                        chartSummaryDTO.ProjectName = ProjectName;

                        if (ccids == null && chartSummaryDTO.CCIDs.Split(",").Length > 1)
                        {
                            ViewBag.CurrentCCId = chartSummaryDTO.CCIDs.Split(",")[0];
                            ViewBag.EnableNext = "true";
                        }
                        else
                            ViewBag.CurrentCCId = PrevOrNextCCId;
                    }
                    break;
                case "QA":
                case "ShadowQA":

                    lstChartSummaryDTO = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID, timeZoneCookie, PrevOrNextCCId);

                    if (lstChartSummaryDTO.Count > 0)
                    {
                        lstChartSummaryDTO.FirstOrDefault().ProjectName = ProjectName;

                        if (ccids == null && lstChartSummaryDTO.FirstOrDefault().CCIDs.Split(",").Length > 1)
                        {
                            ViewBag.CurrentCCId = lstChartSummaryDTO.FirstOrDefault().CCIDs.Split(",")[0];
                            ViewBag.EnableNext = "true";
                        }
                        else
                            ViewBag.CurrentCCId = PrevOrNextCCId;
                    }
                    else
                    {
                        _logger.LogInformation("Loading Ended for GetBlockedChart for User: " + mUserId);
                        TempData["Toast"] = "There are no charts available";
                        return (Role == "QA") ? RedirectToAction("QASummary") : RedirectToAction("ShadowQASummary");
                    }
                    break;
            }

            _logger.LogInformation("Loading Ended for GetBlockedChart for User: " + mUserId);

            if (Role == "QA")
                return View("QA", lstChartSummaryDTO.OrderBy(a => a.ClaimId).ToList());
            else if (Role == "ShadowQA")
                return View("ShadowQA", lstChartSummaryDTO.OrderBy(a => a.ClaimId).ToList());
            else
                return View("Coding", chartSummaryDTO);

        }
        [HttpGet]
        public IActionResult ProviderPostedClinicalcase(string ccid)
        {
            _logger.LogInformation("Loading Started for ProviderPostedClinicalcase for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.ccid = Convert.ToInt32(ccid);

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));

            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));

            _logger.LogInformation("Loading Ended for ProviderPostedClinicalcase for User: " + mUserId);

            return PartialView("_ProviderPosted");
        }

        [HttpGet]
        public IActionResult BlockClinicalcase(string ccid, bool isFromAgingReport)
        {
            _logger.LogInformation("Loading Started for Fetching BlockClinicalcase for User: " + mUserId);

            if (isFromAgingReport)
                ViewBag.fromAging = true;
            else
                ViewBag.fromAging = false;

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.BlockCategories = clinicalcaseOperations.GetBlockCategories();
            ViewBag.ccid = Convert.ToInt32(ccid);
            ViewBag.statusid = clinicalcaseOperations.GetStatusId(ccid);

            _logger.LogInformation("Loading Ended for Fetching BlockClinicalcase for User: " + mUserId);

            return PartialView("_BlockCategory");
        }
        [HttpPost]
        public IActionResult BlockClinicalcase(string ccid, string bid, string remarks)
        {
            _logger.LogInformation("Loading Started for Submit BlockClinicalcase for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            if (ccid != null && bid != null && remarks != null)
            {
                clinicalcaseOperations.BlockClinicalcase(ccid, bid, remarks);
            }
            _logger.LogInformation("Loading Ended for Submit BlockClinicalcase for User: " + mUserId);

            return RedirectToAction("CodingSummary");
        }

        public IActionResult GetCodingIncorrectChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetCodingIncorrectChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lstchartSummary = new List<ChartSummaryDTO>();
            lstchartSummary = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            lstchartSummary.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));

            ViewBag.ErrorTypes = BindErrorType();
            #endregion

            _logger.LogInformation("Loading Ended for GetCodingIncorrectChart for User: " + mUserId);

            return View("IncorrectCharts", lstchartSummary);
        }

        public IActionResult GetCodingReadyForPostingChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetCodingReadyForPostingChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> chartSummaryDTO = new List<ChartSummaryDTO>();
            chartSummaryDTO = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            if (chartSummaryDTO.Count == 0)
            {
                _logger.LogInformation("Loading Ended for GetCodingReadyForPostingChart for User: " + mUserId);
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("CodingSummary");
            }
            chartSummaryDTO.FirstOrDefault().ProjectName = ProjectName;
            #region binding data
            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            _logger.LogInformation("Loading Ended for GetCodingReadyForPostingChart for User: " + mUserId);
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
        private void PrepareCptCodes(string cpt, DataTable dtCPT, int claimId)
        {
            string[] lstcpts = cpt.Split("|");
            foreach (var item in lstcpts.OrderBy(a => a.Split("^")[0]).Distinct().ToList())
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string[] lstcptrow = item.Split("^");
                    dtCPT.Rows.Add(lstcptrow[1], lstcptrow[2], lstcptrow[3], lstcptrow[4], claimId);
                }
            }
        }

        private void PrepareCpt1(string cpt, DataTable dtCPT, int claimId)
        {
            string[] lstcpts = cpt.Split("|");
            foreach (var item in lstcpts.OrderBy(a => a.Split("^")[0]).Distinct().ToList())
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string[] lstcptrow = item.Split("^");
                    dtCPT.Rows.Add(lstcptrow[0], lstcptrow[1], lstcptrow[2], lstcptrow[3], claimId);
                }
            }
        }

        private void PrepareDxCodes(string dx, DataTable dtDx, int claimId)
        {
            string[] lstdxs = dx.Split("|");
            foreach (var item in lstdxs)
            {
                string[] lstdxrow = item.Split("^");
                dtDx.Rows.Add(lstdxrow[1], claimId);
            }
        }

        private void PrepareDx(string dx, DataTable dtDx, int claimId)
        {
            string[] lstdxs = dx.Split(",");
            foreach (var item in lstdxs)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    var dataRows = dtDx.Select("DxCode='" + item + "'");
                    if (dataRows.Count() == 0)
                        dtDx.Rows.Add(item, claimId);
                }
            }
        }

        public IActionResult SubmitCodingAvailableChart(ChartSummaryDTO chartSummaryDTO, string codingSubmitAndGetNext, int providerPostedId, DateTime txtPostingDate, string txtCoderComment)
        {
            _logger.LogInformation("Loading Started for SubmitCodingAvailableChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            string providerPosted = Request.Form["hdnProviderPosted"].ToString();

            //Below 3 fields are related to Block->Next & Previous functionality
            string hdnBlockedCCIds = Request.Form["CCIDs"].ToString();
            string hdnCurrentCCId = Request.Form["hdnCurrentCCId"].ToString();
            string hdnIsBlocked = Request.Form["hdnIsBlocked"].ToString();
            string IsWrongProvider = Request.Form["hdnWrongProvider"].ToString();

            if (IsWrongProvider == "true")
                chartSummaryDTO.isWrongProvider = true;
            else
                chartSummaryDTO.isWrongProvider = false;
            //string submitType = Request.Form["hdnSubmitAndPost"];
            string hdnIsAuditRequired = Request.Form["hdnIsAuditRequired"];

            if (hdnIsAuditRequired == "true")
            {
                chartSummaryDTO.IsAuditRequired = true;
                chartSummaryDTO.SubmitAndPostAlso = false;
            }
            else
            {
                chartSummaryDTO.IsAuditRequired = false;
                chartSummaryDTO.SubmitAndPostAlso = true;
            }

            if (string.IsNullOrEmpty(codingSubmitAndGetNext))
                codingSubmitAndGetNext = Request.Form["hdnButtonType"];

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

            List<DashboardDTO> lstDto = new List<DashboardDTO>();

            if (providerPosted != "")
            {
                DataTable dtProCpt = new DataTable();
                dtProCpt.Columns.Add("CPTCode", typeof(string));
                dtProCpt.Columns.Add("Mod", typeof(string));
                dtProCpt.Columns.Add("Qty", typeof(string));
                dtProCpt.Columns.Add("Links", typeof(string));
                dtProCpt.Columns.Add("ClaimId", typeof(int));

                DataTable dtProDx = new DataTable();
                dtProDx.Columns.Add("DxCode", typeof(string));
                dtProDx.Columns.Add("ClaimId", typeof(int));

                string hdnProDxCodes = Request.Form["hdnProDxCodes"].ToString();
                PrepareDx(hdnProDxCodes, dtProDx, 0);
                string hdnProCptCodes = Request.Form["hdnProCptCodes"].ToString();
                PrepareCpt1(hdnProCptCodes, dtProCpt, 0);

                clinicalcaseOperations.SubmitProviderPostedChart(chartSummaryDTO, dtProDx, dtProCpt);
            }
            else
            {
                if (codingSubmitAndGetNext == "codingSubmit")
                    clinicalcaseOperations.SubmitCodingAvailableChart(chartSummaryDTO, dtClaim, dtCpt);
                else
                {
                    clinicalcaseOperations.SubmitCodingAvailableChart(chartSummaryDTO, dtClaim, dtCpt);

                    if (hdnIsBlocked == "1" && hdnBlockedCCIds != "" && hdnCurrentCCId != "")
                    {
                        List<int> blockedCCIds = hdnBlockedCCIds.Split(",").Select(int.Parse).ToList();

                        int CurrentIndex = blockedCCIds.IndexOf(Convert.ToInt32(hdnCurrentCCId));

                        string currCCId = "";
                        if (CurrentIndex > 0)
                            currCCId = blockedCCIds[CurrentIndex - 1].ToString();

                        blockedCCIds.RemoveAll(item => item == Convert.ToInt32(hdnCurrentCCId));

                        if (blockedCCIds.Count == 0 || hdnCurrentCCId == "0")//(CurrentIndex + 1) == blockedCCIds.Count ||
                        {
                            _logger.LogInformation("Loading Ended for SubmitCodingAvailableChart for User: " + mUserId);
                            TempData["Toast"] = "There are no charts available";
                            lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());
                            return RedirectToAction("CodingSummary", lstDto);
                        }
                        else
                        {
                            _logger.LogInformation("Loading Ended for SubmitCodingAvailableChart for User: " + mUserId);
                            return RedirectToAction("GetBlockedChart", new { Role = Roles.Coder.ToString(), ChartType = "Block", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName, ccids = ((CurrentIndex == blockedCCIds.Count) || CurrentIndex == 0) ? null : string.Join<int>(",", blockedCCIds), Next = "1", CurrCCId = currCCId });
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Loading Ended for SubmitCodingAvailableChart for User: " + mUserId);
                        return RedirectToAction("GetCodingAvailableChart", new { Role = Roles.Coder.ToString(), ChartType = "Available", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
                    }
                }
            }
            lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            _logger.LogInformation("Loading Ended for SubmitCodingAvailableChart for User: " + mUserId);

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("CodingSummary", lstDto);
        }
        public IActionResult codingSubmitPopup(string buttonType, string isAuditRequired)
        {
            _logger.LogInformation("Loading Started for codingSubmitPopup for User: " + mUserId);

            ViewBag.buttonType = buttonType;
            ViewBag.isAuditRequired = isAuditRequired;

            _logger.LogInformation("Loading Ended for codingSubmitPopup for User: " + mUserId);

            return PartialView("_CodingSubmitPopup");
        }
        public IActionResult GetAuditDetails(string chartType, int projectId, string dt)
        {
            _logger.LogInformation("Loading Started for GetAuditDetails for User: " + mUserId);

            bool auditFlag = IsAuditRequired(chartType, projectId, dt);

            _logger.LogInformation("Loading Ended for GetAuditDetails for User: " + mUserId);

            return new JsonResult(auditFlag);
        }
        public bool IsAuditRequired(string chartType, int projectId, string currDate)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            int samplePercentage = clinicalcaseOperations.GetSamplingPercentage(mUserId, chartType, projectId);

            if (samplePercentage > 0)
            {
                var configurationBuilder = new ConfigurationBuilder();
                var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                configurationBuilder.AddJsonFile(path, false);

                var root = configurationBuilder.Build();
                string filePath = root.GetSection("AuditInfoFilePath").Value + "\\" + chartType + "-" + mUserId + "-" + projectId + ".txt";
                string content = "";

                if (!Directory.Exists(root.GetSection("AuditInfoFilePath").Value))
                    Directory.CreateDirectory(root.GetSection("AuditInfoFilePath").Value);

                if (System.IO.File.Exists(filePath))
                    content = System.IO.File.ReadAllText(filePath);

                if (content == "" || content.Split(",")[0] != currDate)
                {
                    System.IO.File.WriteAllText(filePath, currDate + ",1-10:1~11-20:0~21-30:0~31-40:0~41-50:0~51-60:0~61-70:0,1");
                    return true;
                }
                else
                {
                    int currentChart = Convert.ToInt32(content.Split(",")[2]) + 1;

                    string[] arrAuditDetails = content.Split(",")[1].Split("~");

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
                                if (currentChart >= genRand || ((samplePercentage / 10) > (lastIndex - currentChart)))
                                {
                                    //1-10:1
                                    int newAuditChartCnt = auditedCharts + 1;
                                    string newContent = content.Split(",")[1].Replace(startIndex + "-" + lastIndex + ":" + auditedCharts, startIndex + "-" + lastIndex + ":" + newAuditChartCnt);

                                    System.IO.File.WriteAllText(filePath, currDate + "," + newContent + "," + currentChart);
                                    return true;
                                }
                            }
                            System.IO.File.WriteAllText(filePath, currDate + "," + content.Split(",")[1] + "," + currentChart);
                            return false;
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

        void PrepareRejectAudit(string rejectedFields, DataTable dtAudit, int errorTypeID)
        {
            string[] items = rejectedFields.Split("^");
            int claimId = Convert.ToInt32(items[0].Split(",")[1]);
            for (int i = 1; i < items.Count(); i++)
            {
                dtAudit.Rows.Add(items[i].Split(",")[0], items[i].Split(",")[1], items[i].Split(",")[2], errorTypeID, claimId, false);
            }
        }

        void PrepareAcceptAudit(string rejectedFields, DataTable dtAudit, int errorTypeID)
        {
            string[] items = rejectedFields.Split("^");
            int claimId = Convert.ToInt32(items[0].Split(",")[1]);
            for (int i = 1; i < items.Count(); i++)
            {
                var dataRows = dtAudit.Select("FieldName='" + items[i].Split(",")[0] + "' AND ClaimId = '" + claimId + "' AND IsAccepted = false");
                if (dataRows.Count() == 0)
                    dtAudit.Rows.Add(items[i].Split(",")[0], items[i].Split(",")[1], "", errorTypeID, claimId, true);
            }
        }

        void PrepareAcceptAudit1(string rejectedFields, DataTable dtAudit, int errorTypeID)
        {
            string[] items = rejectedFields.Split("^");
            int claimId = Convert.ToInt32(items[0].Split(",")[1]);
            for (int i = 1; i < items.Count(); i++)
            {
                dtAudit.Rows.Add(items[i].Split(",")[0], items[i].Split(",")[1], items[i].Split(",")[2], errorTypeID, claimId, true);
            }
        }
        void PrepareAudit1(string rejectedFields, DataTable dtAudit)
        {
            string[] items = rejectedFields.Split("^");
            int claimId = Convert.ToInt32(items[0].Split(",")[1]);
            for (int i = 1; i < items.Count(); i++)
            {
                dtAudit.Rows.Add(items[i].Split(",")[0], items[i].Split(",")[1], items[i].Split(",")[2], Convert.ToInt32(items[i].Split(",")[3]), claimId);
            }
        }

        void PrepareBasicParams(string basicFields, DataTable dtbasicParams)
        {
            string[] items = basicFields.Split("^");
            int claimId = Convert.ToInt32(items[0].Split(",")[1]);
            dtbasicParams.Rows.Add(items[1].Split(",")[1], items[2].Split(",")[1], items[3].Split(",")[1], claimId);
        }
        public IActionResult SubmitCodingIncorrectChart(ChartSummaryDTO chartSummaryDTO)
        {
            _logger.LogInformation("Loading Started for SubmitCodingIncorrectChart for User: " + mUserId);

            var hdnStatusId = Request.Form["hdnStatusId"].ToString();

            string hdnClaim1 = Request.Form["hdnClaim1"].ToString();
            string hdnClaim2 = Request.Form["hdnClaim2"].ToString();
            string hdnClaim3 = Request.Form["hdnClaim3"].ToString();
            string hdnClaim4 = Request.Form["hdnClaim4"].ToString();

            string hdnAcceptedClaim1 = Request.Form["hdnAcceptedClaim1"].ToString();
            string hdnAcceptedClaim2 = Request.Form["hdnAcceptedClaim2"].ToString();
            string hdnAcceptedClaim3 = Request.Form["hdnAcceptedClaim3"].ToString();
            string hdnAcceptedClaim4 = Request.Form["hdnAcceptedClaim4"].ToString();

            string hdnQAErrorTypeID1 = Request.Form["hdnQAErrorTypeID1"].ToString();
            string hdnQAErrorTypeID2 = Request.Form["hdnQAErrorTypeID2"].ToString();
            string hdnQAErrorTypeID3 = Request.Form["hdnQAErrorTypeID3"].ToString();
            string hdnQAErrorTypeID4 = Request.Form["hdnQAErrorTypeID4"].ToString();

            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ErrorTypeId", typeof(int));
            dtAudit.Columns.Add("ClaimId", typeof(int));
            dtAudit.Columns.Add("IsAccepted", typeof(bool));

            DataTable dtbasicParams = new DataTable();
            dtbasicParams.Columns.Add("ProviderID", typeof(int));
            dtbasicParams.Columns.Add("PayorID", typeof(int));
            dtbasicParams.Columns.Add("ProviderFeedbackID", typeof(int));
            dtbasicParams.Columns.Add("ClaimId", typeof(int));

            DataTable dtCpt = new DataTable();
            dtCpt.Columns.Add("CPTCode", typeof(string));
            dtCpt.Columns.Add("Mod", typeof(string));
            dtCpt.Columns.Add("Qty", typeof(string));
            dtCpt.Columns.Add("Links", typeof(string));
            dtCpt.Columns.Add("ClaimId", typeof(int));

            DataTable dtDx = new DataTable();
            dtDx.Columns.Add("DxCode", typeof(string));
            dtDx.Columns.Add("ClaimId", typeof(int));

            // Default Claim 

            var hdnDxCodes = Request.Form["hdnDxCodes"].ToString();
            var hdnRejectedDxRemarks = Request.Form["hdnRejectedDxRemarks"].ToString();
            var hdnRejectedDxCodes = Request.Form["hdnRejectedDxCodes"].ToString();

            var hdnCptCodes = Request.Form["hdnCptCodes"].ToString();
            var hdnRejectedCptRemarks = Request.Form["hdnRejectedCptRemarks"].ToString();
            var hdnRejectedCptCodes = Request.Form["hdnRejectedCptCodes"].ToString();

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

            var hdnAcceptedDxRemarks = Request.Form["hdnAcceptedDxRemarks"].ToString();
            var hdnAcceptedDxRemarks1 = Request.Form["hdnAcceptedDxRemarks1"].ToString();
            var hdnAcceptedDxRemarks2 = Request.Form["hdnAcceptedDxRemarks2"].ToString();
            var hdnAcceptedDxRemarks3 = Request.Form["hdnAcceptedDxRemarks3"].ToString();

            var hdnAcceptedCptRemarks = Request.Form["hdnAcceptedCptRemarks"].ToString();
            var hdnAcceptedCptRemarks1 = Request.Form["hdnAcceptedCptRemarks1"].ToString();
            var hdnAcceptedCptRemarks2 = Request.Form["hdnAcceptedCptRemarks2"].ToString();
            var hdnAcceptedCptRemarks3 = Request.Form["hdnAcceptedCptRemarks3"].ToString();

            var hdnClaimId1 = Request.Form["hdnClaimId1"].ToString();
            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            // Rejected basic Params

            if (!string.IsNullOrEmpty(hdnClaim1))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    PrepareRejectAudit(hdnClaim1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));
                else
                    PrepareRejectAudit(hdnClaim1, dtAudit, 0);

            if (!string.IsNullOrEmpty(hdnClaim2))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    PrepareRejectAudit(hdnClaim2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));
                else
                    PrepareRejectAudit(hdnClaim2, dtAudit, 0);

            if (!string.IsNullOrEmpty(hdnClaim3))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    PrepareRejectAudit(hdnClaim3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));
                else
                    PrepareRejectAudit(hdnClaim3, dtAudit, 0);

            if (!string.IsNullOrEmpty(hdnClaim4))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    PrepareRejectAudit(hdnClaim4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));
                else
                    PrepareRejectAudit(hdnClaim4, dtAudit, 0);

            // Rejected Dx Codes

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("Dx", hdnRejectedDxCodes, hdnRejectedDxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);
                else
                    dtAudit.Rows.Add("Dx", hdnRejectedDxCodes, hdnRejectedDxRemarks, 0, Convert.ToInt32(hdnClaimId1), false);
            }

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes1))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("Dx", hdnRejectedDxCodes1, hdnRejectedDxRemarks1, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);
                else
                    dtAudit.Rows.Add("Dx", hdnRejectedDxCodes1, hdnRejectedDxRemarks1, 0, Convert.ToInt32(hdnClaimId2), false);
            }

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("Dx", hdnRejectedDxCodes2, hdnRejectedDxRemarks2, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);
                else
                    dtAudit.Rows.Add("Dx", hdnRejectedDxCodes2, hdnRejectedDxRemarks2, 0, Convert.ToInt32(hdnClaimId3), false);
            }

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("Dx", hdnRejectedDxCodes3, hdnRejectedDxRemarks3, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);
                else
                    dtAudit.Rows.Add("Dx", hdnRejectedDxCodes3, hdnRejectedDxRemarks3, 0, Convert.ToInt32(hdnClaimId4), false);
            }

            // Rejected CPT Codes
            if (!string.IsNullOrEmpty(hdnRejectedCptCodes))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes, hdnRejectedCptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes, hdnRejectedCptRemarks, 0, Convert.ToInt32(hdnClaimId1), false);
            }

            if (!string.IsNullOrEmpty(hdnRejectedCptCodes1))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes1, hdnRejectedCptRemarks1, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes1, hdnRejectedCptRemarks1, 0, Convert.ToInt32(hdnClaimId2), false);
            }

            if (!string.IsNullOrEmpty(hdnRejectedCptCodes2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes2, hdnRejectedCptRemarks2, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes2, hdnRejectedCptRemarks2, 0, Convert.ToInt32(hdnClaimId3), false);
            }

            if (!string.IsNullOrEmpty(hdnRejectedCptCodes3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes3, hdnRejectedCptRemarks3, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes3, hdnRejectedCptRemarks3, 0, Convert.ToInt32(hdnClaimId4), false);
            }

            // Accepetd basic Params

            if (!string.IsNullOrEmpty(hdnAcceptedClaim1))
            {
                PrepareBasicParams(hdnAcceptedClaim1, dtbasicParams);
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    PrepareAcceptAudit(hdnAcceptedClaim1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));
                else
                    PrepareAcceptAudit(hdnAcceptedClaim1, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptedClaim2))
            {
                PrepareBasicParams(hdnAcceptedClaim2, dtbasicParams);
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    PrepareAcceptAudit(hdnAcceptedClaim2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));
                else
                    PrepareAcceptAudit(hdnAcceptedClaim2, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptedClaim3))
            {
                PrepareBasicParams(hdnAcceptedClaim3, dtbasicParams);
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    PrepareAcceptAudit(hdnAcceptedClaim3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));
                else
                    PrepareAcceptAudit(hdnAcceptedClaim3, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptedClaim4))
            {
                PrepareBasicParams(hdnAcceptedClaim4, dtbasicParams);
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    PrepareAcceptAudit(hdnAcceptedClaim4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));
                else
                    PrepareAcceptAudit(hdnAcceptedClaim4, dtAudit, 0);
            }

            // Accepetd Dx Codes

            if (!string.IsNullOrEmpty(hdnDxCodes))
            {
                PrepareDxCodes(hdnDxCodes, dtDx, Convert.ToInt32(hdnClaimId1));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("Dx", hdnDxCodes, hdnAcceptedDxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
                else
                    dtAudit.Rows.Add("Dx", hdnDxCodes, hdnAcceptedDxRemarks, 0, Convert.ToInt32(hdnClaimId1), true);

            }

            if (!string.IsNullOrEmpty(hdnDxCodes1))
            {
                PrepareDxCodes(hdnDxCodes1, dtDx, Convert.ToInt32(hdnClaimId2));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("Dx", hdnDxCodes1, hdnAcceptedDxRemarks1, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                else
                    dtAudit.Rows.Add("Dx", hdnDxCodes1, hdnAcceptedDxRemarks1, 0, Convert.ToInt32(hdnClaimId2), true);
            }

            if (!string.IsNullOrEmpty(hdnDxCodes2))
            {
                PrepareDxCodes(hdnDxCodes2, dtDx, Convert.ToInt32(hdnClaimId3));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("Dx", hdnDxCodes2, hdnAcceptedDxRemarks2, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                else
                    dtAudit.Rows.Add("Dx", hdnDxCodes2, hdnAcceptedDxRemarks2, 0, Convert.ToInt32(hdnClaimId3), true);
            }

            if (!string.IsNullOrEmpty(hdnDxCodes3))
            {
                PrepareDxCodes(hdnDxCodes3, dtDx, Convert.ToInt32(hdnClaimId4));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("Dx", hdnDxCodes3, hdnAcceptedDxRemarks3, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                else
                    dtAudit.Rows.Add("Dx", hdnDxCodes3, hdnAcceptedDxRemarks3, 0, Convert.ToInt32(hdnClaimId4), true);
            }

            // Accepetd CPT Codes

            if (!string.IsNullOrEmpty(hdnCptCodes))
            {
                PrepareCptCodes(hdnCptCodes, dtCpt, Convert.ToInt32(hdnClaimId1));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("CPTCode", hdnCptCodes, hdnAcceptedCptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnCptCodes, hdnAcceptedCptRemarks, 0, Convert.ToInt32(hdnClaimId1), true);
            }

            if (!string.IsNullOrEmpty(hdnCptCodes1))
            {
                PrepareCptCodes(hdnCptCodes1, dtCpt, Convert.ToInt32(hdnClaimId2));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("CPTCode", hdnCptCodes1, hdnAcceptedCptRemarks1, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnCptCodes1, hdnAcceptedCptRemarks1, 0, Convert.ToInt32(hdnClaimId2), true);
            }

            if (!string.IsNullOrEmpty(hdnCptCodes2))
            {
                PrepareCptCodes(hdnCptCodes2, dtCpt, Convert.ToInt32(hdnClaimId3));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("CPTCode", hdnCptCodes1, hdnAcceptedCptRemarks2, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnCptCodes1, hdnAcceptedCptRemarks2, 0, Convert.ToInt32(hdnClaimId3), true);
            }

            if (!string.IsNullOrEmpty(hdnCptCodes3))
            {
                PrepareCptCodes(hdnCptCodes3, dtCpt, Convert.ToInt32(hdnClaimId4));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("CPTCode", hdnCptCodes1, hdnAcceptedCptRemarks3, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnCptCodes1, hdnAcceptedCptRemarks3, 0, Convert.ToInt32(hdnClaimId4), true);
            }

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitCodingIncorrectChart(chartSummaryDTO, Convert.ToInt16(hdnStatusId), dtAudit, dtbasicParams, dtDx, dtCpt);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            _logger.LogInformation("Loading Ended for SubmitCodingIncorrectChart for User: " + mUserId);

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("CodingSummary", lstDto);
        }
        public IActionResult SubmitCodingReadyForPostingChart(ChartSummaryDTO chartSummaryDTO, string postingSubmitAndGetNext)
        {
            _logger.LogInformation("Loading Started for SubmitCodingReadyForPostingChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            if (string.IsNullOrEmpty(postingSubmitAndGetNext))
                postingSubmitAndGetNext = Request.Form["hdnButtonType"];
            if (string.IsNullOrEmpty(postingSubmitAndGetNext))
                clinicalcaseOperations.SubmitCodingReadyForPostingChart(chartSummaryDTO);
            else
            {
                clinicalcaseOperations.SubmitCodingReadyForPostingChart(chartSummaryDTO);
                return RedirectToAction("GetCodingReadyForPostingChart", new { Role = Roles.Coder.ToString(), ChartType = "ReadyForPosting", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }
            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            _logger.LogInformation("Loading Ended for SubmitCodingReadyForPostingChart for User: " + mUserId);

            TempData["Success"] = "Chart Details posted successfully !";
            return View("CodingSummary", lstDto);
        }

        [HttpGet]
        public IActionResult GetReadyforPostingPopup(string buttonType)
        {
            _logger.LogInformation("Loading Started for GetReadyforPostingPopup for User: " + mUserId);

            ViewBag.buttonType = buttonType;

            _logger.LogInformation("Loading Ended for GetReadyforPostingPopup for User: " + mUserId);

            return PartialView("_ReadyForPostingSubmitPopup");
        }

        [HttpGet]
        public IActionResult AddNewClaim(int claimID, int pid1, int pid2, int pid3, int pid4)
        {
            _logger.LogInformation("Loading Started for AddNewClaim for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            var prlst = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            prlst.RemoveAll(x => x.ID == pid1 || x.ID == pid2 || x.ID == pid3 || x.ID == pid4);
            ViewBag.Providers = prlst;
            //ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));
            ViewBag.ClaimId = claimID;

            _logger.LogInformation("Loading Ended for AddNewClaim for User: " + mUserId);

            return PartialView("_CodingClaim");
        }
        #endregion

        #region QA
        public IActionResult QASummary()
        {
            _logger.LogInformation("Loading Started for QASummary for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            _logger.LogInformation("Loading Ended for QASummary for User: " + mUserId);

            return View(lstDto);
        }
        public IActionResult GetQAAvailableChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetQAAvailableChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lstchartSummary = new List<ChartSummaryDTO>();
            lstchartSummary = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            if (lstchartSummary.Count > 0)
                lstchartSummary.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));

            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            if (lstchartSummary.Count == 0)
            {
                _logger.LogInformation("Loading Ended for GetQAAvailableChart for User: " + mUserId);
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("QASummary");
            }
            var res = clinicalcaseOperations.GetBlockResponseBycid(lstchartSummary.FirstOrDefault().CodingDTO.ClinicalCaseID);
            if (res != null)
            {
                lstchartSummary.FirstOrDefault().BlockResponseDTO = res;
            }
            _logger.LogInformation("Loading Ended for GetQAAvailableChart for User: " + mUserId);
            return View("QA", lstchartSummary.OrderBy(a => a.ClaimId).ToList());
        }

        public IActionResult GetQARebuttalChartsOfCoder(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetQARebuttalChartsOfCoder for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lstchartSummary = new List<ChartSummaryDTO>();
            lstchartSummary = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            if (lstchartSummary.Count > 0)
                lstchartSummary.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));

            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            if (lstchartSummary.Count == 0)
            {
                _logger.LogInformation("Loading Ended for GetQARebuttalChartsOfCoder for User: " + mUserId);
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("QASummary");
            }
            _logger.LogInformation("Loading Ended for GetQARebuttalChartsOfCoder for User: " + mUserId);

            return View("QARebuttalChartsOfCoder", lstchartSummary.OrderBy(a => a.ClaimId).ToList());
        }

        public IActionResult GetQARejectedChartsOfShadowQA(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetQARejectedChartsOfShadowQA for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lstChartSummaryDTO = new List<ChartSummaryDTO>();
            lstChartSummaryDTO = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            lstChartSummaryDTO.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));

            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            _logger.LogInformation("Loading Ended for GetQARejectedChartsOfShadowQA for User: " + mUserId);

            return View("QARejectedChartsOfShadowQA", lstChartSummaryDTO);
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
            _logger.LogInformation("Loading Started for SubmitQAAvailableChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            //Below 3 fields are related to Block->Next & Previous functionality
            string hdnBlockedCCIds = Request.Form["CCIDs"].ToString();
            string hdnCurrentCCId = Request.Form["hdnCurrentCCId"].ToString();
            string hdnIsBlocked = Request.Form["hdnIsBlocked"].ToString();

            //Starting of Reading Claim2 to Claim4 Data
            DataTable dtCpt = new DataTable();
            dtCpt.Columns.Add("CPTCode", typeof(string));
            dtCpt.Columns.Add("Mod", typeof(string));
            dtCpt.Columns.Add("Qty", typeof(string));
            dtCpt.Columns.Add("Links", typeof(string));
            dtCpt.Columns.Add("ClaimId", typeof(int));

            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ErrorTypeId", typeof(int));
            dtAudit.Columns.Add("ClaimId", typeof(int));
            dtAudit.Columns.Add("IsAccepted", typeof(bool));

            var hdnClaimId1 = Request.Form["hdnClaimId1"].ToString();
            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            string hdnAcceptClaim1 = Request.Form["hdnAcceptClaim1"].ToString();
            string hdnAcceptClaim2 = Request.Form["hdnAcceptClaim2"].ToString();
            string hdnAcceptClaim3 = Request.Form["hdnAcceptClaim3"].ToString();
            string hdnAcceptClaim4 = Request.Form["hdnAcceptClaim4"].ToString();

            string hdnClaim1 = Request.Form["hdnClaim1"].ToString();
            string hdnClaim2 = Request.Form["hdnClaim2"].ToString();
            string hdnClaim3 = Request.Form["hdnClaim3"].ToString();
            string hdnClaim4 = Request.Form["hdnClaim4"].ToString();

            var hdnQADxCodes = Request.Form["hdnQADxCodes"].ToString();
            var hdnQADxRemarks = Request.Form["hdnQADxRemarks"].ToString();
            var hdnQACptCodes = Request.Form["hdnQACptCodes"].ToString();
            var hdnQACptRemarks = Request.Form["hdnQACptRemarks"].ToString();

            string hdnQADxRemarks2 = Request.Form["hdnQADxRemarks2"].ToString();
            string hdnQADxCodes2 = Request.Form["hdnQADxCodes2"].ToString();
            string hdnQADxRemarks3 = Request.Form["hdnQADxRemarks3"].ToString();
            string hdnQADxCodes3 = Request.Form["hdnQADxCodes3"].ToString();
            string hdnQADxRemarks4 = Request.Form["hdnQADxRemarks4"].ToString();
            string hdnQADxCodes4 = Request.Form["hdnQADxCodes4"].ToString();

            string hdnQAErrorTypeID1 = Request.Form["hdnQAErrorTypeID1"].ToString();
            string hdnQAErrorTypeID2 = Request.Form["hdnQAErrorTypeID2"].ToString();
            string hdnQAErrorTypeID3 = Request.Form["hdnQAErrorTypeID3"].ToString();
            string hdnQAErrorTypeID4 = Request.Form["hdnQAErrorTypeID4"].ToString();

            string hdnQAAcceptDxCodes = Request.Form["hdnQAAcceptDxCodes"].ToString();
            string hdnQAAcceptCptCodes = Request.Form["hdnQAAcceptCptCodes"].ToString();

            // basic Accept Params fro Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnAcceptClaim1) && !string.IsNullOrEmpty(hdnQAErrorTypeID1))
                PrepareAcceptAudit(hdnAcceptClaim1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));

            if (!string.IsNullOrEmpty(hdnAcceptClaim2) && !string.IsNullOrEmpty(hdnQAErrorTypeID2))
                PrepareAcceptAudit(hdnAcceptClaim2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));

            if (!string.IsNullOrEmpty(hdnAcceptClaim3) && !string.IsNullOrEmpty(hdnQAErrorTypeID3))
                PrepareAcceptAudit(hdnAcceptClaim3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));

            if (!string.IsNullOrEmpty(hdnAcceptClaim4) && !string.IsNullOrEmpty(hdnQAErrorTypeID4))
                PrepareAcceptAudit(hdnAcceptClaim4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));


            // basic Reject Params fro Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnClaim1))
                PrepareRejectAudit(hdnClaim1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));

            if (!string.IsNullOrEmpty(hdnClaim2))
                PrepareRejectAudit(hdnClaim2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));

            if (!string.IsNullOrEmpty(hdnClaim3))
                PrepareRejectAudit(hdnClaim3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));

            if (!string.IsNullOrEmpty(hdnClaim4))
                PrepareRejectAudit(hdnClaim4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));

            // Claim 1 Accept Dx & CPT

            if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
            {
                if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes, "", Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);

                if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes, "", Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
            }

            // Claim 1 Reject Dx & CPT

            if (!string.IsNullOrEmpty(hdnQADxCodes) && !string.IsNullOrEmpty(hdnQADxRemarks))
                dtAudit.Rows.Add("Dx", hdnQADxCodes, hdnQADxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);

            if (!string.IsNullOrEmpty(hdnQACptCodes) && !string.IsNullOrEmpty(hdnQACptRemarks))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes, hdnQACptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);

            string hdnQAAcceptDxCodes2 = Request.Form["hdnQAAcceptDxCodes2"].ToString();
            string hdnQAAcceptCptCodes2 = Request.Form["hdnQAAcceptCptCodes2"].ToString();

            // Claim 2 Accept Dx & CPT

            if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
            {
                if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes2))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes2, "", Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);

                if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes2))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes2, "", Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
            }

            string hdnQAAcceptDxCodes3 = Request.Form["hdnQAAcceptDxCodes3"].ToString();
            string hdnQAAcceptCptCodes3 = Request.Form["hdnQAAcceptCptCodes3"].ToString();

            // Claim 3 Accept Dx & CPT

            if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
            {
                if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes3))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes3, "", Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);

                if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes3))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes3, "", Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
            }

            string hdnQAAcceptDxCodes4 = Request.Form["hdnQAAcceptDxCodes4"].ToString();
            string hdnQAAcceptCptCodes4 = Request.Form["hdnQAAcceptCptCodes4"].ToString();

            // Claim 4 Accept Dx & CPT

            if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
            {
                if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes4))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes4, "", Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);

                if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes4))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes4, "", Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
            }

            if (!string.IsNullOrEmpty(hdnQADxCodes2))
                dtAudit.Rows.Add("Dx", hdnQADxCodes2, hdnQADxRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);

            if (!string.IsNullOrEmpty(hdnQADxCodes3))
                dtAudit.Rows.Add("Dx", hdnQADxCodes3, hdnQADxRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);

            if (!string.IsNullOrEmpty(hdnQADxCodes4))
                dtAudit.Rows.Add("Dx", hdnQADxCodes4, hdnQADxRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);

            string hdnQACptRemarks2 = Request.Form["hdnQACptRemarks2"].ToString();
            string hdnQACptCodes2 = Request.Form["hdnQACptCodes2"].ToString();
            string hdnQACptRemarks3 = Request.Form["hdnQACptRemarks3"].ToString();
            string hdnQACptCodes3 = Request.Form["hdnQACptCodes3"].ToString();
            string hdnQACptRemarks4 = Request.Form["hdnQACptRemarks4"].ToString();
            string hdnQACptCodes4 = Request.Form["hdnQACptCodes4"].ToString();

            if (!string.IsNullOrEmpty(hdnQACptCodes2))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes2, hdnQACptRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);

            if (!string.IsNullOrEmpty(hdnQACptCodes3))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes3, hdnQACptRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);

            if (!string.IsNullOrEmpty(hdnQACptCodes4))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes4, hdnQACptRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);

            //Ending of Reading Claim2 to Claim4 Data

            AuditDTO auditDTO = clinicalcaseOperations.GetAuditInfoForCPTAndProvider(chartSummaryDTO.ProjectID);

            foreach (string str in auditDTO.CPTCodes.Split(","))
            {
                var dataRows = dtAudit.Select("FieldName='CPTCode'");
                if (dataRows.Count() != 0)
                {
                    foreach (var dr in dataRows)
                    {
                        string strCptCode = Convert.ToString(dr[1]);
                        foreach (string code in strCptCode.Split("|"))
                        {
                            if (str == code.Split("^")[1])
                                chartSummaryDTO.IsAuditRequired = true;
                        }
                    }
                }
            }

            foreach (string str in auditDTO.ProviderIDs.Split(","))
            {
                var dataRows = dtAudit.Select("FieldName='ProviderID' AND FieldValue = '" + str + "'");
                if (dataRows.Count() != 0)
                    chartSummaryDTO.IsAuditRequired = true;
            }

            if (chartSummaryDTO.IsAuditRequired == false)
            {
                string currDt = Request.Form["hdnCurrDate"].ToString();
                bool audit = IsAuditRequired("QA", chartSummaryDTO.ProjectID, currDt);
                chartSummaryDTO.IsAuditRequired = audit;
            }
            List<DashboardDTO> lstDto = new List<DashboardDTO>();


            if (string.IsNullOrEmpty(SubmitAndGetNext))
                clinicalcaseOperations.SubmitQAAvailableChart(chartSummaryDTO, dtAudit);
            else
            {
                clinicalcaseOperations.SubmitQAAvailableChart(chartSummaryDTO, dtAudit);
                if (hdnIsBlocked == "1" && hdnBlockedCCIds != "" && hdnCurrentCCId != "")
                {
                    List<int> blockedCCIds = hdnBlockedCCIds.Split(",").Select(int.Parse).ToList();

                    int CurrentIndex = blockedCCIds.IndexOf(Convert.ToInt32(hdnCurrentCCId));

                    string currCCId = "";
                    if (CurrentIndex > 0)
                        currCCId = blockedCCIds[CurrentIndex - 1].ToString();

                    blockedCCIds.RemoveAll(item => item == Convert.ToInt32(hdnCurrentCCId));

                    if (blockedCCIds.Count == 0 || hdnCurrentCCId == "0")//(CurrentIndex + 1) == blockedCCIds.Count ||
                    {
                        _logger.LogInformation("Loading Ended for SubmitQAAvailableChart for User: " + mUserId);
                        TempData["Toast"] = "There are no charts available";
                        lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());
                        return RedirectToAction("QASummary", lstDto);
                    }
                    else
                    {
                        _logger.LogInformation("Loading Ended for SubmitQAAvailableChart for User: " + mUserId);
                        return RedirectToAction("GetBlockedChart", new { Role = Roles.QA.ToString(), ChartType = "Block", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName, ccids = ((CurrentIndex == blockedCCIds.Count) || CurrentIndex == 0) ? null : string.Join<int>(",", blockedCCIds), Next = "1", CurrCCId = currCCId });
                    }
                }
                else
                {
                    _logger.LogInformation("Loading Ended for SubmitQAAvailableChart for User: " + mUserId);
                    return RedirectToAction("GetQAAvailableChart", new { Role = Roles.QA.ToString(), ChartType = "Available", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
                }
            }
            lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            _logger.LogInformation("Loading Ended for SubmitQAAvailableChart for User: " + mUserId);

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("QASummary", lstDto);
        }
        public IActionResult SubmitQARebuttalChartsOfCoder(ChartSummaryDTO chartSummaryDTO, string SubmitAndGetNext)
        {
            _logger.LogInformation("Loading Started for SubmitQARebuttalChartsOfCoder for User: " + mUserId);

            //Starting of fetching Dx,CPT in Claim2 to Claim4 
            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ErrorTypeId", typeof(int));
            dtAudit.Columns.Add("ClaimId", typeof(int));
            dtAudit.Columns.Add("IsAccepted", typeof(bool));

            DataTable dtbasicParams = new DataTable();
            dtbasicParams.Columns.Add("ProviderID", typeof(int));
            dtbasicParams.Columns.Add("PayorID", typeof(int));
            dtbasicParams.Columns.Add("ProviderFeedbackID", typeof(int));
            dtbasicParams.Columns.Add("ClaimId", typeof(int));

            DataTable dtCpt = new DataTable();
            dtCpt.Columns.Add("CPTCode", typeof(string));
            dtCpt.Columns.Add("Mod", typeof(string));
            dtCpt.Columns.Add("Qty", typeof(string));
            dtCpt.Columns.Add("Links", typeof(string));
            dtCpt.Columns.Add("ClaimId", typeof(int));

            DataTable dtDx = new DataTable();
            dtDx.Columns.Add("DxCode", typeof(string));
            dtDx.Columns.Add("ClaimId", typeof(int));

            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            string hdnAcceptClaim1 = Request.Form["hdnAcceptClaim1"].ToString();
            string hdnAcceptClaim2 = Request.Form["hdnAcceptClaim2"].ToString();
            string hdnAcceptClaim3 = Request.Form["hdnAcceptClaim3"].ToString();
            string hdnAcceptClaim4 = Request.Form["hdnAcceptClaim4"].ToString();

            string hdnClaimData1 = Request.Form["hdnClaimData1"].ToString();
            string hdnClaimData2 = Request.Form["hdnClaimData2"].ToString();
            string hdnClaimData3 = Request.Form["hdnClaimData3"].ToString();
            string hdnClaimData4 = Request.Form["hdnClaimData4"].ToString();

            string hdnQAErrorTypeID1 = Request.Form["hdnQAErrorTypeID1"].ToString();
            string hdnQAErrorTypeID2 = Request.Form["hdnQAErrorTypeID2"].ToString();
            string hdnQAErrorTypeID3 = Request.Form["hdnQAErrorTypeID3"].ToString();
            string hdnQAErrorTypeID4 = Request.Form["hdnQAErrorTypeID4"].ToString();

            // basic Accept Params for Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnAcceptClaim1))
            {
                PrepareBasicParams(hdnAcceptClaim1, dtbasicParams);

                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    PrepareAcceptAudit(hdnAcceptClaim1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));
                else
                    PrepareAcceptAudit(hdnAcceptClaim1, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptClaim2))
            {
                PrepareBasicParams(hdnAcceptClaim2, dtbasicParams);

                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    PrepareAcceptAudit(hdnAcceptClaim2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));
                else
                    PrepareAcceptAudit(hdnAcceptClaim2, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptClaim3))
            {
                PrepareBasicParams(hdnAcceptClaim3, dtbasicParams);

                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    PrepareAcceptAudit(hdnAcceptClaim3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));
                else
                    PrepareAcceptAudit(hdnAcceptClaim3, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptClaim4))
            {
                PrepareBasicParams(hdnAcceptClaim4, dtbasicParams);

                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    PrepareAcceptAudit(hdnAcceptClaim4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));
                else
                    PrepareAcceptAudit(hdnAcceptClaim4, dtAudit, 0);
            }


            // basic Reject Params fro Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnClaimData1))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    PrepareRejectAudit(hdnClaimData1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));
                else
                    PrepareRejectAudit(hdnClaimData1, dtAudit, 0);

            if (!string.IsNullOrEmpty(hdnClaimData2))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    PrepareRejectAudit(hdnClaimData2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));
                else
                    PrepareRejectAudit(hdnClaimData2, dtAudit, 0);

            if (!string.IsNullOrEmpty(hdnClaimData3))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    PrepareRejectAudit(hdnClaimData3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));
                else
                    PrepareRejectAudit(hdnClaimData3, dtAudit, 0);

            if (!string.IsNullOrEmpty(hdnClaimData4))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    PrepareRejectAudit(hdnClaimData4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));
                else
                    PrepareRejectAudit(hdnClaimData4, dtAudit, 0);

            var hdnDx = Request.Form["hdnDx"].ToString();
            var hdnDxRemarks = Request.Form["hdnDxRemarks"].ToString();
            string hdnDx2 = Request.Form["hdnDx2"].ToString();
            string hdnDxRemarks2 = Request.Form["hdnDxRemarks2"].ToString();
            string hdnDx3 = Request.Form["hdnDx3"].ToString();
            string hdnDxRemarks3 = Request.Form["hdnDxRemarks3"].ToString();
            string hdnDx4 = Request.Form["hdnDx4"].ToString();
            string hdnDxRemarks4 = Request.Form["hdnDxRemarks4"].ToString();

            string hdnQAAcceptDxCodes = Request.Form["hdnQAAcceptDxCodes"].ToString();
            string hdnQAAcceptCptCodes = Request.Form["hdnQAAcceptCptCodes"].ToString();
            string hdnQAAcceptDxRemarks = Request.Form["hdnQAAcceptDxRemarks"].ToString();
            string hdnQAAcceptCptRemarks = Request.Form["hdnQAAcceptCptRemarks"].ToString();

            // Accept Claim 1 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes))
            {
                PrepareDxCodes(hdnQAAcceptDxCodes, dtDx, 0);
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes, hdnQAAcceptDxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), 0, true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes, hdnQAAcceptDxRemarks, 0, 0, true);
            }

            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes))
            {
                PrepareCptCodes(hdnQAAcceptCptCodes, dtCpt, 0);
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes, hdnQAAcceptCptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), 0, true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes, hdnQAAcceptCptRemarks, 0, 0, true);
            }

            string hdnQAAcceptDxCodes2 = Request.Form["hdnQAAcceptDxCodes2"].ToString();
            string hdnQAAcceptCptCodes2 = Request.Form["hdnQAAcceptCptCodes2"].ToString();
            string hdnQAAcceptDxRemarks2 = Request.Form["hdnQAAcceptDxRemarks2"].ToString();
            string hdnQAAcceptCptRemarks2 = Request.Form["hdnQAAcceptCptRemarks2"].ToString();


            // Accept 2 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes2))
            {
                PrepareDxCodes(hdnQAAcceptDxCodes2, dtDx, Convert.ToInt32(hdnClaimId2));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes2, hdnQAAcceptDxRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes2, hdnQAAcceptDxRemarks2, 0, Convert.ToInt32(hdnClaimId2), true);
            }

            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes2))
            {
                PrepareCptCodes(hdnQAAcceptCptCodes2, dtCpt, Convert.ToInt32(hdnClaimId2));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes2, hdnQAAcceptCptRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes2, hdnQAAcceptCptRemarks2, 0, Convert.ToInt32(hdnClaimId2), true);
            }

            string hdnQAAcceptDxCodes3 = Request.Form["hdnQAAcceptDxCodes3"].ToString();
            string hdnQAAcceptCptCodes3 = Request.Form["hdnQAAcceptCptCodes3"].ToString();
            string hdnQAAcceptDxRemarks3 = Request.Form["hdnQAAcceptDxRemarks3"].ToString();
            string hdnQAAcceptCptRemarks3 = Request.Form["hdnQAAcceptCptRemarks3"].ToString();

            // Accept Claim 3 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes3))
            {
                PrepareDxCodes(hdnQAAcceptDxCodes3, dtDx, Convert.ToInt32(hdnClaimId3));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes3, hdnQAAcceptDxRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes3, hdnQAAcceptDxRemarks3, 0, Convert.ToInt32(hdnClaimId3), true);
            }
            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes3))
            {
                PrepareCptCodes(hdnQAAcceptCptCodes3, dtCpt, Convert.ToInt32(hdnClaimId3));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes3, hdnQAAcceptCptRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes3, hdnQAAcceptCptRemarks3, 0, Convert.ToInt32(hdnClaimId3), true);
            }

            string hdnQAAcceptDxCodes4 = Request.Form["hdnQAAcceptDxCodes4"].ToString();
            string hdnQAAcceptCptCodes4 = Request.Form["hdnQAAcceptCptCodes4"].ToString();
            string hdnQAAcceptDxRemarks4 = Request.Form["hdnQAAcceptDxRemarks4"].ToString();
            string hdnQAAcceptCptRemarks4 = Request.Form["hdnQAAcceptCptRemarks4"].ToString();

            // Accept Claim 4 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes4))
            {
                PrepareDxCodes(hdnQAAcceptDxCodes4, dtDx, Convert.ToInt32(hdnClaimId4));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes4, hdnQAAcceptDxRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes4, hdnQAAcceptDxRemarks4, 0, Convert.ToInt32(hdnClaimId4), true);
            }
            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes4))
            {
                PrepareCptCodes(hdnQAAcceptCptCodes4, dtCpt, Convert.ToInt32(hdnClaimId4));
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes4, hdnQAAcceptCptRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes4, hdnQAAcceptCptRemarks4, 0, Convert.ToInt32(hdnClaimId4), true);
            }

            // Reject Claim 1-4 Dx & CPT

            if (!string.IsNullOrEmpty(hdnDx) && !string.IsNullOrEmpty(hdnDxRemarks))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("Dx", hdnDx, hdnDxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), 0, false);
                else
                    dtAudit.Rows.Add("Dx", hdnDx, hdnDxRemarks, 0, 0, false);

            if (!string.IsNullOrEmpty(hdnDx2) && !string.IsNullOrEmpty(hdnDxRemarks2))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("Dx", hdnDx2, hdnDxRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);
                else
                    dtAudit.Rows.Add("Dx", hdnDx2, hdnDxRemarks2, 0, Convert.ToInt32(hdnClaimId2), false);

            if (!string.IsNullOrEmpty(hdnDx3) && !string.IsNullOrEmpty(hdnDxRemarks3))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("Dx", hdnDx3, hdnDxRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);
                else
                    dtAudit.Rows.Add("Dx", hdnDx3, hdnDxRemarks3, 0, Convert.ToInt32(hdnClaimId3), false);

            if (!string.IsNullOrEmpty(hdnDx4) && !string.IsNullOrEmpty(hdnDxRemarks4))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("Dx", hdnDx4, hdnDxRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);
                else
                    dtAudit.Rows.Add("Dx", hdnDx4, hdnDxRemarks4, 0, Convert.ToInt32(hdnClaimId4), false);

            var hdnCpt = Request.Form["hdnCpt"].ToString();
            var hdnCptRemarks = Request.Form["hdnCptRemarks"].ToString();
            string hdnCpt2 = Request.Form["hdnCpt2"].ToString();
            string hdnCptRemarks2 = Request.Form["hdnCptRemarks2"].ToString();
            string hdnCpt3 = Request.Form["hdnCpt3"].ToString();
            string hdnCptRemarks3 = Request.Form["hdnCptRemarks3"].ToString();
            string hdnCpt4 = Request.Form["hdnCpt4"].ToString();
            string hdnCptRemarks4 = Request.Form["hdnCptRemarks4"].ToString();

            if (!string.IsNullOrEmpty(hdnCpt) && !string.IsNullOrEmpty(hdnCptRemarks))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("CPTCode", hdnCpt, hdnCptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), 0, false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnCpt, hdnCptRemarks, 0, 0, false);

            if (!string.IsNullOrEmpty(hdnCpt2) && !string.IsNullOrEmpty(hdnCptRemarks2))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("CPTCode", hdnCpt2, hdnCptRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnCpt2, hdnCptRemarks2, 0, Convert.ToInt32(hdnClaimId2), false);

            if (!string.IsNullOrEmpty(hdnCpt3) && !string.IsNullOrEmpty(hdnCptRemarks3))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("CPTCode", hdnCpt3, hdnCptRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnCpt3, hdnCptRemarks3, 0, Convert.ToInt32(hdnClaimId3), false);

            if (!string.IsNullOrEmpty(hdnCpt4) && !string.IsNullOrEmpty(hdnCptRemarks4))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("CPTCode", hdnCpt4, hdnCptRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnCpt4, hdnCptRemarks4, 0, Convert.ToInt32(hdnClaimId4), false);

            //Ending of fetching Dx,CPT in Claim2 to Claim4 

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            if (string.IsNullOrEmpty(SubmitAndGetNext))
                clinicalcaseOperations.SubmitQARebuttalChartsOfCoder(chartSummaryDTO, dtAudit, dtbasicParams, dtDx, dtCpt);
            else
            {
                clinicalcaseOperations.SubmitQARebuttalChartsOfCoder(chartSummaryDTO, dtAudit, dtbasicParams, dtDx, dtCpt);
                return RedirectToAction("GetQARebuttalChartsOfCoder", new { Role = Roles.QA.ToString(), ChartType = "RebuttalOfCoder", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            _logger.LogInformation("Loading Ended for SubmitQARebuttalChartsOfCoder for User: " + mUserId);

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("QASummary", lstDto);
        }
        public IActionResult SubmitQARejectedChartsOfShadowQA(ChartSummaryDTO chartSummaryDTO)
        {
            _logger.LogInformation("Loading Started for SubmitQARejectedChartsOfShadowQA for User: " + mUserId);

            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ErrorTypeId", typeof(int));
            dtAudit.Columns.Add("ClaimId", typeof(int));
            dtAudit.Columns.Add("IsAccepted", typeof(bool));

            var hdnQADxCodes = Request.Form["hdnQADxCodes"].ToString();
            var hdnQADxRemarks = Request.Form["hdnQADxRemarks"].ToString();
            var hdnQACptCodes = Request.Form["hdnQACptCodes"].ToString();
            var hdnQACptRemarks = Request.Form["hdnQACptRemarks"].ToString();

            var hdnQAAcceptDxCodes = Request.Form["hdnQAAcceptDxCodes"].ToString();
            var hdnQAAcceptDxRemarks = Request.Form["hdnQAAcceptDxRemarks"].ToString();
            var hdnQAAcceptCptCodes = Request.Form["hdnQAAcceptCptCodes"].ToString();
            var hdnQAAcceptCptRemarks = Request.Form["hdnQAAcceptCptRemarks"].ToString();

            var hdnQADxCodes2 = Request.Form["hdnQADxCodes2"].ToString();
            var hdnQADxRemarks2 = Request.Form["hdnQADxRemarks2"].ToString();
            var hdnQACptCodes2 = Request.Form["hdnQACptCodes2"].ToString();
            var hdnQACptRemarks2 = Request.Form["hdnQACptRemarks2"].ToString();

            var hdnQAAcceptDxCodes2 = Request.Form["hdnQAAcceptDxCodes2"].ToString();
            var hdnQAAcceptDxRemarks2 = Request.Form["hdnQAAcceptDxRemarks2"].ToString();
            var hdnQAAcceptCptCodes2 = Request.Form["hdnQAAcceptCptCodes2"].ToString();
            var hdnQAAcceptCptRemarks2 = Request.Form["hdnQAAcceptCptRemarks2"].ToString();

            var hdnQADxCodes3 = Request.Form["hdnQADxCodes3"].ToString();
            var hdnQADxRemarks3 = Request.Form["hdnQADxRemarks3"].ToString();
            var hdnQACptCodes3 = Request.Form["hdnQACptCodes3"].ToString();
            var hdnQACptRemarks3 = Request.Form["hdnQACptRemarks3"].ToString();

            var hdnQAAcceptDxCodes3 = Request.Form["hdnQAAcceptDxCodes3"].ToString();
            var hdnQAAcceptDxRemarks3 = Request.Form["hdnQAAcceptDxRemarks3"].ToString();
            var hdnQAAcceptCptCodes3 = Request.Form["hdnQAAcceptCptCodes3"].ToString();
            var hdnQAAcceptCptRemarks3 = Request.Form["hdnQAAcceptCptRemarks3"].ToString();

            var hdnQADxCodes4 = Request.Form["hdnQADxCodes4"].ToString();
            var hdnQADxRemarks4 = Request.Form["hdnQADxRemarks4"].ToString();
            var hdnQACptCodes4 = Request.Form["hdnQACptCodes4"].ToString();
            var hdnQACptRemarks4 = Request.Form["hdnQACptRemarks4"].ToString();

            var hdnQAAcceptDxCodes4 = Request.Form["hdnQAAcceptDxCodes4"].ToString();
            var hdnQAAcceptDxRemarks4 = Request.Form["hdnQAAcceptDxRemarks4"].ToString();
            var hdnQAAcceptCptCodes4 = Request.Form["hdnQAAcceptCptCodes4"].ToString();
            var hdnQAAcceptCptRemarks4 = Request.Form["hdnQAAcceptCptRemarks4"].ToString();

            string hdnClaim1 = Request.Form["hdnClaim1"].ToString();
            string hdnClaim2 = Request.Form["hdnClaim2"].ToString();
            string hdnClaim3 = Request.Form["hdnClaim3"].ToString();
            string hdnClaim4 = Request.Form["hdnClaim4"].ToString();

            var hdnClaimId1 = Request.Form["hdnClaimId1"].ToString();
            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            string hdnQAErrorTypeID1 = Request.Form["hdnQAErrorTypeID1"].ToString();
            string hdnQAErrorTypeID2 = Request.Form["hdnQAErrorTypeID2"].ToString();
            string hdnQAErrorTypeID3 = Request.Form["hdnQAErrorTypeID3"].ToString();
            string hdnQAErrorTypeID4 = Request.Form["hdnQAErrorTypeID4"].ToString();

            string hdnStatusID = Request.Form["hdnStatusID"].ToString();

            string hdnAcceptClaim1 = Request.Form["hdnAcceptClaim1"].ToString();
            string hdnAcceptClaim2 = Request.Form["hdnAcceptClaim2"].ToString();
            string hdnAcceptClaim3 = Request.Form["hdnAcceptClaim3"].ToString();
            string hdnAcceptClaim4 = Request.Form["hdnAcceptClaim4"].ToString();

            // basic Accept Params for Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnAcceptClaim1))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    PrepareAcceptAudit1(hdnAcceptClaim1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));
                else
                    PrepareAcceptAudit1(hdnAcceptClaim1, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptClaim2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    PrepareAcceptAudit1(hdnAcceptClaim2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));
                else
                    PrepareAcceptAudit1(hdnAcceptClaim2, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptClaim3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    PrepareAcceptAudit1(hdnAcceptClaim3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));
                else
                    PrepareAcceptAudit1(hdnAcceptClaim3, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptClaim4))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    PrepareAcceptAudit1(hdnAcceptClaim4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));
                else
                    PrepareAcceptAudit1(hdnAcceptClaim4, dtAudit, 0);
            }

            // basic Reject Params for Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnClaim1))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    PrepareRejectAudit(hdnClaim1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));
                else
                    PrepareRejectAudit(hdnClaim1, dtAudit, 0);

            if (!string.IsNullOrEmpty(hdnClaim2))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    PrepareRejectAudit(hdnClaim2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));
                else
                    PrepareRejectAudit(hdnClaim2, dtAudit, 0);

            if (!string.IsNullOrEmpty(hdnClaim3))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    PrepareRejectAudit(hdnClaim3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));
                else
                    PrepareRejectAudit(hdnClaim3, dtAudit, 0);

            if (!string.IsNullOrEmpty(hdnClaim4))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    PrepareRejectAudit(hdnClaim4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));
                else
                    PrepareRejectAudit(hdnClaim4, dtAudit, 0);

            // Reject Claim 1 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes) && !string.IsNullOrEmpty(hdnQADxRemarks))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("Dx", hdnQADxCodes, hdnQADxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);
                else
                    dtAudit.Rows.Add("Dx", hdnQADxCodes, hdnQADxRemarks, 0, Convert.ToInt32(hdnClaimId1), false);

            if (!string.IsNullOrEmpty(hdnQACptCodes) && !string.IsNullOrEmpty(hdnQACptRemarks))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes, hdnQACptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes, hdnQACptRemarks, 0, Convert.ToInt32(hdnClaimId1), false);

            // Accept Claim 1 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes, hdnQAAcceptDxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes, hdnQAAcceptDxRemarks, 0, Convert.ToInt32(hdnClaimId1), true);
            }

            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes, hdnQAAcceptCptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes, hdnQAAcceptCptRemarks, 0, Convert.ToInt32(hdnClaimId1), true);
            }

            // Reject 2 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes2) && !string.IsNullOrEmpty(hdnQADxRemarks2))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("Dx", hdnQADxCodes2, hdnQADxRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);
                else
                    dtAudit.Rows.Add("Dx", hdnQADxCodes2, hdnQADxRemarks2, 0, Convert.ToInt32(hdnClaimId2), false);

            if (!string.IsNullOrEmpty(hdnQACptCodes2) && !string.IsNullOrEmpty(hdnQACptRemarks2))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes2, hdnQACptRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes2, hdnQACptRemarks2, 0, Convert.ToInt32(hdnClaimId2), false);

            // Accept 2 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes2, hdnQAAcceptDxRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes2, hdnQAAcceptDxRemarks2, 0, Convert.ToInt32(hdnClaimId2), true);
            }

            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes2, hdnQAAcceptCptRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes2, hdnQAAcceptCptRemarks2, 0, Convert.ToInt32(hdnClaimId2), true);
            }
            // Reject Claim 3 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes3) && !string.IsNullOrEmpty(hdnQADxRemarks3))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("Dx", hdnQADxCodes3, hdnQADxRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);
                else
                    dtAudit.Rows.Add("Dx", hdnQADxCodes3, hdnQADxRemarks3, 0, Convert.ToInt32(hdnClaimId3), false);

            if (!string.IsNullOrEmpty(hdnQACptCodes3) && !string.IsNullOrEmpty(hdnQACptRemarks3))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes3, hdnQACptRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes3, hdnQACptRemarks3, 0, Convert.ToInt32(hdnClaimId3), false);

            // Accept Claim 3 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes3, hdnQAAcceptDxRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes3, hdnQAAcceptDxRemarks3, 0, Convert.ToInt32(hdnClaimId3), true);
            }
            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes3, hdnQAAcceptCptRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes3, hdnQAAcceptCptRemarks3, 0, Convert.ToInt32(hdnClaimId3), true);
            }
            // Reject Claim 4 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes4) && !string.IsNullOrEmpty(hdnQADxRemarks4))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("Dx", hdnQADxCodes4, hdnQADxRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);
                else
                    dtAudit.Rows.Add("Dx", hdnQADxCodes4, hdnQADxRemarks4, 0, Convert.ToInt32(hdnClaimId4), false);

            if (!string.IsNullOrEmpty(hdnQACptCodes4) && !string.IsNullOrEmpty(hdnQACptRemarks4))
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes4, hdnQACptRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes4, hdnQACptRemarks4, 0, Convert.ToInt32(hdnClaimId4), false);

            // Accept Claim 4 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes4))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes4, hdnQAAcceptDxRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes4, hdnQAAcceptDxRemarks4, 0, Convert.ToInt32(hdnClaimId4), true);
            }
            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes4))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes4, hdnQAAcceptCptRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes4, hdnQAAcceptCptRemarks4, 0, Convert.ToInt32(hdnClaimId4), true);
            }

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitQARejectedChartsOfShadowQA(chartSummaryDTO, dtAudit, Convert.ToInt32(hdnStatusID));

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            _logger.LogInformation("Loading Ended for SubmitQARejectedChartsOfShadowQA for User: " + mUserId);

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
            _logger.LogInformation("Loading Started for ShadowQASummary for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.ShadowQA.ToString());

            _logger.LogInformation("Loading Ended for ShadowQASummary for User: " + mUserId);

            return View(lstDto);
        }

        public IActionResult GetShadowQAAvailableChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetShadowQAAvailableChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lstchartSummary = new List<ChartSummaryDTO>();
            lstchartSummary = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            if (lstchartSummary.Count > 0)
                lstchartSummary.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));

            ViewBag.ErrorTypes = BindErrorType();
            #endregion

            if (lstchartSummary.Count == 0)
            {
                _logger.LogInformation("Loading Ended for GetShadowQAAvailableChart for User: " + mUserId);
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("ShadowQASummary");
            }
            var res = clinicalcaseOperations.GetBlockResponseBycid(lstchartSummary.FirstOrDefault().CodingDTO.ClinicalCaseID);
            if (res != null)
            {
                lstchartSummary.FirstOrDefault().BlockResponseDTO = res;
            }
            _logger.LogInformation("Loading Ended for GetShadowQAAvailableChart for User: " + mUserId);
            return View("ShadowQA", lstchartSummary.OrderBy(a => a.ClaimId).ToList());
        }

        public IActionResult GetShadowQARebuttalChartsOfQA(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetShadowQARebuttalChartsOfQA for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lstChartSummaryDTO = new List<ChartSummaryDTO>();
            lstChartSummaryDTO = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            lstChartSummaryDTO.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            if (_httpContextAccessor.HttpContext.Session.GetString("PayorsList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("PayorsList", JsonConvert.SerializeObject(clinicalcaseOperations.GetPayorsList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("ProvidersList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ProvidersList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProvidersList()));

            if (_httpContextAccessor.HttpContext.Session.GetString("FeedbackList") == null)
                _httpContextAccessor.HttpContext.Session.SetString("FeedbackList", JsonConvert.SerializeObject(clinicalcaseOperations.GetProviderFeedbacksList()));

            ViewBag.Payors = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("PayorsList"));
            ViewBag.Providers = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("ProvidersList"));
            ViewBag.ProviderFeedbacks = JsonConvert.DeserializeObject<List<BindDTO>>(_httpContextAccessor.HttpContext.Session.GetString("FeedbackList"));

            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            _logger.LogInformation("Loading Ended for GetShadowQARebuttalChartsOfQA for User: " + mUserId);
            return View("ShadowQARebuttalChartsOfQA", lstChartSummaryDTO);
        }

        [HttpPost]
        public IActionResult SubmitShadowQAAvailableChart(ChartSummaryDTO chartSummaryDTO, string SubmitAndGetNext)
        {
            _logger.LogInformation("Loading Started for SubmitShadowQAAvailableChart for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            //Below 3 fields are related to Block->Next & Previous functionality
            string hdnBlockedCCIds = Request.Form["CCIDs"].ToString();
            string hdnCurrentCCId = Request.Form["hdnCurrentCCId"].ToString();
            string hdnIsBlocked = Request.Form["hdnIsBlocked"].ToString();

            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ErrorTypeId", typeof(int));
            dtAudit.Columns.Add("ClaimId", typeof(int));
            dtAudit.Columns.Add("IsAccepted", typeof(bool));

            var hdnShadowQADxCodes = Request.Form["hdnShadowQADxCodes"].ToString();
            var hdnShadowQADxRemarks = Request.Form["hdnShadowQADxRemarks"].ToString();
            var hdnShadowQACptCodes = Request.Form["hdnShadowQACptCodes"].ToString();
            var hdnShadowQACptRemarks = Request.Form["hdnShadowQACptRemarks"].ToString();

            var hdnShadowQADxCodes2 = Request.Form["hdnShadowQADxCodes2"].ToString();
            var hdnShadowQADxRemarks2 = Request.Form["hdnShadowQADxRemarks2"].ToString();
            var hdnShadowQACptCodes2 = Request.Form["hdnShadowQACptCodes2"].ToString();
            var hdnShadowQACptRemarks2 = Request.Form["hdnShadowQACptRemarks2"].ToString();

            var hdnShadowQADxCodes3 = Request.Form["hdnShadowQADxCodes3"].ToString();
            var hdnShadowQADxRemarks3 = Request.Form["hdnShadowQADxRemarks3"].ToString();
            var hdnShadowQACptCodes3 = Request.Form["hdnShadowQACptCodes3"].ToString();
            var hdnShadowQACptRemarks3 = Request.Form["hdnShadowQACptRemarks3"].ToString();

            var hdnShadowQADxCodes4 = Request.Form["hdnShadowQADxCodes4"].ToString();
            var hdnShadowQADxRemarks4 = Request.Form["hdnShadowQADxRemarks4"].ToString();
            var hdnShadowQACptCodes4 = Request.Form["hdnShadowQACptCodes4"].ToString();
            var hdnShadowQACptRemarks4 = Request.Form["hdnShadowQACptRemarks4"].ToString();

            string hdnClaim1 = Request.Form["hdnClaim1"].ToString();
            string hdnClaim2 = Request.Form["hdnClaim2"].ToString();
            string hdnClaim3 = Request.Form["hdnClaim3"].ToString();
            string hdnClaim4 = Request.Form["hdnClaim4"].ToString();

            var hdnClaimId1 = Request.Form["hdnClaimId1"].ToString();
            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            string hdnShadowQAErrorTypeID1 = Request.Form["hdnShadowQAErrorTypeID1"].ToString();
            string hdnShadowQAErrorTypeID2 = Request.Form["hdnShadowQAErrorTypeID2"].ToString();
            string hdnShadowQAErrorTypeID3 = Request.Form["hdnShadowQAErrorTypeID3"].ToString();
            string hdnShadowQAErrorTypeID4 = Request.Form["hdnShadowQAErrorTypeID4"].ToString();

            string hdnAcceptClaim1 = Request.Form["hdnAcceptClaim1"].ToString();
            string hdnAcceptClaim2 = Request.Form["hdnAcceptClaim2"].ToString();
            string hdnAcceptClaim3 = Request.Form["hdnAcceptClaim3"].ToString();
            string hdnAcceptClaim4 = Request.Form["hdnAcceptClaim4"].ToString();

            var hdnShadowQAAcceptDxCodes = Request.Form["hdnShadowQAAcceptDxCodes"].ToString();
            var hdnShadowQAAcceptDxRemarks = Request.Form["hdnShadowQAAcceptDxRemarks"].ToString();
            var hdnShadowQAAcceptCptCodes = Request.Form["hdnShadowQAAcceptCptCodes"].ToString();
            var hdnShadowQAAcceptCptRemarks = Request.Form["hdnShadowQAAcceptCptRemarks"].ToString();

            var hdnShadowQAAcceptDxCodes2 = Request.Form["hdnShadowQAAcceptDxCodes2"].ToString();
            var hdnShadowQAAcceptDxRemarks2 = Request.Form["hdnShadowQAAcceptDxRemarks2"].ToString();
            var hdnShadowQAAcceptCptCodes2 = Request.Form["hdnShadowQAAcceptCptCodes2"].ToString();
            var hdnShadowQAAcceptCptRemarks2 = Request.Form["hdnShadowQAAcceptCptRemarks2"].ToString();

            var hdnShadowQAAcceptDxCodes3 = Request.Form["hdnShadowQAAcceptDxCodes3"].ToString();
            var hdnShadowQAAcceptDxRemarks3 = Request.Form["hdnShadowQAAcceptDxRemarks3"].ToString();
            var hdnShadowQAAcceptCptCodes3 = Request.Form["hdnShadowQAAcceptCptCodes3"].ToString();
            var hdnShadowQAAcceptCptRemarks3 = Request.Form["hdnShadowQAAcceptCptRemarks3"].ToString();

            var hdnShadowQAAcceptDxCodes4 = Request.Form["hdnShadowQAAcceptDxCodes4"].ToString();
            var hdnShadowQAAcceptDxRemarks4 = Request.Form["hdnShadowQAAcceptDxRemarks4"].ToString();
            var hdnShadowQAAcceptCptCodes4 = Request.Form["hdnShadowQAAcceptCptCodes4"].ToString();
            var hdnShadowQAAcceptCptRemarks4 = Request.Form["hdnShadowQAAcceptCptRemarks4"].ToString();

            // basic Accept Params for Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnAcceptClaim1) && !string.IsNullOrEmpty(hdnShadowQAErrorTypeID1))
                PrepareAcceptAudit1(hdnAcceptClaim1, dtAudit, Convert.ToInt32(hdnShadowQAErrorTypeID1));

            if (!string.IsNullOrEmpty(hdnAcceptClaim2) && !string.IsNullOrEmpty(hdnShadowQAErrorTypeID2))
                PrepareAcceptAudit1(hdnAcceptClaim2, dtAudit, Convert.ToInt32(hdnShadowQAErrorTypeID2));

            if (!string.IsNullOrEmpty(hdnAcceptClaim3) && !string.IsNullOrEmpty(hdnShadowQAErrorTypeID3))
                PrepareAcceptAudit1(hdnAcceptClaim3, dtAudit, Convert.ToInt32(hdnShadowQAErrorTypeID3));

            if (!string.IsNullOrEmpty(hdnAcceptClaim4) && !string.IsNullOrEmpty(hdnShadowQAErrorTypeID4))
                PrepareAcceptAudit1(hdnAcceptClaim4, dtAudit, Convert.ToInt32(hdnShadowQAErrorTypeID4));

            // basic Reject Params for Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnClaim1))
                PrepareRejectAudit(hdnClaim1, dtAudit, Convert.ToInt32(hdnShadowQAErrorTypeID1));

            if (!string.IsNullOrEmpty(hdnClaim2))
                PrepareRejectAudit(hdnClaim2, dtAudit, Convert.ToInt32(hdnShadowQAErrorTypeID2));

            if (!string.IsNullOrEmpty(hdnClaim3))
                PrepareRejectAudit(hdnClaim3, dtAudit, Convert.ToInt32(hdnShadowQAErrorTypeID3));

            if (!string.IsNullOrEmpty(hdnClaim4))
                PrepareRejectAudit(hdnClaim4, dtAudit, Convert.ToInt32(hdnShadowQAErrorTypeID4));

            // Claim 1 Accept Dx & CPT

            if (!string.IsNullOrEmpty(hdnShadowQAAcceptDxCodes) && !string.IsNullOrEmpty(hdnShadowQAAcceptCptCodes) && hdnShadowQAErrorTypeID1 != "0")
            {
                dtAudit.Rows.Add("Dx", hdnShadowQAAcceptDxCodes, hdnShadowQAAcceptDxRemarks, Convert.ToInt32(hdnShadowQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
                dtAudit.Rows.Add("CPTCode", hdnShadowQAAcceptCptCodes, hdnShadowQAAcceptCptRemarks, Convert.ToInt32(hdnShadowQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
            }
            else if (!string.IsNullOrEmpty(hdnShadowQAAcceptDxCodes) && !string.IsNullOrEmpty(hdnShadowQAAcceptCptCodes))
            {
                dtAudit.Rows.Add("Dx", hdnShadowQAAcceptDxCodes, hdnShadowQAAcceptDxRemarks, Convert.ToInt32(hdnShadowQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
                dtAudit.Rows.Add("CPTCode", hdnShadowQAAcceptCptCodes, hdnShadowQAAcceptCptRemarks, Convert.ToInt32(hdnShadowQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
            }
            // Claim 2 Accept Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQAAcceptDxCodes2) && !string.IsNullOrEmpty(hdnShadowQAAcceptCptCodes2) && hdnShadowQAErrorTypeID2 != "0")
            {
                dtAudit.Rows.Add("Dx", hdnShadowQAAcceptDxCodes2, hdnShadowQAAcceptDxRemarks2, Convert.ToInt32(hdnShadowQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                dtAudit.Rows.Add("CPTCode", hdnShadowQAAcceptCptCodes2, hdnShadowQAAcceptCptRemarks2, Convert.ToInt32(hdnShadowQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
            }
            else if (!string.IsNullOrEmpty(hdnShadowQAAcceptDxCodes2) && !string.IsNullOrEmpty(hdnShadowQAAcceptCptCodes2))
            {
                dtAudit.Rows.Add("Dx", hdnShadowQAAcceptDxCodes2, hdnShadowQAAcceptDxRemarks2, Convert.ToInt32(hdnShadowQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                dtAudit.Rows.Add("CPTCode", hdnShadowQAAcceptCptCodes2, hdnShadowQAAcceptCptRemarks2, Convert.ToInt32(hdnShadowQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
            }
            // Claim 3 Accept Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQAAcceptDxCodes3) && !string.IsNullOrEmpty(hdnShadowQAAcceptCptCodes3) && hdnShadowQAErrorTypeID3 != "0")
            {
                dtAudit.Rows.Add("Dx", hdnShadowQAAcceptDxCodes3, hdnShadowQAAcceptDxRemarks3, Convert.ToInt32(hdnShadowQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                dtAudit.Rows.Add("CPTCode", hdnShadowQAAcceptCptCodes3, hdnShadowQAAcceptCptRemarks3, Convert.ToInt32(hdnShadowQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
            }
            else if (!string.IsNullOrEmpty(hdnShadowQAAcceptDxCodes3) && !string.IsNullOrEmpty(hdnShadowQAAcceptCptCodes3))
            {
                dtAudit.Rows.Add("Dx", hdnShadowQAAcceptDxCodes3, hdnShadowQAAcceptDxRemarks3, Convert.ToInt32(hdnShadowQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                dtAudit.Rows.Add("CPTCode", hdnShadowQAAcceptCptCodes3, hdnShadowQAAcceptCptRemarks3, Convert.ToInt32(hdnShadowQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
            }
            // Claim 4 Accept Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQAAcceptDxCodes4) && !string.IsNullOrEmpty(hdnShadowQAAcceptCptCodes4) && hdnShadowQAErrorTypeID4 != "0")
            {
                dtAudit.Rows.Add("Dx", hdnShadowQAAcceptDxCodes4, hdnShadowQAAcceptDxRemarks4, Convert.ToInt32(hdnShadowQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                dtAudit.Rows.Add("CPTCode", hdnShadowQAAcceptCptCodes4, hdnShadowQAAcceptCptRemarks4, Convert.ToInt32(hdnShadowQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
            }
            else if (!string.IsNullOrEmpty(hdnShadowQAAcceptDxCodes4) && !string.IsNullOrEmpty(hdnShadowQAAcceptCptCodes4))
            {
                dtAudit.Rows.Add("Dx", hdnShadowQAAcceptDxCodes4, hdnShadowQAAcceptDxRemarks4, Convert.ToInt32(hdnShadowQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                dtAudit.Rows.Add("CPTCode", hdnShadowQAAcceptCptCodes4, hdnShadowQAAcceptCptRemarks4, Convert.ToInt32(hdnShadowQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
            }

            // Reject Dx & CPT

            // Claim 1 Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQADxCodes) && !string.IsNullOrEmpty(hdnShadowQADxRemarks))
                dtAudit.Rows.Add("Dx", hdnShadowQADxCodes, hdnShadowQADxRemarks, Convert.ToInt32(hdnShadowQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);

            if (!string.IsNullOrEmpty(hdnShadowQACptCodes) && !string.IsNullOrEmpty(hdnShadowQACptRemarks))
                dtAudit.Rows.Add("CPTCode", hdnShadowQACptCodes, hdnShadowQACptRemarks, Convert.ToInt32(hdnShadowQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);

            // Claim 2 Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQADxCodes2) && !string.IsNullOrEmpty(hdnShadowQADxRemarks2))
                dtAudit.Rows.Add("Dx", hdnShadowQADxCodes2, hdnShadowQADxRemarks2, Convert.ToInt32(hdnShadowQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);

            if (!string.IsNullOrEmpty(hdnShadowQACptCodes2) && !string.IsNullOrEmpty(hdnShadowQACptRemarks2))
                dtAudit.Rows.Add("CPTCode", hdnShadowQACptCodes2, hdnShadowQACptRemarks2, Convert.ToInt32(hdnShadowQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);

            // Claim 3 Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQADxCodes3) && !string.IsNullOrEmpty(hdnShadowQADxRemarks3))
                dtAudit.Rows.Add("Dx", hdnShadowQADxCodes3, hdnShadowQADxRemarks3, Convert.ToInt32(hdnShadowQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);

            if (!string.IsNullOrEmpty(hdnShadowQACptCodes3) && !string.IsNullOrEmpty(hdnShadowQACptRemarks3))
                dtAudit.Rows.Add("CPTCode", hdnShadowQACptCodes3, hdnShadowQACptRemarks3, Convert.ToInt32(hdnShadowQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);

            // Claim 4 Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQADxCodes4) && !string.IsNullOrEmpty(hdnShadowQADxRemarks4))
                dtAudit.Rows.Add("Dx", hdnShadowQADxCodes4, hdnShadowQADxRemarks4, Convert.ToInt32(hdnShadowQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);

            if (!string.IsNullOrEmpty(hdnShadowQACptCodes4) && !string.IsNullOrEmpty(hdnShadowQACptRemarks4))
                dtAudit.Rows.Add("CPTCode", hdnShadowQACptCodes4, hdnShadowQACptRemarks4, Convert.ToInt32(hdnShadowQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);


            List<DashboardDTO> lstDto = new List<DashboardDTO>();

            if (string.IsNullOrEmpty(SubmitAndGetNext))
                clinicalcaseOperations.SubmitShadowQAAvailableChart(chartSummaryDTO, dtAudit);
            else
            {
                clinicalcaseOperations.SubmitShadowQAAvailableChart(chartSummaryDTO, dtAudit);
                if (hdnIsBlocked == "1" && hdnBlockedCCIds != "" && hdnCurrentCCId != "")
                {
                    List<int> blockedCCIds = hdnBlockedCCIds.Split(",").Select(int.Parse).ToList();

                    int CurrentIndex = blockedCCIds.IndexOf(Convert.ToInt32(hdnCurrentCCId));

                    string currCCId = "";
                    if (CurrentIndex > 0)
                        currCCId = blockedCCIds[CurrentIndex - 1].ToString();

                    blockedCCIds.RemoveAll(item => item == Convert.ToInt32(hdnCurrentCCId));

                    if (blockedCCIds.Count == 0 || hdnCurrentCCId == "0")
                    {
                        _logger.LogInformation("Loading Ended for SubmitShadowQAAvailableChart for User: " + mUserId);
                        TempData["Toast"] = "There are no charts available";
                        lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.ShadowQA.ToString());
                        return RedirectToAction("ShadowQASummary", lstDto);
                    }
                    else
                    {
                        _logger.LogInformation("Loading Ended for SubmitShadowQAAvailableChart for User: " + mUserId);
                        return RedirectToAction("GetBlockedChart", new { Role = Roles.ShadowQA.ToString(), ChartType = "Block", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName, ccids = ((CurrentIndex == blockedCCIds.Count) || CurrentIndex == 0) ? null : string.Join<int>(",", blockedCCIds), Next = "1", CurrCCId = currCCId });
                    }
                }
                else
                {
                    _logger.LogInformation("Loading Ended for SubmitShadowQAAvailableChart for User: " + mUserId);
                    return RedirectToAction("GetShadowQAAvailableChart", new { Role = Roles.ShadowQA.ToString(), ChartType = "Available", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
                }
            }
            lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.ShadowQA.ToString());
            _logger.LogInformation("Loading Ended for SubmitShadowQAAvailableChart for User: " + mUserId);
            TempData["Success"] = "Chart Details submitted successfully !";
            return View("ShadowQASummary", lstDto);
        }

        public IActionResult SubmitShadowQARebuttalChartsOfQA(ChartSummaryDTO chartSummaryDTO)
        {
            _logger.LogInformation("Loading Started for SubmitShadowQARebuttalChartsOfQA for User: " + mUserId);

            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ErrorTypeId", typeof(int));
            dtAudit.Columns.Add("ClaimId", typeof(int));
            dtAudit.Columns.Add("IsAccepted", typeof(bool));

            var hdnQADxCodes = Request.Form["hdnQADxCodes"].ToString();
            var hdnQADxRemarks = Request.Form["hdnQADxRemarks"].ToString();
            var hdnQACptCodes = Request.Form["hdnQACptCodes"].ToString();
            var hdnQACptRemarks = Request.Form["hdnQACptRemarks"].ToString();

            var hdnQAAcceptDxCodes = Request.Form["hdnQAAcceptDxCodes"].ToString();
            var hdnQAAcceptDxRemarks = Request.Form["hdnQAAcceptDxRemarks"].ToString();
            var hdnQAAcceptCptCodes = Request.Form["hdnQAAcceptCptCodes"].ToString();
            var hdnQAAcceptCptRemarks = Request.Form["hdnQAAcceptCptRemarks"].ToString();

            var hdnQADxCodes2 = Request.Form["hdnQADxCodes2"].ToString();
            var hdnQADxRemarks2 = Request.Form["hdnQADxRemarks2"].ToString();
            var hdnQACptCodes2 = Request.Form["hdnQACptCodes2"].ToString();
            var hdnQACptRemarks2 = Request.Form["hdnQACptRemarks2"].ToString();

            var hdnQAAcceptDxCodes2 = Request.Form["hdnQAAcceptDxCodes2"].ToString();
            var hdnQAAcceptDxRemarks2 = Request.Form["hdnQAAcceptDxRemarks2"].ToString();
            var hdnQAAcceptCptCodes2 = Request.Form["hdnQAAcceptCptCodes2"].ToString();
            var hdnQAAcceptCptRemarks2 = Request.Form["hdnQAAcceptCptRemarks2"].ToString();

            var hdnQADxCodes3 = Request.Form["hdnQADxCodes3"].ToString();
            var hdnQADxRemarks3 = Request.Form["hdnQADxRemarks3"].ToString();
            var hdnQACptCodes3 = Request.Form["hdnQACptCodes3"].ToString();
            var hdnQACptRemarks3 = Request.Form["hdnQACptRemarks3"].ToString();

            var hdnQAAcceptDxCodes3 = Request.Form["hdnQAAcceptDxCodes3"].ToString();
            var hdnQAAcceptDxRemarks3 = Request.Form["hdnQAAcceptDxRemarks3"].ToString();
            var hdnQAAcceptCptCodes3 = Request.Form["hdnQAAcceptCptCodes3"].ToString();
            var hdnQAAcceptCptRemarks3 = Request.Form["hdnQAAcceptCptRemarks3"].ToString();

            var hdnQADxCodes4 = Request.Form["hdnQADxCodes4"].ToString();
            var hdnQADxRemarks4 = Request.Form["hdnQADxRemarks4"].ToString();
            var hdnQACptCodes4 = Request.Form["hdnQACptCodes4"].ToString();
            var hdnQACptRemarks4 = Request.Form["hdnQACptRemarks4"].ToString();

            string hdnAcceptClaim1 = Request.Form["hdnAcceptClaim1"].ToString();
            string hdnAcceptClaim2 = Request.Form["hdnAcceptClaim2"].ToString();
            string hdnAcceptClaim3 = Request.Form["hdnAcceptClaim3"].ToString();
            string hdnAcceptClaim4 = Request.Form["hdnAcceptClaim4"].ToString();

            var hdnQAAcceptDxCodes4 = Request.Form["hdnQAAcceptDxCodes4"].ToString();
            var hdnQAAcceptDxRemarks4 = Request.Form["hdnQAAcceptDxRemarks4"].ToString();
            var hdnQAAcceptCptCodes4 = Request.Form["hdnQAAcceptCptCodes4"].ToString();
            var hdnQAAcceptCptRemarks4 = Request.Form["hdnQAAcceptCptRemarks4"].ToString();

            string hdnClaim1 = Request.Form["hdnClaim1"].ToString();
            string hdnClaim2 = Request.Form["hdnClaim2"].ToString();
            string hdnClaim3 = Request.Form["hdnClaim3"].ToString();
            string hdnClaim4 = Request.Form["hdnClaim4"].ToString();

            var hdnClaimId1 = Request.Form["hdnClaimId1"].ToString();
            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            string hdnQAErrorTypeID1 = Request.Form["hdnQAErrorTypeID1"].ToString();
            string hdnQAErrorTypeID2 = Request.Form["hdnQAErrorTypeID2"].ToString();
            string hdnQAErrorTypeID3 = Request.Form["hdnQAErrorTypeID3"].ToString();
            string hdnQAErrorTypeID4 = Request.Form["hdnQAErrorTypeID4"].ToString();

            string hdnStatusID = Request.Form["hdnStatusID"].ToString();

            // basic Accept Params for Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnAcceptClaim1))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    PrepareAcceptAudit1(hdnAcceptClaim1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));
                else
                    PrepareAcceptAudit1(hdnAcceptClaim1, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptClaim2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    PrepareAcceptAudit1(hdnAcceptClaim2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));
                else
                    PrepareAcceptAudit1(hdnAcceptClaim2, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptClaim3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    PrepareAcceptAudit1(hdnAcceptClaim3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));
                else
                    PrepareAcceptAudit1(hdnAcceptClaim3, dtAudit, 0);
            }

            if (!string.IsNullOrEmpty(hdnAcceptClaim4))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    PrepareAcceptAudit1(hdnAcceptClaim4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));
                else
                    PrepareAcceptAudit1(hdnAcceptClaim4, dtAudit, 0);
            }

            // basic Reject Params for Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnClaim1))
                PrepareRejectAudit(hdnClaim1, dtAudit, Convert.ToInt32(hdnQAErrorTypeID1));

            if (!string.IsNullOrEmpty(hdnClaim2))
                PrepareRejectAudit(hdnClaim2, dtAudit, Convert.ToInt32(hdnQAErrorTypeID2));

            if (!string.IsNullOrEmpty(hdnClaim3))
                PrepareRejectAudit(hdnClaim3, dtAudit, Convert.ToInt32(hdnQAErrorTypeID3));

            if (!string.IsNullOrEmpty(hdnClaim4))
                PrepareRejectAudit(hdnClaim4, dtAudit, Convert.ToInt32(hdnQAErrorTypeID4));

            // Reject Claim 1 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes) && !string.IsNullOrEmpty(hdnQADxRemarks))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("Dx", hdnQADxCodes, hdnQADxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);
                else
                    dtAudit.Rows.Add("Dx", hdnQADxCodes, hdnQADxRemarks, 0, Convert.ToInt32(hdnClaimId1), false);
            }

            if (!string.IsNullOrEmpty(hdnQACptCodes) && !string.IsNullOrEmpty(hdnQACptRemarks))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes, hdnQACptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes, hdnQACptRemarks, 0, Convert.ToInt32(hdnClaimId1), false);
            }

            // Accept Claim 1 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes, hdnQAAcceptDxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes, hdnQAAcceptDxRemarks, 0, Convert.ToInt32(hdnClaimId1), true);
            }

            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID1))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes, hdnQAAcceptCptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes, hdnQAAcceptCptRemarks, 0, Convert.ToInt32(hdnClaimId1), true);
            }
            // Reject 2 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes2) && !string.IsNullOrEmpty(hdnQADxRemarks2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("Dx", hdnQADxCodes2, hdnQADxRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);
                else
                    dtAudit.Rows.Add("Dx", hdnQADxCodes2, hdnQADxRemarks2, 0, Convert.ToInt32(hdnClaimId2), false);
            }

            if (!string.IsNullOrEmpty(hdnQACptCodes2) && !string.IsNullOrEmpty(hdnQACptRemarks2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes2, hdnQACptRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes2, hdnQACptRemarks2, 0, Convert.ToInt32(hdnClaimId2), false);
            }

            // Accept 2 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes2, hdnQAAcceptDxRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes2, hdnQAAcceptDxRemarks2, 0, Convert.ToInt32(hdnClaimId2), true);
            }
            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes2))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID2))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes2, hdnQAAcceptCptRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes2, hdnQAAcceptCptRemarks2, 0, Convert.ToInt32(hdnClaimId2), true);
            }
            // Reject Claim 3 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes3) && !string.IsNullOrEmpty(hdnQADxRemarks3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("Dx", hdnQADxCodes3, hdnQADxRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);
                else
                    dtAudit.Rows.Add("Dx", hdnQADxCodes3, hdnQADxRemarks3, 0, Convert.ToInt32(hdnClaimId3), false);
            }

            if (!string.IsNullOrEmpty(hdnQACptCodes3) && !string.IsNullOrEmpty(hdnQACptRemarks3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes3, hdnQACptRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes3, hdnQACptRemarks3, 0, Convert.ToInt32(hdnClaimId3), false);
            }

            // Accept Claim 3 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes3, hdnQAAcceptDxRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes3, hdnQAAcceptDxRemarks3, 0, Convert.ToInt32(hdnClaimId3), true);
            }
            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes3))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID3))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes3, hdnQAAcceptCptRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes3, hdnQAAcceptCptRemarks3, 0, Convert.ToInt32(hdnClaimId3), true);
            }
            // Reject Claim 4 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes4) && !string.IsNullOrEmpty(hdnQADxRemarks4))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("Dx", hdnQADxCodes4, hdnQADxRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);
                else
                    dtAudit.Rows.Add("Dx", hdnQADxCodes4, hdnQADxRemarks4, 0, Convert.ToInt32(hdnClaimId4), false);
            }

            if (!string.IsNullOrEmpty(hdnQACptCodes4) && !string.IsNullOrEmpty(hdnQACptRemarks4))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes4, hdnQACptRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), false);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQACptCodes4, hdnQACptRemarks4, 0, Convert.ToInt32(hdnClaimId4), false);
            }

            // Accept Claim 4 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQAAcceptDxCodes4))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes4, hdnQAAcceptDxRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                else
                    dtAudit.Rows.Add("Dx", hdnQAAcceptDxCodes4, hdnQAAcceptDxRemarks4, 0, Convert.ToInt32(hdnClaimId4), true);
            }
            if (!string.IsNullOrEmpty(hdnQAAcceptCptCodes4))
            {
                if (!string.IsNullOrEmpty(hdnQAErrorTypeID4))
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes4, hdnQAAcceptCptRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4), true);
                else
                    dtAudit.Rows.Add("CPTCode", hdnQAAcceptCptCodes4, hdnQAAcceptCptRemarks4, 0, Convert.ToInt32(hdnClaimId4), true);
            }

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitShadowQARebuttalChartsOfQA(chartSummaryDTO, dtAudit, Convert.ToInt32(hdnStatusID));

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.ShadowQA.ToString());

            _logger.LogInformation("Loading Ended for SubmitShadowQARebuttalChartsOfQA for User: " + mUserId);

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("ShadowQASummary", lstDto);
        }
        #endregion

        #region Settings

        public List<BindDTO> BindErrorType()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            if (_httpContextAccessor.HttpContext.Session.GetString("ErrorType") == null)
                _httpContextAccessor.HttpContext.Session.SetString("ErrorType", JsonConvert.SerializeObject(clinicalcaseOperations.GetErrorTypes()));

            List<ErrorType> lstErrorType = JsonConvert.DeserializeObject<List<ErrorType>>(_httpContextAccessor.HttpContext.Session.GetString("ErrorType"));

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
            _logger.LogInformation("Loading Started for Fetching AssignClinicalCaseToUser for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            string assignedusername = clinicalcaseOperations.GetAssignedusername(ccid);
            var assignusers = clinicalcaseOperations.GetAssignedToUsers(ccid);



            ViewBag.assignusers = assignusers;
            ViewBag.ccid = ccid;
            SearchResultDTO SearchResultDTO = new SearchResultDTO();
            SearchResultDTO.ClinicalCaseId = ccid;
            SearchResultDTO.AssignFromUserEmail = assignedusername;

            _logger.LogInformation("Loading Ended for Fetching AssignClinicalCaseToUser for User: " + mUserId);

            return PartialView("_AssignClinicalCaseToUser", SearchResultDTO);

        }
        [HttpPost]
        public IActionResult AssignClinicalCaseToUser(string ccid, string AssignedTo, string IsPriority)
        {
            _logger.LogInformation("Loading Started for Submit AssignClinicalCaseToUser for User: " + mUserId);

            SearchResultDTO SearchResultDTO = new SearchResultDTO();
            SearchResultDTO.ClinicalCaseId = ccid;
            SearchResultDTO.AssignToUserEmail = AssignedTo;
            SearchResultDTO.IsPriority = Convert.ToBoolean(IsPriority);


            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            clinicalcaseOperations.AssignClinicalcase(SearchResultDTO);
            _logger.LogInformation("Loading Ended for Submit AssignClinicalCaseToUser for User: " + mUserId);
            return RedirectToAction("SettingsSearch");
        }
        [HttpGet]
        public IActionResult SettingsSearch()
        {
            _logger.LogInformation("Loading Started for Fetching SettingsSearch for User: " + mUserId);

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();
            var status = clinicalcaseOperations.GetStatusList();
            ViewBag.Status = status.Where(x => x.StatusId != 3 && x.StatusId != 6 && x.StatusId != 10).ToList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            _logger.LogInformation("Loading Ended for Fetching SettingsSearch for User: " + mUserId);
            return View();
        }
        [HttpPost]
        public IActionResult SettingsSearch(string fname, string lname, string mrn, string dosfrom, string dosto, string status, string project, string provider, bool includeblocked)
        {
            _logger.LogInformation("Loading Started for Submit SettingsSearch for User: " + mUserId);
            SearchParametersDTO searchParametersDTO = new SearchParametersDTO()
            {
                FirstName = fname,
                LastName = lname,
                MRN = mrn,
                DoSFrom = Convert.ToDateTime(dosfrom),
                DoSTo = Convert.ToDateTime(dosto),
                StatusName = status,
                ProjectName = project,
                ProviderName = provider,
                IncludeBlocked = includeblocked
            };
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId, mUserRole);
            var searchData = clinicalcaseOperations.GetSearchData(searchParametersDTO);
            _logger.LogInformation("Loading Ended for Submit SettingsSearch for User: " + mUserId);
            return PartialView("_SettingsSearchResults", searchData);
        }
        [HttpPost]

        [HttpGet]
        public IActionResult SettingsBlockCategories()
        {
            _logger.LogInformation("Loading Started for SettingsBlockCategories for User: " + mUserId);

            List<BlockCategory> lstblock = new List<BlockCategory>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstblock = clinicalcaseOperations.GetBlockCategories();
            ViewBag.lstblock = lstblock;

            _logger.LogInformation("Loading Ended for SettingsBlockCategories for User: " + mUserId);

            return View();
        }
        [HttpGet]
        public ActionResult Add_EditBlockCategories(int id = 0)
        {
            _logger.LogInformation("Loading Started for Add_EditBlockCategories for User: " + mUserId);

            BlockCategory obj = new BlockCategory();
            if (id != 0)
            {
                List<BlockCategory> lstProvider = new List<BlockCategory>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstProvider = clinicalcaseOperations.GetBlockCategories();
                var res = lstProvider.Where(a => a.BlockCategoryId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Add_EditBlockCategories for User: " + mUserId);

            return PartialView("_AddEditBlockCategory", obj);
        }
        [HttpPost]
        public IActionResult AddSettingsBlockCategory(BlockCategory category)
        {
            _logger.LogInformation("Loading Started for AddSettingsBlockCategory for User: " + mUserId);

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
            _logger.LogInformation("Loading Ended for AddSettingsBlockCategory for User: " + mUserId);

            return RedirectToAction("SettingsBlockCategories");
        }
        [HttpGet]
        public IActionResult DeleteBlockCategory(int id)
        {
            _logger.LogInformation("Loading Started for Fetching DeleteBlockCategory for User: " + mUserId);

            BlockCategory obj = new BlockCategory();
            if (id != 0)
            {
                List<BlockCategory> lstblock = new List<BlockCategory>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstblock = clinicalcaseOperations.GetBlockCategories();
                var res = lstblock.Where(a => a.BlockCategoryId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Fetching DeleteBlockCategory for User: " + mUserId);
            return PartialView("_DeleteBlockCategory", obj);
        }

        [HttpPost]
        public IActionResult DeleteBlockCategory(BlockCategory blockCategory)
        {
            try
            {
                _logger.LogInformation("Loading Started for Submit DeleteBlockCategory for User: " + mUserId);

                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (blockCategory.BlockCategoryId != 0)
                    clinicalcaseOperations.DeletetBlockCategory(blockCategory.BlockCategoryId); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for Submit DeleteBlockCategory for User: " + mUserId);
            return RedirectToAction("SettingsBlockCategories");
        }

        [HttpGet]
        public IActionResult ManageEMCodeLevels()
        {
            _logger.LogInformation("Loading Started for ManageEMCodeLevels for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            //var emlevels = clinicalcaseOperations.GetManageEMCLevelsByProjectId(1);
            var emlevels = clinicalcaseOperations.GetManageEMCLevelsByProjectId();
            ViewBag.emlevels = emlevels;
            var lstProject = clinicalcaseOperations.GetProjects();
            ViewBag.lstProject = lstProject;
            _logger.LogInformation("Loading Ended for ManageEMCodeLevels for User: " + mUserId);
            return View();
        }


        [HttpGet]
        public IActionResult GetEMCodeLevelsbyId(int ProjectId)
        {
            _logger.LogInformation("Loading Started for GetEMCodeLevelsbyId for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.emlevels = clinicalcaseOperations.GetEMCodeLevelsbyId(ProjectId);
            _logger.LogInformation("Loading Ended for GetEMCodeLevelsbyId for User: " + mUserId);
            return PartialView("_bindEMLevels");
        }


        [HttpGet]
        [Route("UAB/EMLevelDetails")]
        [Route("EMLevelDetails/{eMLevel}")]
        [Route("EMLevelDetails/{eMLevelId}/{eMLevel}/{projectname}")]
        public ActionResult EMLevelDetails(int eMLevelId, int eMLevel, string projectname)
        {
            if (eMLevelId != 0)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                var emleveldetails = clinicalcaseOperations.GetEMCodeLevelDetails(eMLevelId);
                ViewBag.emleveldetails = emleveldetails;
                ViewBag.projectname = projectname;
                ViewBag.eMLevel = eMLevel;
                ViewBag.eMLevelId = eMLevelId;
                return View("EMLevelDetails", emleveldetails);
            }
            return RedirectToAction("ManageEMCodeLevels");
        }
        [HttpGet]
        public IActionResult UpdateEMCode(int Id)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var emcode = clinicalcaseOperations.GetEMCodeById(Id);
            return PartialView("_UpdateEMCode", emcode);
        }
        [HttpPost]
        public IActionResult UpdateEMCode(EMCodeLevel model)
        {
            try
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                clinicalcaseOperations.UpdateEMCode(model);
                TempData["Success"] = "Successfully EM Code Updated";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("EMLevelDetails", new { eMLevel = model.EMLevel });
        }
        [HttpGet]
        public ActionResult AddEMCode(int eMLevelId)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            EMCodeLevel model = new EMCodeLevel();
            model.Id = eMLevelId;
            return PartialView("_AddEMCode", model);
        }

        [HttpPost]
        public ActionResult AddEMCode(EMCodeLevel model)
        {

            if (model.Id != 0 && !string.IsNullOrEmpty(model.EMCode))
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

                try
                {
                    clinicalcaseOperations.AddEMCode(model);
                    TempData["Success"] = "Successfully EM Code Added User";
                    return RedirectToAction("EMLevelDetails", new { eMLevel = model.EMLevel });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
                return RedirectToAction("EMLevelDetails", new { eMLevelId = model.EMLevel });
            }
            return RedirectToAction("ManageEMCodeLevels");
        }
        [HttpGet]
        public IActionResult DeleteEMCode(int Id)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var emcode = clinicalcaseOperations.GetEMCodeById(Id);
            return PartialView("_DeleteEMCode", emcode);
        }
        [HttpPost]
        public IActionResult DeleteEMCode(EMCodeLevel model)
        {

            try
            {
                if (model.Id != 0)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                    clinicalcaseOperations.DeletetEMCode(model);
                    TempData["Success"] = "EM Code Deleted Successfully";
                    return RedirectToAction("EMLevelDetails", new { eMLevel = model.EMLevel });
                }
                return RedirectToAction("EMLevelDetails", new { eMLevel = model.EMLevel });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("ManageEMCodeLevels");
        }
        [HttpGet]
        public IActionResult DeleteEMLevel(int emlevel)
        {
            EMLevel eml = new EMLevel
            {
                Level = emlevel
            };
            return PartialView("_DeleteEMLevel", eml);
        }
        [HttpPost]
        public IActionResult DeleteEMLevel(int emlevel, string emcode = null)
        {
            try
            {
                if (emlevel != 0)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                    clinicalcaseOperations.DeletetEMCode(emlevel);
                    TempData["Success"] = "EM Level Deleted Successfully";
                    return RedirectToAction("ManageEMCodeLevels");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("ManageEMCodeLevels");
        }
        [HttpGet]
        public ActionResult AddEMLevel()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstProject = clinicalcaseOperations.GetProjects();
            ViewBag.lstProject = lstProject;

            return PartialView("_AddEMLevel");
        }
        [HttpPost]
        public ActionResult AddEMLevel(EMLevelDTO model)
        {
            if (model.EMLevel != 0 && model.ProjectId != 0)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                try
                {
                    clinicalcaseOperations.AddEMLevel(model);
                    TempData["Success"] = "Successfully EM Level Added";
                    //return RedirectToAction("EMLevelDetails", new { eMLevel = model.EMLevel });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction("ManageEMCodeLevels");
        }

        [HttpGet]
        public IActionResult SettingsLocation()
        {
            _logger.LogInformation("Loading Started for SettingsLocation for User: " + mUserId);
            List<Location> lstlocation = new List<Location>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.lstlocation = clinicalcaseOperations.GetLocations();
            _logger.LogInformation("Loading Ended for SettingsLocation for User: " + mUserId);
            return View();
        }
        [HttpGet]
        public ActionResult Add_EditLocation(int id = 0)
        {
            _logger.LogInformation("Loading Started for Add_EditLocation for User: " + mUserId);
            Location obj = new Location();
            if (id != 0)
            {
                List<Location> lstLocation = new List<Location>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstLocation = clinicalcaseOperations.GetLocations();
                var res = lstLocation.Where(a => a.LocationId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Add_EditLocation for User: " + mUserId);
            return PartialView("_AddEditLocation", obj);
        }
        [HttpPost]
        public IActionResult Add_EditSettingsLocation(Location location)
        {
            _logger.LogInformation("Loading Started for Add_EditSettingsLocation for User: " + mUserId);

            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                List<string> lstLocation = clinicalcaseOperations.GetLocations().Select(x => x.Name).ToList();
                if (!lstLocation.Contains(location.Name))
                {
                    if (location.LocationId == 0)
                    {
                        clinicalcaseOperations.AddLocation(location);
                        TempData["Success"] = "Location \"" + location.Name + "\" Added Successfully!";
                    }
                    else
                    {
                        clinicalcaseOperations.UpdateLocation(location);
                        TempData["Success"] = "Location \"" + location.Name + "\" Updated Successfully!";
                    }
                }
                else
                {
                    TempData["Error"] = "The Location \"" + location.Name + "\" is already present in our Location list!";
                }
            }
            _logger.LogInformation("Loading Ended for Add_EditSettingsLocation for User: " + mUserId);
            return RedirectToAction("SettingsLocation");
        }
        [HttpGet]
        public IActionResult DeleteLocation(int id)
        {
            _logger.LogInformation("Loading Started for Fetching DeleteLocation for User: " + mUserId);
            Location obj = new Location();
            if (id != 0)
            {
                List<Location> lstLocation = new List<Location>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstLocation = clinicalcaseOperations.GetLocations();
                var res = lstLocation.Where(a => a.LocationId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Fetching DeleteLocation for User: " + mUserId);
            return PartialView("_DeleteLocation", obj);
        }

        [HttpPost]
        public IActionResult DeleteLocation(Location location)
        {
            try
            {
                _logger.LogInformation("Loading Started for Submit DeleteLocation for User: " + mUserId);
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (location.LocationId != 0)
                {
                    clinicalcaseOperations.DeletetLocation(location.LocationId);
                    TempData["Success"] = "Location \"" + location.Name + "\" Deleted Successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for Submit DeleteLocation for User: " + mUserId);
            return RedirectToAction("SettingsLocation");
        }
        [HttpGet]
        public IActionResult SettingsListName()
        {
            _logger.LogInformation("Loading Started for SettingsListName for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.lstnames = clinicalcaseOperations.GetLists();
            _logger.LogInformation("Loading Ended for SettingsListName for User: " + mUserId);
            return View();
        }
        [HttpGet]
        public ActionResult AddListName()
        {
            return PartialView("_AddListName");
        }
        [HttpGet]
        public ActionResult EditListName(long id = 0)
        {
            _logger.LogInformation("Loading Started for EditListName for User: " + mUserId);
            List obj = new List();
            if (id != 0)
            {
                List<List> lstnames = new List<List>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstnames = clinicalcaseOperations.GetLists();
                var res = lstnames.Where(a => a.ListId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for EditListName for User: " + mUserId);
            return PartialView("_UpdateListName", obj);
        }
        [HttpPost]
        public IActionResult AddListName(List list)
        {
            _logger.LogInformation("Loading Started for AddListName for User: " + mUserId);

            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                var lstName = clinicalcaseOperations.GetLists().Where(x => x.ListId == list.ListId && x.Name == list.Name).FirstOrDefault();
                if (lstName == null)
                {
                    clinicalcaseOperations.AddListname(list);
                    TempData["Success"] = "Location \"" + list.Name + "\" Added Successfully!";
                }
                else
                {
                    TempData["Error"] = "The Location \"" + list.Name + "\" is already present in our Location list!";
                }
            }
            _logger.LogInformation("Loading Ended for AddListName for User: " + mUserId);
            return RedirectToAction("SettingsListName");
        }
        [HttpPost]
        public IActionResult EditListName(List list)
        {
            _logger.LogInformation("Loading Started for EditListName for User: " + mUserId);
            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                var lstName = clinicalcaseOperations.GetLists().Where(x => x.ListId == list.ListId).FirstOrDefault();
                if (lstName != null)
                {
                    clinicalcaseOperations.UpdateListname(list);
                    TempData["Success"] = "List \"" + list.Name + "\" Updated Successfully!";
                }
                else
                {
                    TempData["Error"] = "List \"" + list.Name + "\" is already present in our  list Name!";
                }
            }
            _logger.LogInformation("Loading Ended for EditListName for User: " + mUserId);
            return RedirectToAction("SettingsListName");
        }
        [HttpGet]
        public IActionResult DeleteListName(long id)
        {
            _logger.LogInformation("Loading Started for Fetching DeleteListName for User: " + mUserId);
            List obj = new List();
            if (id != 0)
            {
                List<List> lstList = new List<List>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstList = clinicalcaseOperations.GetLists();
                var res = lstList.Where(a => a.ListId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Fetching DeleteListName for User: " + mUserId);
            return PartialView("_DeleteListName", obj);
        }

        [HttpPost]
        public IActionResult DeleteListName(List list)
        {
            try
            {
                _logger.LogInformation("Loading Started for Submit DeleteListName for User: " + mUserId);
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (list.ListId != 0)
                {
                    clinicalcaseOperations.DeletetListname(list.ListId);
                    TempData["Success"] = "List \"" + list.Name + "\" Deleted Successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for Submit DeleteListName for User: " + mUserId);
            return RedirectToAction("SettingsListName");
        }


        public IActionResult AddSettingsProvider(Provider provider)
        {
            _logger.LogInformation("Loading Started for AddSettingsProvider for User: " + mUserId);

            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                List<string> lstProvider = clinicalcaseOperations.GetProviderNames();
                if (provider.ProviderId == 0)
                {
                    if (!lstProvider.Contains(provider.Name.ToLower()))
                    {
                        clinicalcaseOperations.AddProvider(provider);
                        TempData["Success"] = "Provider \"" + provider.Name + "\" Added Successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "The Provider \"" + provider.Name + "\" is already present in our Provider list!";
                    }
                }
                else
                {
                    clinicalcaseOperations.UpdateProvider(provider); // Update
                    TempData["Success"] = "Provider \"" + provider.Name + "\" Updated Successfully!";
                }
            }
            _logger.LogInformation("Loading Ended for AddSettingsProvider for User: " + mUserId);
            return RedirectToAction("SettingsProvider");
        }

        [HttpGet]
        public IActionResult SettingsProvider()
        {
            _logger.LogInformation("Loading Started for SettingsProvider for User: " + mUserId);
            List<Provider> lstProvider = new List<Provider>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstProvider = clinicalcaseOperations.GetProviders();
            ViewBag.lstProvider = lstProvider;
            _logger.LogInformation("Loading Ended for SettingsProvider for User: " + mUserId);
            return View();
        }

        [HttpGet]
        public ActionResult Add_EditProvider(int id = 0)
        {
            _logger.LogInformation("Loading Started for Add_EditProvider for User: " + mUserId);
            Provider obj = new Provider();
            if (id != 0)
            {
                List<Provider> lstProvider = new List<Provider>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstProvider = clinicalcaseOperations.GetProviders();
                var res = lstProvider.Where(a => a.ProviderId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Add_EditProvider for User: " + mUserId);
            return PartialView("_AddEditProvider", obj);
        }

        [HttpGet]
        public IActionResult DeleteProvider(int id)
        {
            _logger.LogInformation("Loading Started for Fetching DeleteProvider for User: " + mUserId);
            Provider obj = new Provider();
            if (id != 0)
            {
                List<Provider> lstProvider = new List<Provider>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstProvider = clinicalcaseOperations.GetProviders();
                var res = lstProvider.Where(a => a.ProviderId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Fetching DeleteProvider for User: " + mUserId);
            return PartialView("_DeleteProvider", obj);
        }

        [HttpPost]
        public IActionResult DeleteProvider(Provider provider)
        {
            try
            {
                _logger.LogInformation("Loading Started for Submit DeleteProvider for User: " + mUserId);
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (provider.ProviderId != 0)
                    clinicalcaseOperations.DeleteProvider(provider); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for Submit DeleteProvider for User: " + mUserId);
            return RedirectToAction("SettingsProvider");
        }

        [HttpPost]
        public IActionResult AddSettingsPayor(Payor payor)
        {
            _logger.LogInformation("Loading Started for AddSettingsPayor for User: " + mUserId);

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
            _logger.LogInformation("Loading Ended for AddSettingsPayor for User: " + mUserId);
            return RedirectToAction("SettingsPayor");
        }


        [HttpGet]
        public ActionResult Add_EditPayor(int id = 0)
        {
            _logger.LogInformation("Loading Started for Add_EditPayor for User: " + mUserId);
            Payor obj = new Payor();
            if (id != 0)
            {
                List<Payor> lstPayor = new List<Payor>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstPayor = clinicalcaseOperations.GetPayors();
                var res = lstPayor.Where(a => a.PayorId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Add_EditPayor for User: " + mUserId);
            return PartialView("_AddEditPayor", obj);
        }

        [HttpGet]
        public IActionResult DeletePayor(int id)
        {
            _logger.LogInformation("Loading Started for Fetching DeletePayor for User: " + mUserId);
            Payor obj = new Payor();
            if (id != 0)
            {
                List<Payor> lstPayor = new List<Payor>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstPayor = clinicalcaseOperations.GetPayors();
                var res = lstPayor.Where(a => a.PayorId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Fetching DeletePayor for User: " + mUserId);
            return PartialView("_DeletePayor", obj);
        }

        [HttpPost]
        public IActionResult DeletePayor(Payor payor)
        {
            try
            {
                _logger.LogInformation("Loading Started for Submit DeletePayor for User: " + mUserId);
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (payor.PayorId != 0)
                    clinicalcaseOperations.DeletePayor(payor); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for Submit DeletePayor for User: " + mUserId);
            return RedirectToAction("SettingsPayor");
        }

        [HttpGet]
        public IActionResult SettingsPayor()
        {
            _logger.LogInformation("Loading Started for SettingsPayor for User: " + mUserId);
            List<Payor> lstPayor = new List<Payor>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstPayor = clinicalcaseOperations.GetPayors();
            ViewBag.lstPayor = lstPayor;
            _logger.LogInformation("Loading Ended for SettingsPayor for User: " + mUserId);
            return View();
        }

        [HttpPost]
        public IActionResult AddSettingsErrorType(ErrorType errorType)
        {
            _logger.LogInformation("Loading Started for AddSettingsErrorType for User: " + mUserId);
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
            _logger.LogInformation("Loading Ended for AddSettingsErrorType for User: " + mUserId);
            return RedirectToAction("SettingsErrorType");
        }


        [HttpGet]
        public ActionResult Add_EditErrorType(int id = 0)
        {
            _logger.LogInformation("Loading Started for Add_EditErrorType for User: " + mUserId);
            ErrorType obj = new ErrorType();
            if (id != 0)
            {
                List<ErrorType> lstErrorType = new List<ErrorType>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstErrorType = clinicalcaseOperations.GetErrorTypes();
                var res = lstErrorType.Where(a => a.ErrorTypeId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Add_EditErrorType for User: " + mUserId);

            return PartialView("_AddEditErrorType", obj);
        }

        [HttpGet]
        public IActionResult DeleteErrorType(int id)
        {
            _logger.LogInformation("Loading Started for Fetching DeleteErrorType for User: " + mUserId);

            ErrorType obj = new ErrorType();
            if (id != 0)
            {
                List<ErrorType> lstErrorType = new List<ErrorType>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstErrorType = clinicalcaseOperations.GetErrorTypes();
                var res = lstErrorType.Where(a => a.ErrorTypeId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Fetching DeleteErrorType for User: " + mUserId);
            return PartialView("_DeleteErrorType", obj);
        }

        [HttpPost]
        public IActionResult DeleteErrorType(ErrorType errorType)
        {
            try
            {
                _logger.LogInformation("Loading Started for Submit DeleteErrorType for User: " + mUserId);
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (errorType.ErrorTypeId != 0)
                    clinicalcaseOperations.DeleteErrorType(errorType); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for Submit DeleteErrorType for User: " + mUserId);
            return RedirectToAction("SettingsErrorType");
        }

        [HttpGet]
        public IActionResult SettingsErrorType()
        {
            _logger.LogInformation("Loading Started for SettingsErrorType for User: " + mUserId);
            List<ErrorType> lstErrorType = new List<ErrorType>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstErrorType = clinicalcaseOperations.GetErrorTypes();
            ViewBag.lstErrorType = lstErrorType;
            _logger.LogInformation("Loading Ended for SettingsErrorType for User: " + mUserId);
            return View();
        }

        [HttpPost]
        public IActionResult AddSettingsProviderFeedback(BindDTO providerFeedback)
        {
            _logger.LogInformation("Loading Started for AddSettingsProviderFeedback for User: " + mUserId);
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
            _logger.LogInformation("Loading Ended for AddSettingsProviderFeedback for User: " + mUserId);
            return RedirectToAction("SettingsProviderFeedback");
        }
        [HttpGet]
        public IActionResult SettingsProviderFeedback()
        {
            _logger.LogInformation("Loading Started for SettingsProviderFeedback for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            ViewBag.lstProviderFeedback = clinicalcaseOperations.GetProviderFeedbacksList();
            _logger.LogInformation("Loading Ended for SettingsProviderFeedback for User: " + mUserId);
            return View();
        }

        [HttpGet]
        public ActionResult Add_EditProviderFeedback(int id = 0)
        {
            _logger.LogInformation("Loading Started for Add_EditProviderFeedback for User: " + mUserId);
            BindDTO obj = new BindDTO();
            if (id != 0)
            {
                List<BindDTO> lstproviderFeedback = new List<BindDTO>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

                lstproviderFeedback = clinicalcaseOperations.GetProviderFeedbacksList();
                var res = lstproviderFeedback.Where(a => a.ID == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Add_EditProviderFeedback for User: " + mUserId);
            return PartialView("_AddEditProviderFeedback", obj);
        }

        [HttpGet]
        public IActionResult DeleteProviderFeedback(int id)
        {
            _logger.LogInformation("Loading Started for Fetching DeleteProviderFeedback for User: " + mUserId);
            BindDTO obj = new BindDTO();
            if (id != 0)
            {
                List<BindDTO> lstproviderFeedback = new List<BindDTO>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

                lstproviderFeedback = clinicalcaseOperations.GetProviderFeedbacksList();
                var res = lstproviderFeedback.Where(a => a.ID == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Fetching DeleteProviderFeedback for User: " + mUserId);
            return PartialView("_DeleteProviderFeedback", obj);
        }

        [HttpPost]
        public IActionResult DeleteProviderFeedback(BindDTO providerFeedback)
        {
            try
            {
                _logger.LogInformation("Loading Started for Submit DeleteProviderFeedback for User: " + mUserId);
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (providerFeedback.ID != 0)
                    clinicalcaseOperations.DeleteProviderFeedback(providerFeedback); // Delete
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for Submit DeleteProviderFeedback for User: " + mUserId);
            return RedirectToAction("SettingsProviderFeedback");
        }

        [HttpGet]
        public IActionResult SettingsProject()
        {
            _logger.LogInformation("Loading Started for SettingsProject for User: " + mUserId);

            List<ApplicationProject> lstProject = new List<ApplicationProject>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lstProject = clinicalcaseOperations.GetProjects();
            ViewBag.lstProject = lstProject;
            _logger.LogInformation("Loading Ended for SettingsProject for User: " + mUserId);
            return View();
        }

        [HttpPost]
        public IActionResult AddSettingsProject(ApplicationProject project)
        {
            _logger.LogInformation("Loading Started for AddSettingsProject for User: " + mUserId);
            try
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
                            TempData["Success"] = "Project \"" + project.Name + "\" Added Successfully!";
                        }
                        else
                        {
                            TempData["Error"] = "The Project \"" + project.Name + "\" is already present in our Project list!";
                        }
                    }
                    else
                    {
                        clinicalcaseOperations.UpdateProject(project); // Update
                        TempData["Success"] = "Project \"" + project.Name + "\" Updated Successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for AddSettingsProject for User: " + mUserId);
            return RedirectToAction("SettingsProject");
        }

        [HttpGet]
        public ActionResult Add_EditProject(int id = 0)
        {
            _logger.LogInformation("Loading Started for Add_EditProject for User: " + mUserId);
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
                _logger.LogInformation("Loading Ended for Add_EditProject for User: " + mUserId);
                return PartialView("_AddEditProject", obj);
            }
            _logger.LogInformation("Loading Ended for Add_EditProject for User: " + mUserId);
            return PartialView("_AddEditProject", obj);
        }


        [HttpGet]
        public IActionResult SettingsCptAudit()
        {
            _logger.LogInformation("Loading Started for SettingsCptAudit for User: " + mUserId);

            List<CptAudit> lst = new List<CptAudit>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            lst = clinicalcaseOperations.GetCptAudits();
            ViewBag.lstcptaudit = lst;
            ViewBag.projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for SettingsCptAudit for User: " + mUserId);
            return View();
        }
        [HttpGet]
        public ActionResult Add_EditCptAudit(int id = 0)
        {
            _logger.LogInformation("Loading Started for Add_EditCptAudit for User: " + mUserId);
            CptAudit obj = new CptAudit();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            if (id != 0)
            {
                List<CptAudit> lstaudit = new List<CptAudit>();
                lstaudit = clinicalcaseOperations.GetCptAudits();
                var res = lstaudit.Where(a => a.CPTAuditId == id).FirstOrDefault();
                obj = res;
                _logger.LogInformation("Loading Ended for Add_EditCptAudit for User: " + mUserId);
                return PartialView("_Add_EditCptAudit", obj);
            }
            _logger.LogInformation("Loading Ended for Add_EditCptAudit for User: " + mUserId);
            return PartialView("_Add_EditCptAudit", obj);
        }
        [HttpPost]
        public IActionResult AddSettingsCptAudit(CptAudit cptAudit)
        {
            _logger.LogInformation("Loading Started for AddSettingsCptAudit for User: " + mUserId);
            try
            {
                if (ModelState.IsValid)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                    if (cptAudit.CPTAuditId == 0)
                    {
                        clinicalcaseOperations.AddCptAudit(cptAudit);
                        TempData["Success"] = "CPT Code \"" + cptAudit.CPTCode + "\" Added Successfully!";

                    }
                    else
                    {
                        clinicalcaseOperations.UpdateCptAudit(cptAudit); // Update
                        TempData["Success"] = "CPT Code \"" + cptAudit.CPTCode + "\" Updated Successfully!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for AddSettingsCptAudit for User: " + mUserId);
            return RedirectToAction("SettingsCptAudit");
        }
        [HttpGet]
        public IActionResult DeleteCptAudit(int id)
        {
            _logger.LogInformation("Loading Started for Fetching DeleteCptAudit for User: " + mUserId);
            CptAudit obj = new CptAudit();
            if (id != 0)
            {
                List<CptAudit> lst = new List<CptAudit>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lst = clinicalcaseOperations.GetCptAudits();
                var res = lst.Where(a => a.CPTAuditId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Fetching DeleteCptAudit for User: " + mUserId);
            return PartialView("_DeleteCptAudit", obj);
        }
        [HttpPost]
        public IActionResult DeleteCptAudit(CptAudit cptAudit)
        {
            try
            {
                _logger.LogInformation("Loading Started for Submit DeleteCptAudit for User: " + mUserId);
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (cptAudit.CPTAuditId != 0)
                    clinicalcaseOperations.DeleteCptAudit(cptAudit);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for Submit DeleteCptAudit for User: " + mUserId);
            return RedirectToAction("SettingsCptAudit");
        }



        [HttpPost]
        public IActionResult DeleteProject(ApplicationProject project)
        {
            try
            {
                _logger.LogInformation("Loading Started for Submit DeleteProject for User: " + mUserId);
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                if (project.ProjectId != 0)
                    clinicalcaseOperations.DeleteProject(project);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            _logger.LogInformation("Loading Ended for Submit DeleteProject for User: " + mUserId);
            return RedirectToAction("SettingsProject");
        }

        [HttpGet]
        public IActionResult DeleteProject(int id)
        {
            _logger.LogInformation("Loading Started for Fetching DeleteProject for User: " + mUserId);
            ApplicationProject obj = new ApplicationProject();
            if (id != 0)
            {
                List<ApplicationProject> lstProject = new List<ApplicationProject>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                lstProject = clinicalcaseOperations.GetProjects();
                var res = lstProject.Where(a => a.ProjectId == id).FirstOrDefault();
                obj = res;
            }
            _logger.LogInformation("Loading Ended for Fetching DeleteProject for User: " + mUserId);
            return PartialView("_DeleteProject", obj);
        }
        #endregion
    }
}
