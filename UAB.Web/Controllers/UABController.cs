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
            if (Role == Roles.QA.ToString())
            {
                lst = clinicalcaseOperations.DisplayBlockCharts(Role, ProjectID);
            }
            else
            {
                lst = clinicalcaseOperations.GetBlockNext(Role, ChartType, ProjectID);
            }
            var projects = clinicalcaseOperations.GetProjects();
            ViewBag.Role = Role;
            ViewBag.Project = projects.Where(a => a.ProjectId.Equals(ProjectID)).FirstOrDefault().Name;
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

            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();                                //coding,ShadowQA
            List<ChartSummaryDTO> chartSummaryDTOlst = new List<ChartSummaryDTO>();                 //coding,ShadowQA
            List<ChartSummaryDTO> qadto = new List<ChartSummaryDTO>();                              //QA


            if (ProjectName == null || ProjectID == 0)
            {
                var projects = clinicalcaseOperations.GetProjects();
                if (ProjectName == null)
                {
                    ProjectName = projects.Where(a => a.ProjectId == ProjectID).Select(a => a.Name).FirstOrDefault();
                }
                else if (ProjectID == 0)
                {
                    ProjectID = projects.Where(a => a.Name == ProjectName).Select(a => a.ProjectId).FirstOrDefault();
                }
            }
            if (Role == Roles.QA.ToString())
            {
                var res = clinicalcaseOperations.DisplayBlockCharts(Role, ProjectID);
                List<int> ccids = res.Select(a => a.CodingDTO.ClinicalCaseID).ToList();
                int searchcid = Convert.ToInt32(ccid);
                if (ccids.Count != 0)
                {
                    switch (plusorminus)
                    {
                        case "Next":
                            searchcid = ccids[(ccids.IndexOf(Convert.ToInt32(ccid)) + 1) % ccids.Count];
                            break;
                        case "Previous":
                            searchcid = ccids[(ccids.IndexOf(Convert.ToInt32(ccid)) - 1 + ccids.Count) % ccids.Count];
                            break;
                        default:
                            searchcid = Convert.ToInt32(ccid);
                            break;
                    }
                    int currentindex = ccids.IndexOf(searchcid);
                    ViewBag.currentindex = currentindex;
                    ViewBag.lastindex = ccids.Count - 1;
                }
                qadto = clinicalcaseOperations.GetQABlockedChart(Role, ChartType, ProjectID, searchcid);
                qadto.FirstOrDefault().ProjectName = ProjectName;
            }
            else if (Role == Roles.Coder.ToString())
            {
                chartSummaryDTOlst = clinicalcaseOperations.GetBlockNext(Role, ChartType, ProjectID);  //total block charts
                List<int> cidslst = chartSummaryDTOlst.Select(x => x.CodingDTO.ClinicalCaseID).ToList();
                switch (plusorminus)
                {
                    case "Next":
                        chartSummaryDTO = chartSummaryDTOlst.SkipWhile(x => !x.CodingDTO.ClinicalCaseID.Equals(Convert.ToInt32(ccid))).Skip(1).FirstOrDefault();
                        if (chartSummaryDTO == null)
                        {
                            chartSummaryDTO = chartSummaryDTOlst.Where(c => c.CodingDTO.ClinicalCaseID == Convert.ToInt32(ccid)).FirstOrDefault();
                            chartSummaryDTO.ProjectName = ProjectName;
                        }
                        break;
                    case "Previous":
                        var x = chartSummaryDTOlst;
                        x.Reverse();
                        chartSummaryDTO = x.SkipWhile(x => !x.CodingDTO.ClinicalCaseID.Equals(Convert.ToInt32(ccid))).Skip(1).FirstOrDefault();
                        if (chartSummaryDTO == null)
                        {
                            chartSummaryDTO = chartSummaryDTOlst.Where(c => c.CodingDTO.ClinicalCaseID == Convert.ToInt32(ccid)).FirstOrDefault();
                            chartSummaryDTO.ProjectName = ProjectName;
                        }
                        break;

                    default:
                        chartSummaryDTO = chartSummaryDTOlst.Where(c => c.CodingDTO.ClinicalCaseID == Convert.ToInt32(ccid)).FirstOrDefault();
                        chartSummaryDTO.ProjectName = ProjectName;
                        break;
                }
                int indcid = chartSummaryDTO.CodingDTO.ClinicalCaseID;
                int currentindex = cidslst.IndexOf(indcid);
                ViewBag.currentindex = currentindex;
                ViewBag.lastindex = cidslst.Count - 1;
            }
            else if (Role == Roles.ShadowQA.ToString())
            {
                chartSummaryDTOlst = clinicalcaseOperations.GetBlockNext(Role, ChartType, ProjectID);  //total block charts
                List<int> cidslst = chartSummaryDTOlst.Select(x => x.CodingDTO.ClinicalCaseID).ToList();
                switch (plusorminus)
                {
                    case "Next":
                        chartSummaryDTO = chartSummaryDTOlst.SkipWhile(x => !x.CodingDTO.ClinicalCaseID.Equals(Convert.ToInt32(ccid))).Skip(1).FirstOrDefault();
                        if (chartSummaryDTO == null)
                        {
                            chartSummaryDTO = chartSummaryDTOlst.Where(c => c.CodingDTO.ClinicalCaseID == Convert.ToInt32(ccid)).FirstOrDefault();
                            chartSummaryDTO.ProjectName = ProjectName;
                        }
                        break;
                    case "Previous":
                        var x = chartSummaryDTOlst;
                        x.Reverse();
                        chartSummaryDTO = x.SkipWhile(x => !x.CodingDTO.ClinicalCaseID.Equals(Convert.ToInt32(ccid))).Skip(1).FirstOrDefault();
                        if (chartSummaryDTO == null)
                        {
                            chartSummaryDTO = chartSummaryDTOlst.Where(c => c.CodingDTO.ClinicalCaseID == Convert.ToInt32(ccid)).FirstOrDefault();
                            chartSummaryDTO.ProjectName = ProjectName;
                        }
                        break;

                    default:
                        chartSummaryDTO = chartSummaryDTOlst.Where(c => c.CodingDTO.ClinicalCaseID == Convert.ToInt32(ccid)).FirstOrDefault();
                        chartSummaryDTO.ProjectName = ProjectName;
                        break;
                }
                int indcid = chartSummaryDTO.CodingDTO.ClinicalCaseID;
                int currentindex = cidslst.IndexOf(indcid);
                ViewBag.currentindex = currentindex;
                ViewBag.lastindex = cidslst.Count - 1;
            }

            ViewBag.IsBlocked = "1";

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion

            if (Role == Roles.QA.ToString())
                return View("QA", qadto);
            else if (Role == "ShadowQA")
                return View("ShadowQA", chartSummaryDTO);
            else
                return View("Coding", chartSummaryDTO);

        }

        public IActionResult GetCodingBlockedChart(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            List<ChartSummaryDTO> lstChartSummaryDTO = new List<ChartSummaryDTO>();

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            ViewBag.IsBlocked = "1";

            ViewBag.currentindex = 0;            //always first will open from here so currentindex is zero

            #endregion

            chartSummaryDTO.ProjectName = ProjectName;

            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            switch (Role)
            {
                case "Coder":
                    chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
                    //for prenxt button disblenable operation
                    lst = clinicalcaseOperations.GetBlockNext(Role, ChartType, ProjectID);
                    List<int> codercidslst = lst.Select(x => x.CodingDTO.ClinicalCaseID).ToList();
                    ViewBag.lastindex = codercidslst.Count - 1;
                    break;
                case "QA":
                    lstChartSummaryDTO = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
                    if (lstChartSummaryDTO.Count > 0)
                        lstChartSummaryDTO.FirstOrDefault().ProjectName = ProjectName;
                    //for prenxt button disblenable operation
                    lst = clinicalcaseOperations.DisplayBlockCharts(Role, ProjectID);
                    List<int> qacidslst = lst.Select(x => x.CodingDTO.ClinicalCaseID).ToList();
                    ViewBag.lastindex = qacidslst.Count - 1;
                    break;
                case "ShadowQa":
                    chartSummaryDTO = clinicalcaseOperations.GetNext(Role, ChartType, ProjectID);
                    //for prenxt button disblenable operation
                    lst = clinicalcaseOperations.GetBlockNext(Role, ChartType, ProjectID);
                    List<int> shadowqacidslst = lst.Select(x => x.CodingDTO.ClinicalCaseID).ToList();
                    ViewBag.lastindex = shadowqacidslst.Count - 1;
                    break;
            }
            if (Role == "QA")
                return View("QA", lstChartSummaryDTO.OrderBy(a => a.ClaimId).ToList());
            else if (Role == "ShadowQA")
                return View("ShadowQA", chartSummaryDTO);
            else
                return View("Coding", chartSummaryDTO);

        }
        [HttpGet]
        public IActionResult ProviderPostedClinicalcase(string ccid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.ccid = Convert.ToInt32(ccid);
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            return PartialView("_ProviderPosted");
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
            List<ChartSummaryDTO> chartSummaryDTO = new List<ChartSummaryDTO>();
            chartSummaryDTO = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            if (chartSummaryDTO.Count == 0)
            {
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("CodingSummary");
            }
            chartSummaryDTO.FirstOrDefault().ProjectName = ProjectName;
            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
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
            foreach (var item in lstcpts.OrderBy(a => a.Split("^")[0]).ToList())
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string[] lstcptrow = item.Split("^");
                    dtCPT.Rows.Add(lstcptrow[1], lstcptrow[2], lstcptrow[3], lstcptrow[4], claimId);
                }
            }
        }

        private void PrepareDxCodes(string dx, DataTable dtDx, int claimId)
        {
            string[] lstdxs = dx.Split(",");
            foreach (var item in lstdxs)
            {
                string[] lstdxrow = item.Split("^");
                dtDx.Rows.Add(lstdxrow[0], claimId);
            }
        }

        public IActionResult SubmitCodingAvailableChart(ChartSummaryDTO chartSummaryDTO, string codingSubmitAndGetNext, string submitAndPost, string submitOnly, int providerId, DateTime txtPostingDate, string txtCoderComment)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            string providerPosted = Request.Form["hdnProviderPosted"].ToString();

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

            if (providerPosted != "")
            {
                clinicalcaseOperations.SubmitProviderPostedChart(chartSummaryDTO, dtClaim, dtCpt,providerId,txtPostingDate,txtCoderComment);
            }
            else
            {
                if (codingSubmitAndGetNext == "codingSubmit")
                    clinicalcaseOperations.SubmitCodingAvailableChart(chartSummaryDTO, dtClaim, dtCpt);
                else
                {
                    clinicalcaseOperations.SubmitCodingAvailableChart(chartSummaryDTO, dtClaim, dtCpt);
                    return RedirectToAction("GetCodingAvailableChart", new { Role = Roles.Coder.ToString(), ChartType = "Available", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
                }
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
        public IActionResult GetAuditDetails(string chartType, int projectId, string dt)
        {
            bool auditFlag = IsAuditRequired(chartType, projectId, dt);
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
            var hdnStatusId = Request.Form["hdnStatusId"].ToString();

            string hdnClaim1 = Request.Form["hdnClaim1"].ToString();
            string hdnClaim2 = Request.Form["hdnClaim2"].ToString();
            string hdnClaim3 = Request.Form["hdnClaim3"].ToString();
            string hdnClaim4 = Request.Form["hdnClaim4"].ToString();

            string hdnAcceptedClaim1 = Request.Form["hdnAcceptedClaim1"].ToString();
            string hdnAcceptedClaim2 = Request.Form["hdnAcceptedClaim2"].ToString();
            string hdnAcceptedClaim3 = Request.Form["hdnAcceptedClaim3"].ToString();
            string hdnAcceptedClaim4 = Request.Form["hdnAcceptedClaim4"].ToString();

            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ClaimId", typeof(int));

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

            var hdnClaimId1 = Request.Form["hdnClaimId1"].ToString();
            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            // Rejected basic Params

            if (!string.IsNullOrEmpty(hdnClaim1))
                PrepareAudit(hdnClaim1, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim2))
                PrepareAudit(hdnClaim2, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim3))
                PrepareAudit(hdnClaim3, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim4))
                PrepareAudit(hdnClaim4, dtAudit);

            // Rejected Dx Codes

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes))
                dtAudit.Rows.Add("Dx", hdnRejectedDxCodes, hdnRejectedDxRemarks, Convert.ToInt32(hdnClaimId1));

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes1))
                dtAudit.Rows.Add("Dx", hdnRejectedDxCodes1, hdnRejectedDxRemarks1, Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes2))
                dtAudit.Rows.Add("Dx", hdnRejectedDxCodes2, hdnRejectedDxRemarks2, Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnRejectedDxCodes3))
                dtAudit.Rows.Add("Dx", hdnRejectedDxCodes3, hdnRejectedDxRemarks3, Convert.ToInt32(hdnClaimId4));

            // Rejected CPT Codes
            if (!string.IsNullOrEmpty(hdnRejectedCptCodes))
                dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes, hdnRejectedCptRemarks, Convert.ToInt32(hdnClaimId1));

            if (!string.IsNullOrEmpty(hdnRejectedCptCodes1))
                dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes1, hdnRejectedCptRemarks1, Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnRejectedCptCodes2))
                dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes2, hdnRejectedCptRemarks2, Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnRejectedCptCodes3))
                dtAudit.Rows.Add("CPTCode", hdnRejectedCptCodes3, hdnRejectedCptRemarks3, Convert.ToInt32(hdnClaimId4));

            // Accepetd basic Params

            if (!string.IsNullOrEmpty(hdnAcceptedClaim1))
                PrepareBasicParams(hdnAcceptedClaim1, dtbasicParams);

            if (!string.IsNullOrEmpty(hdnAcceptedClaim2))
                PrepareBasicParams(hdnAcceptedClaim2, dtbasicParams);

            if (!string.IsNullOrEmpty(hdnAcceptedClaim3))
                PrepareBasicParams(hdnAcceptedClaim3, dtbasicParams);

            if (!string.IsNullOrEmpty(hdnAcceptedClaim4))
                PrepareBasicParams(hdnAcceptedClaim4, dtbasicParams);

            // Accepetd Dx Codes

            if (!string.IsNullOrEmpty(hdnDxCodes))
                PrepareDxCodes(hdnDxCodes, dtDx, Convert.ToInt32(hdnClaimId1));

            if (!string.IsNullOrEmpty(hdnDxCodes1))
                PrepareDxCodes(hdnDxCodes1, dtDx, Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnDxCodes2))
                PrepareDxCodes(hdnDxCodes2, dtDx, Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnDxCodes3))
                PrepareDxCodes(hdnDxCodes3, dtDx, Convert.ToInt32(hdnClaimId4));

            // Accepetd CPT Codes

            if (!string.IsNullOrEmpty(hdnCptCodes))
                PrepareCptCodes(hdnCptCodes, dtCpt, Convert.ToInt32(hdnClaimId1));

            if (!string.IsNullOrEmpty(hdnCptCodes1))
                PrepareCptCodes(hdnCptCodes1, dtCpt, Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnCptCodes2))
                PrepareCptCodes(hdnCptCodes2, dtCpt, Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnCptCodes3))
                PrepareCptCodes(hdnCptCodes3, dtCpt, Convert.ToInt32(hdnClaimId4));



            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitCodingIncorrectChart(chartSummaryDTO, Convert.ToInt16(hdnStatusId), dtAudit, dtbasicParams, dtDx, dtCpt);

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.Coder.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("CodingSummary", lstDto);
        }
        public IActionResult SubmitCodingReadyForPostingChart(ChartSummaryDTO chartSummaryDTO, string postingSubmitAndGetNext)
        {
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

            TempData["Success"] = "Chart Details posted successfully !";
            return View("CodingSummary", lstDto);
        }

        [HttpGet]
        public IActionResult GetReadyforPostingPopup(string buttonType)
        {
            ViewBag.buttonType = buttonType;
            return PartialView("_ReadyForPostingSubmitPopup");
        }

        [HttpGet]
        public IActionResult AddNewClaim(int claimID, int pid1, int pid2, int pid3, int pid4)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var prlst = clinicalcaseOperations.GetProvidersList();
            prlst.RemoveAll(x => x.ID == pid1 || x.ID == pid2 || x.ID == pid3 || x.ID == pid4);
            ViewBag.Providers = prlst;
            //ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ClaimId = claimID;
            return PartialView("_CodingClaim");
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
            if (lstchartSummary.Count > 0)
                lstchartSummary.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            if (lstchartSummary.Count == 0)
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
            if (lstchartSummary.Count > 0)
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
            List<ChartSummaryDTO> lstChartSummaryDTO = new List<ChartSummaryDTO>();
            lstChartSummaryDTO = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            lstChartSummaryDTO.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
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
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

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

            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            string hdnClaim2 = Request.Form["hdnClaim2"].ToString();
            string hdnClaim3 = Request.Form["hdnClaim3"].ToString();
            string hdnClaim4 = Request.Form["hdnClaim4"].ToString();

            if (!string.IsNullOrEmpty(hdnClaim2))
                PrepareAudit(hdnClaim2, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim3))
                PrepareAudit(hdnClaim3, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim4))
                PrepareAudit(hdnClaim4, dtAudit);

            string hdnQADxRemarks2 = Request.Form["hdnQADxRemarks2"].ToString();
            string hdnQADxCodes2 = Request.Form["hdnQADxCodes2"].ToString();
            string hdnQADxRemarks3 = Request.Form["hdnQADxRemarks3"].ToString();
            string hdnQADxCodes3 = Request.Form["hdnQADxCodes3"].ToString();
            string hdnQADxRemarks4 = Request.Form["hdnQADxRemarks4"].ToString();
            string hdnQADxCodes4 = Request.Form["hdnQADxCodes4"].ToString();

            string hdnQAErrorTypeID2 = Request.Form["hdnQAErrorTypeID2"].ToString();
            string hdnQAErrorTypeID3 = Request.Form["hdnQAErrorTypeID3"].ToString();
            string hdnQAErrorTypeID4 = Request.Form["hdnQAErrorTypeID4"].ToString();

            if (!string.IsNullOrEmpty(hdnQADxCodes2))
                dtAudit.Rows.Add("Dx", hdnQADxCodes2, hdnQADxRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnQADxCodes3))
                dtAudit.Rows.Add("Dx", hdnQADxCodes3, hdnQADxRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnQADxCodes4))
                dtAudit.Rows.Add("Dx", hdnQADxCodes4, hdnQADxRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4));

            string hdnQACptRemarks2 = Request.Form["hdnQACptRemarks2"].ToString();
            string hdnQACptCodes2 = Request.Form["hdnQACptCodes2"].ToString();
            string hdnQACptRemarks3 = Request.Form["hdnQACptRemarks3"].ToString();
            string hdnQACptCodes3 = Request.Form["hdnQACptCodes3"].ToString();
            string hdnQACptRemarks4 = Request.Form["hdnQACptRemarks4"].ToString();
            string hdnQACptCodes4 = Request.Form["hdnQACptCodes4"].ToString();

            if (!string.IsNullOrEmpty(hdnQACptCodes2))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes2, hdnQACptRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnQACptCodes3))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes3, hdnQACptRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnQACptCodes4))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes4, hdnQACptRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4));

            //Ending of Reading Claim2 to Claim4 Data

            var hdnQADx = Request.Form["hdnQADxCodes"].ToString();
            var hdnQADxRemarks = Request.Form["hdnQADxRemarks"].ToString();
            chartSummaryDTO.QADx = hdnQADx;
            chartSummaryDTO.QADxRemarks = hdnQADxRemarks;

            var hdnQACptCodes = Request.Form["hdnQACptCodes"].ToString();
            var hdnQACptRemarks = Request.Form["hdnQACptRemarks"].ToString();
            chartSummaryDTO.QACPTCode = hdnQACptCodes;
            chartSummaryDTO.QACPTCodeRemarks = hdnQACptRemarks;

            string currDt = Request.Form["hdnCurrDate"].ToString();
            bool audit = IsAuditRequired("QA", chartSummaryDTO.ProjectID, currDt);
            chartSummaryDTO.IsAuditRequired = audit;

            if (string.IsNullOrEmpty(SubmitAndGetNext))
                clinicalcaseOperations.SubmitQAAvailableChart(chartSummaryDTO, dtAudit);
            else
            {
                clinicalcaseOperations.SubmitQAAvailableChart(chartSummaryDTO, dtAudit);
                return RedirectToAction("GetQAAvailableChart", new { Role = Roles.QA.ToString(), ChartType = "Available", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }
            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("QASummary", lstDto);
        }
        public IActionResult SubmitQARebuttalChartsOfCoder(ChartSummaryDTO chartSummaryDTO, string SubmitAndGetNext)
        {
            var hdnRejected = Request.Form["hdnRejected"].ToString();
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

            //Starting of fetching Dx,CPT in Claim2 to Claim4 
            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ClaimId", typeof(int));

            var hdnClaimId2 = Request.Form["hdnClaimId2"].ToString();
            var hdnClaimId3 = Request.Form["hdnClaimId3"].ToString();
            var hdnClaimId4 = Request.Form["hdnClaimId4"].ToString();

            string hdnClaimData2 = Request.Form["hdnClaimData2"].ToString();
            string hdnClaimData3 = Request.Form["hdnClaimData3"].ToString();
            string hdnClaimData4 = Request.Form["hdnClaimData4"].ToString();

            if (!string.IsNullOrEmpty(hdnClaimData2))
                PrepareAudit(hdnClaimData2, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaimData3))
                PrepareAudit(hdnClaimData3, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaimData4))
                PrepareAudit(hdnClaimData4, dtAudit);

            string hdnDx2 = Request.Form["hdnDx2"].ToString();
            string hdnDxRemarks2 = Request.Form["hdnDxRemarks2"].ToString();
            string hdnDx3 = Request.Form["hdnDx3"].ToString();
            string hdnDxRemarks3 = Request.Form["hdnDxRemarks3"].ToString();
            string hdnDx4 = Request.Form["hdnDx4"].ToString();
            string hdnDxRemarks4 = Request.Form["hdnDxRemarks4"].ToString();

            if (!string.IsNullOrEmpty(hdnDx2))
                dtAudit.Rows.Add("Dx", hdnDx2, hdnDxRemarks2, Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnDx3))
                dtAudit.Rows.Add("Dx", hdnDx3, hdnDxRemarks3, Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnDx4))
                dtAudit.Rows.Add("Dx", hdnDx4, hdnDxRemarks4, Convert.ToInt32(hdnClaimId4));

            string hdnCpt2 = Request.Form["hdnCpt2"].ToString();
            string hdnCptRemarks2 = Request.Form["hdnCptRemarks2"].ToString();
            string hdnCpt3 = Request.Form["hdnCpt3"].ToString();
            string hdnCptRemarks3 = Request.Form["hdnCptRemarks3"].ToString();
            string hdnCpt4 = Request.Form["hdnCpt4"].ToString();
            string hdnCptRemarks4 = Request.Form["hdnCptRemarks4"].ToString();

            if (!string.IsNullOrEmpty(hdnCpt2))
                dtAudit.Rows.Add("CPTCode", hdnCpt2, hdnCptRemarks2, Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnCpt3))
                dtAudit.Rows.Add("CPTCode", hdnCpt3, hdnCptRemarks3, Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnCpt4))
                dtAudit.Rows.Add("CPTCode", hdnCpt4, hdnCptRemarks4, Convert.ToInt32(hdnClaimId4));

            //Ending of fetching Dx,CPT in Claim2 to Claim4 

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            if (string.IsNullOrEmpty(SubmitAndGetNext))
                clinicalcaseOperations.SubmitQARebuttalChartsOfCoder(chartSummaryDTO, dtAudit, hdnRejected);
            else
            {
                clinicalcaseOperations.SubmitQARebuttalChartsOfCoder(chartSummaryDTO, dtAudit, hdnRejected);
                return RedirectToAction("GetQARebuttalChartsOfCoder", new { Role = Roles.QA.ToString(), ChartType = "RebuttalOfCoder", ProjectID = chartSummaryDTO.ProjectID, ProjectName = chartSummaryDTO.ProjectName });
            }

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByRole(Roles.QA.ToString());

            TempData["Success"] = "Chart Details submitted successfully !";
            return View("QASummary", lstDto);
        }
        public IActionResult SubmitQARejectedChartsOfShadowQA(ChartSummaryDTO chartSummaryDTO)
        {
            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ErrorTypeId", typeof(int));
            dtAudit.Columns.Add("ClaimId", typeof(int));

            var hdnQADxCodes = Request.Form["hdnQADxCodes"].ToString();
            var hdnQADxRemarks = Request.Form["hdnQADxRemarks"].ToString();
            var hdnQACptCodes = Request.Form["hdnQACptCodes"].ToString();
            var hdnQACptRemarks = Request.Form["hdnQACptRemarks"].ToString();

            var hdnQADxCodes2 = Request.Form["hdnQADxCodes2"].ToString();
            var hdnQADxRemarks2 = Request.Form["hdnQADxRemarks2"].ToString();
            var hdnQACptCodes2 = Request.Form["hdnQACptCodes2"].ToString();
            var hdnQACptRemarks2 = Request.Form["hdnQACptRemarks2"].ToString();

            var hdnQADxCodes3 = Request.Form["hdnQADxCodes3"].ToString();
            var hdnQADxRemarks3 = Request.Form["hdnQADxRemarks3"].ToString();
            var hdnQACptCodes3 = Request.Form["hdnQACptCodes3"].ToString();
            var hdnQACptRemarks3 = Request.Form["hdnQACptRemarks3"].ToString();

            var hdnQADxCodes4 = Request.Form["hdnQADxCodes4"].ToString();
            var hdnQADxRemarks4 = Request.Form["hdnQADxRemarks4"].ToString();
            var hdnQACptCodes4 = Request.Form["hdnQACptCodes4"].ToString();
            var hdnQACptRemarks4 = Request.Form["hdnQACptRemarks4"].ToString();

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

            // basic Params fro Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnClaim1))
                PrepareAudit1(hdnClaim1, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim2))
                PrepareAudit1(hdnClaim2, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim3))
                PrepareAudit1(hdnClaim3, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim4))
                PrepareAudit1(hdnClaim4, dtAudit);

            // Claim 1 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes) && !string.IsNullOrEmpty(hdnQADxRemarks))
                dtAudit.Rows.Add("Dx", hdnQADxCodes, hdnQADxRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1));

            if (!string.IsNullOrEmpty(hdnQACptCodes) && !string.IsNullOrEmpty(hdnQACptRemarks))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes, hdnQACptRemarks, Convert.ToInt32(hdnQAErrorTypeID1), Convert.ToInt32(hdnClaimId1));

            // Claim 2 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes2) && !string.IsNullOrEmpty(hdnQADxRemarks2))
                dtAudit.Rows.Add("Dx", hdnQADxCodes2, hdnQADxRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnQACptCodes2) && !string.IsNullOrEmpty(hdnQACptRemarks2))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes2, hdnQACptRemarks2, Convert.ToInt32(hdnQAErrorTypeID2), Convert.ToInt32(hdnClaimId2));

            // Claim 3 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes3) && !string.IsNullOrEmpty(hdnQADxRemarks3))
                dtAudit.Rows.Add("Dx", hdnQADxCodes3, hdnQADxRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnQACptCodes3) && !string.IsNullOrEmpty(hdnQACptRemarks3))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes3, hdnQACptRemarks3, Convert.ToInt32(hdnQAErrorTypeID3), Convert.ToInt32(hdnClaimId3));

            // Claim 4 Dx & CPT
            if (!string.IsNullOrEmpty(hdnQADxCodes4) && !string.IsNullOrEmpty(hdnQADxRemarks4))
                dtAudit.Rows.Add("Dx", hdnQADxCodes4, hdnQADxRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4));

            if (!string.IsNullOrEmpty(hdnQACptCodes4) && !string.IsNullOrEmpty(hdnQACptRemarks4))
                dtAudit.Rows.Add("CPTCode", hdnQACptCodes4, hdnQACptRemarks4, Convert.ToInt32(hdnQAErrorTypeID4), Convert.ToInt32(hdnClaimId4));

            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            clinicalcaseOperations.SubmitQARejectedChartsOfShadowQA(chartSummaryDTO, dtAudit);

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
            List<ChartSummaryDTO> lstchartSummary = new List<ChartSummaryDTO>();
            lstchartSummary = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            if (lstchartSummary.Count > 0)
                lstchartSummary.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion

            if (lstchartSummary.Count == 0)
            {
                TempData["Toast"] = "There are no charts available";
                return RedirectToAction("ShadowQASummary");
            }
            return View("ShadowQA", lstchartSummary.OrderBy(a => a.ClaimId).ToList());
        }

        public IActionResult GetShadowQARebuttalChartsOfQA(string Role, string ChartType, int ProjectID, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            List<ChartSummaryDTO> lstChartSummaryDTO = new List<ChartSummaryDTO>();
            lstChartSummaryDTO = clinicalcaseOperations.GetNext1(Role, ChartType, ProjectID);
            lstChartSummaryDTO.FirstOrDefault().ProjectName = ProjectName;

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.ErrorTypes = BindErrorType();
            #endregion
            return View("ShadowQARebuttalChartsOfQA", lstChartSummaryDTO);
        }

        [HttpPost]
        public IActionResult SubmitShadowQAAvailableChart(ChartSummaryDTO chartSummaryDTO, bool hdnIsQAAgreed, string SubmitAndGetNext)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);

            DataTable dtAudit = new DataTable();
            dtAudit.Columns.Add("FieldName", typeof(string));
            dtAudit.Columns.Add("FieldValue", typeof(string));
            dtAudit.Columns.Add("Remark", typeof(string));
            dtAudit.Columns.Add("ErrorTypeId", typeof(int));
            dtAudit.Columns.Add("ClaimId", typeof(int));

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

            // basic Params fro Claim 1 - Claim 2

            if (!string.IsNullOrEmpty(hdnClaim1))
                PrepareAudit1(hdnClaim1, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim2))
                PrepareAudit1(hdnClaim2, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim3))
                PrepareAudit1(hdnClaim3, dtAudit);

            if (!string.IsNullOrEmpty(hdnClaim4))
                PrepareAudit1(hdnClaim4, dtAudit);

            // Claim 1 Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQADxCodes) && !string.IsNullOrEmpty(hdnShadowQADxRemarks))
                dtAudit.Rows.Add("Dx", hdnShadowQADxCodes, hdnShadowQADxRemarks, Convert.ToInt32(hdnShadowQAErrorTypeID1), Convert.ToInt32(hdnClaimId1));

            if (!string.IsNullOrEmpty(hdnShadowQACptCodes) && !string.IsNullOrEmpty(hdnShadowQACptRemarks))
                dtAudit.Rows.Add("CPTCode", hdnShadowQACptCodes, hdnShadowQACptRemarks, Convert.ToInt32(hdnShadowQAErrorTypeID1), Convert.ToInt32(hdnClaimId1));

            // Claim 2 Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQADxCodes2) && !string.IsNullOrEmpty(hdnShadowQADxRemarks2))
                dtAudit.Rows.Add("Dx", hdnShadowQADxCodes2, hdnShadowQADxRemarks2, Convert.ToInt32(hdnShadowQAErrorTypeID2), Convert.ToInt32(hdnClaimId2));

            if (!string.IsNullOrEmpty(hdnShadowQACptCodes2) && !string.IsNullOrEmpty(hdnShadowQACptRemarks2))
                dtAudit.Rows.Add("CPTCode", hdnShadowQACptCodes2, hdnShadowQACptRemarks2, Convert.ToInt32(hdnShadowQAErrorTypeID2), Convert.ToInt32(hdnClaimId2));

            // Claim 3 Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQADxCodes3) && !string.IsNullOrEmpty(hdnShadowQADxRemarks3))
                dtAudit.Rows.Add("Dx", hdnShadowQADxCodes3, hdnShadowQADxRemarks3, Convert.ToInt32(hdnShadowQAErrorTypeID3), Convert.ToInt32(hdnClaimId3));

            if (!string.IsNullOrEmpty(hdnShadowQACptCodes3) && !string.IsNullOrEmpty(hdnShadowQACptRemarks3))
                dtAudit.Rows.Add("CPTCode", hdnShadowQACptCodes3, hdnShadowQACptRemarks3, Convert.ToInt32(hdnShadowQAErrorTypeID3), Convert.ToInt32(hdnClaimId3));

            // Claim 4 Dx & CPT
            if (!string.IsNullOrEmpty(hdnShadowQADxCodes4) && !string.IsNullOrEmpty(hdnShadowQADxRemarks4))
                dtAudit.Rows.Add("Dx", hdnShadowQADxCodes4, hdnShadowQADxRemarks4, Convert.ToInt32(hdnShadowQAErrorTypeID4), Convert.ToInt32(hdnClaimId4));

            if (!string.IsNullOrEmpty(hdnShadowQACptCodes4) && !string.IsNullOrEmpty(hdnShadowQACptRemarks4))
                dtAudit.Rows.Add("CPTCode", hdnShadowQACptCodes4, hdnShadowQACptRemarks4, Convert.ToInt32(hdnShadowQAErrorTypeID4), Convert.ToInt32(hdnClaimId4));


            bool isQAAgreed = hdnIsQAAgreed;// Convert.ToBoolean(Request.Form["hdnIsQAAgreed"]);

            if (string.IsNullOrEmpty(SubmitAndGetNext))
                clinicalcaseOperations.SubmitShadowQAAvailableChart(chartSummaryDTO, isQAAgreed, dtAudit);
            else
            {
                clinicalcaseOperations.SubmitShadowQAAvailableChart(chartSummaryDTO, isQAAgreed, dtAudit);
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
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();

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

        [HttpGet]
        public IActionResult ManageEMCodeLevels ()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var eMCodeLevels = clinicalcaseOperations.GetManageEMCodeLevels();
            ViewBag.eMCodeLevels = eMCodeLevels;
            return View();
        }
        [HttpGet]
        [Route("UAB/EMLevelDetails")]
        [Route("EMLevelDetails/{eMLevel}")]
        public ActionResult EMLevelDetails(int eMLevel)
        {
            if (eMLevel != 0)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                var emleveldetails = clinicalcaseOperations.GetEMCodeLevelDetails(eMLevel); 
                ViewBag.emleveldetails = emleveldetails;
                return View("EMLevelDetails", emleveldetails );
            }
            return RedirectToAction("ManageEMCodeLevels");
        }
        [HttpGet]
        public IActionResult UpdateEMCode (int Id) 
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var emcode  = clinicalcaseOperations.GetEMCodeById(Id);
            return PartialView("_UpdateEMCode", emcode);
        }
        [HttpPost]
        public IActionResult UpdateEMCode (EMCodeLevel model)
        {
            if (model.Id != 0 && !string.IsNullOrWhiteSpace(model.EMCode))
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                clinicalcaseOperations.UpdateEMCode(model);
                TempData["Success"] = "Successfully EM Code Updated";
                return RedirectToAction("EMLevelDetails", new { eMLevel = model.EMLevel });
            }
            else
            {
                TempData["Warning"] = "Unable to  update  EM Code :you havent Changed anything";
                return RedirectToAction("EMLevelDetails", new { eMLevel = model.EMLevel });
            }
        }
        [HttpGet]
        public ActionResult AddEMCode(int eMlevel) 
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            EMCodeLevel model = new EMCodeLevel();
            model.EMLevel = eMlevel;
            return PartialView("_AddEMCode", model);
        }

        [HttpPost]
        public ActionResult AddEMCode (EMCodeLevel model)
        {

            if (model.EMLevel != 0 && !string.IsNullOrEmpty(model.EMCode))
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
                return RedirectToAction("EMLevelDetails", new { eMLevel = model.EMLevel });
            }
            return RedirectToAction("ManageEMCodeLevels");
        }
        [HttpGet]
        public IActionResult DeleteEMCode (int Id )
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var emcode = clinicalcaseOperations.GetEMCodeById(Id);
            return PartialView("_DeleteEMCode", emcode);
        }
        [HttpPost]
        public IActionResult DeleteEMCode (EMCodeLevel model)
        {

            try
            {
                if (model.Id != 0)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                    clinicalcaseOperations.DeletetEMCode(model);
                    TempData["Success"] = "Successfully EM Code  Deleted";
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
            EMCodeLevel eml = new EMCodeLevel
            {
                EMLevel = emlevel
            };
            return PartialView("_DeleteEMLevel", eml);
        }
        [HttpPost]
        public IActionResult DeleteEMLevel (int emlevel,string emcode=null)
        {
            try
            {
                if (emlevel != 0)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                    clinicalcaseOperations.DeletetEMCode(emlevel);
                    TempData["Success"] = "Successfully EM Level  Deleted";
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
        public ActionResult AddEMLevel ()
        {
            return PartialView("_AddEMLevel");
        }
        [HttpPost]
        public ActionResult AddEMLevel(EMCodeLevel model)
        {
            if (model.EMLevel != 0 && !string.IsNullOrEmpty(model.EMCode))
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
                try
                {
                    clinicalcaseOperations.AddEMLevel(model);
                    TempData["Success"] = "Successfully EM Level Added";
                    return RedirectToAction("EMLevelDetails", new { eMLevel = model.EMLevel });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction("ManageEMCodeLevels");
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
