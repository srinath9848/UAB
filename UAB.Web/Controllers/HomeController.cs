using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UAB.DAL;
using UAB.DTO;
using UAB.DAL.Models;
using UAB.enums;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using UAB.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using ClosedXML.Excel;
using FastMember;

namespace UAB.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private int mUserId;
        private string timeZoneCookie;
        private string mUserRole;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(IHttpContextAccessor httpContextAccessor, ILogger<HomeController> logger)
        {
            mUserId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value);
            mUserRole = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            timeZoneCookie = _httpContextAccessor.HttpContext.Request.Cookies["UAB_TimeZoneOffset"];
        }

        public IActionResult Index()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult LevellingSummaryReport()
        {
            _logger.LogInformation("Loading Started for LevellingSummaryReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for LevellingSummaryReport for User: " + mUserId);
            return View();
        }
        public IActionResult ReceivedChartsReport()
        {
            _logger.LogInformation("Loading Started for ReceivedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for ReceivedChartsReport for User: " + mUserId);
            return View();
        }

        public IActionResult ChartSummaryReport()
        {
            _logger.LogInformation("Loading Started for ChartSummaryReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for ChartSummaryReport for User: " + mUserId);
            return View();
        }
        public IActionResult CodedChartsReport()
        {
            _logger.LogInformation("Loading Started for CodedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for CodedChartsReport for User: " + mUserId);
            return View();
        }
        public IActionResult QAChartsReport()
        {
            _logger.LogInformation("Loading Started for QAChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for QAChartsReport for User: " + mUserId);
            return View();
        }
        public IActionResult PostedChartsReport()
        {
            _logger.LogInformation("Loading Started for PostedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for PostedChartsReport for User: " + mUserId);
            return View();
        }
        public IActionResult PendingChartsReport()
        {
            _logger.LogInformation("Loading Started for PendingChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for PendingChartsReport for User: " + mUserId);
            return View();
        }
        public IActionResult ProvidedPostedChartsReport()
        {
            _logger.LogInformation("Loading Started for ProvidedPostedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for ProvidedPostedChartsReport for User: " + mUserId);
            return View();
        }
        public IActionResult BacklogChartsReport()
        {
            _logger.LogInformation("Loading Started for BacklogChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            _logger.LogInformation("Loading Ended for BacklogChartsReport for User: " + mUserId);
            return View();
        }

        [HttpPost]
        public IActionResult GetLevellingReport(int ProjectId, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetLevellingReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstLelvellingReportDTO = clinicalcaseOperations.GetLevellingReport(ProjectId, StartDate, EndDate);
            _logger.LogInformation("Loading Ended for GetLevellingReport for User: " + mUserId);
            return PartialView("_LevellingReport", lstLelvellingReportDTO);
        }

        [HttpGet]
        public IActionResult GetAgingReport()
        {
            _logger.LogInformation("Loading Started for GetAgingReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstagingtDTO = clinicalcaseOperations.GetAgingReport();
            //return ExportToExcel(lstagingtDTO.Tables[0]);
            _logger.LogInformation("Loading Ended for GetAgingReport for User: " + mUserId);
            //return PartialView("_AgingReport", lstagingtDTO);
            return View("AgingReport", lstagingtDTO);
        }

        /// <summary>
        /// Exports the given Data Table as Excel Report. Set the TableName parameter of the DataTable so that the output file will get it as well.
        /// </summary>
        /// <param name="dataTable">Data Table with the rows to export as Excel document</param>
        /// <returns>Excel Document to download</returns>
        [HttpGet]
        public IActionResult ExportToExcel(DataTable dataTable)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Export_{dataTable.TableName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx");
                }
            }
        }

        public IActionResult ExportAgingReportByProject(string dataTableName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstagingtDTO = clinicalcaseOperations.GetAgingReport();
            ExportToExcel(lstagingtDTO.Tables[0]);
            return View("AgingReport", lstagingtDTO);
        }

        public IActionResult ExportAgingReportByStatus(string dataTableName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstagingtDTO = clinicalcaseOperations.GetAgingReport();
            ExportToExcel(lstagingtDTO.Tables[1]);
            return View("AgingReport", lstagingtDTO);
        }

        [HttpGet]
        public IActionResult GetAgingReportDetails(string ColumnName, string ProjectType, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetAgingReportDetails for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int ProjectId = clinicalcaseOperations.GetProjects().Where(x => x.Name == ProjectName).Select(x => x.ProjectId).FirstOrDefault();
            var lstAgingReportDetails = clinicalcaseOperations.GetAgingReportDetails(ColumnName, ProjectType, ProjectId);
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.ColumnName = ColumnName;
            ViewBag.projectname = ProjectName;
            ViewBag.projectType = projectType;
            _logger.LogInformation("Loading Ended for GetAgingReportDetails for User: " + mUserId);
            return PartialView("_AgingBreakDownReportDetailsByDay", lstAgingReportDetails);
        }
        [HttpGet]
        public IActionResult ExportAgingReportDetailByProject(string ColumnName, string ProjectType, string ProjectName)
        {
            _logger.LogInformation("Loading Started for ExportAgingReportDetailByProject for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int ProjectId = clinicalcaseOperations.GetProjects().Where(x => x.Name == ProjectName).Select(x => x.ProjectId).FirstOrDefault();
            var lstAgingReportDetails = clinicalcaseOperations.GetAgingReportDetails(ColumnName, ProjectType, ProjectId);
            List<CodingDTO> agingList = new List<CodingDTO>();
            foreach (var agingData in lstAgingReportDetails)
            {
                CodingDTO data = new CodingDTO();
                data = agingData.CodingDTO;
                agingList.Add(data);
            }
            //var listAgingData
            //foreach(var agingData in lstAgingReportDetails)
            //DataTable table = new DataTable();
            //using (var reader = ObjectReader.Create(agingList))
            //{
            //    table.Load(reader);
            //}
            var table=clinicalcaseOperations.ToDataTable<CodingDTO>(agingList);

            ExportToExcel(table);
            _logger.LogInformation("Loading Ended for ExportAgingReportDetailByProject for User: " + mUserId);
            return PartialView("_AgingBreakDownReportDetailsByDay", lstAgingReportDetails);
        }

        [HttpGet]
        public IActionResult GetAgingReportDetailsByStatus(string ColumnName, string ProjectType, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetAgingReportDetailsByStatus for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int ProjectId = clinicalcaseOperations.GetProjects().Where(x => x.Name == ProjectName).Select(x => x.ProjectId).FirstOrDefault();
            var lstAgingReportDetails = clinicalcaseOperations.GetAgingReportDetailsByStatus(ColumnName, ProjectType, ProjectId);
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.ColumnName = ColumnName;
            ViewBag.projectname = ProjectName;
            ViewBag.projectType = projectType;
            _logger.LogInformation("Loading Ended for GetAgingReportDetailsByStatus for User: " + mUserId);
            return PartialView("_AgingBreakDownReportDetailsByDay", lstAgingReportDetails);
        }

        [HttpGet]
        public IActionResult GetAgingReportDetailsForBlockedCharts(string ColumnName, string ProjectName)
        {
            _logger.LogInformation("Loading Started for GetAgingReportDetailsForBlockedCharts for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int ProjectId = clinicalcaseOperations.GetProjects().Where(x => x.Name == ProjectName).Select(x => x.ProjectId).FirstOrDefault();
            var lstAgingReportDetails = clinicalcaseOperations.GetAgingReportDetailsForBlockedCharts(ColumnName, ProjectId, timeZoneCookie);

            ViewBag.ColumnName = ColumnName;
            ViewBag.projectname = ProjectName;
            _logger.LogInformation("Loading Ended for GetAgingReportDetailsForBlockedCharts for User: " + mUserId);
            return PartialView("_AgingReportDetails", lstAgingReportDetails);
        }

        [HttpPost]
        public IActionResult SaveOrUnblocktheChart(int cid, string ManagerResponse, string flag)
        {
            _logger.LogInformation("Loading Started for SaveOrUnblocktheChart for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            clinicalcaseOperations.SaveOrUnblockchart(cid, ManagerResponse, flag);
            _logger.LogInformation("Loading Ended for SaveOrUnblocktheChart for User: " + mUserId);
            return RedirectToAction("GetAgingReport", "Home");
        }
        [HttpPost]
        public IActionResult GetReceivedChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetReceivedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetReceivedChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetReceivedChartsReport for User: " + mUserId);
            return PartialView("_ReceivedChartReport", lstReceivedChartReport);
        }
        public IActionResult ExportReceivedChartReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetReceivedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetReceivedChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ExportToExcel(lstReceivedChartReport.Tables[0]);
            _logger.LogInformation("Loading Ended for GetReceivedChartsReport for User: " + mUserId);
            return PartialView("_ReceivedChartReport", lstReceivedChartReport);
        }

        [HttpGet]
        public IActionResult GetReceivedReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range)
        {
            _logger.LogInformation("Loading Started for GetReceivedReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetReceivedChartReportDetails(date, week, month, year, ProjectId, range);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.projectname = projectname;
            ViewBag.projectType = projectType;
            ViewBag.date = date;
            ViewBag.week = week;
            ViewBag.month = month;
            ViewBag.year = year;
            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.ChartName = "Received Chart Details";
            _logger.LogInformation("Loading Ended for GetReceivedReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstPendingReportDetails);
        }

        public IActionResult ExportDetailedReport(DateTime date, int week, string month, string year, int ProjectId, string range)
        {
            _logger.LogInformation("Loading Started for GetReceivedReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetReceivedChartReportDetails(date, week, month, year, ProjectId, range);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.projectname = projectname;
            ViewBag.projectType = projectType;
            ViewBag.date = date;
            ViewBag.week = week;
            ViewBag.month = month;
            ViewBag.year = year;
            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.ChartName = "Received Chart Details";
            _logger.LogInformation("Loading Ended for GetReceivedReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstPendingReportDetails);
        }

        [HttpPost]
        public IActionResult GetChartSummaryReport(int ProjectId, DateTime StartDate, DateTime EndDate,string buttonType)
        {
            _logger.LogInformation("Loading Started for GetChartSummaryReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetChartSummaryReport(ProjectId, StartDate, EndDate);
            ViewBag.ProjectId = ProjectId;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            if (buttonType == "btnExport")
            {
                ExportToExcel(lstReceivedChartReport.Tables[0]);
            }
            _logger.LogInformation("Loading Ended for GetChartSummaryReport for User: " + mUserId);
            return PartialView("_ChartSummaryReport", lstReceivedChartReport);
        }
        public IActionResult ExportChartSummaryReport(int ProjectId, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetChartSummaryReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetChartSummaryReport(ProjectId, StartDate, EndDate);
            ViewBag.ProjectId = ProjectId;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            ExportToExcel(lstReceivedChartReport.Tables[0]);
            _logger.LogInformation("Loading Ended for GetChartSummaryReport for User: " + mUserId);
            return PartialView("_ChartSummaryReport", lstReceivedChartReport);
        }

        [HttpGet]
        public IActionResult GetChartSummaryReportDetails(string ColumnName, DateTime dos, int ProjectId)
        {
            _logger.LogInformation("Loading Started for GetChartSummaryReportDetails for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartSummaryReportDetails = clinicalcaseOperations.GetChartSummaryReportDetails(ProjectId, dos, ColumnName);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.ColumnName = ColumnName;
            ViewBag.dos = dos.ToString("MM/dd/yyyy");
            ViewBag.projectname = projectname;
            ViewBag.projectType = projectType;
            _logger.LogInformation("Loading Ended for GetChartSummaryReportDetails for User: " + mUserId);
            return PartialView("_ChartSummaryReportDetails", lstChartSummaryReportDetails);
        }

        public IActionResult GetPostedChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetPostedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetPostedChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetPostedChartsReport for User: " + mUserId);
            return PartialView("_PostedChartReport", lstReceivedChartReport);
        }
        public IActionResult ExportPostedChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetPostedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPostedChartReport = clinicalcaseOperations.GetPostedChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            ExportToExcel(lstPostedChartReport.Tables[0]);
            _logger.LogInformation("Loading Ended for GetPostedChartsReport for User: " + mUserId);
            return PartialView("_PostedChartReport", lstPostedChartReport);
        }

        [HttpGet]
        public IActionResult GetPostedReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range)
        {
            _logger.LogInformation("Loading Started for GetPostedReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetPostedChartReportDetails(date, week, month, year, ProjectId, range);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.projectname = projectname;
            ViewBag.projectType = projectType;
            ViewBag.ChartName = "Posted Chart Details";
            _logger.LogInformation("Loading Ended for GetPostedReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstPendingReportDetails);
        }

        public IActionResult GetBackLogChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetBackLogChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstBacklogChartReport = clinicalcaseOperations.GetBacklogChartsReport(ProjectId, range, StartDate, EndDate);
            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetBackLogChartsReport for User: " + mUserId);
            return PartialView("_BacklogChartReport", lstBacklogChartReport);
        }
        public IActionResult ExportBackLogChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetBackLogChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstBacklogChartReport = clinicalcaseOperations.GetBacklogChartsReport(ProjectId, range, StartDate, EndDate);
            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            ExportToExcel(lstBacklogChartReport.Tables[0]);
            _logger.LogInformation("Loading Ended for GetBackLogChartsReport for User: " + mUserId);
            return PartialView("_BacklogChartReport", lstBacklogChartReport);
        }

        [HttpGet]
        public IActionResult GetBackLogChartsReportDetails(int delaydays, string status, int projectid)
        {
            _logger.LogInformation("Loading Started for GetBackLogChartsReportDetails for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int statusid = clinicalcaseOperations.GetStatusList().Where(x => x.Name == status).Select(x => x.StatusId).FirstOrDefault();
            var lstBackLogChartReportDetails = clinicalcaseOperations.GetBacklogChartsReportDetails(delaydays, statusid, projectid);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == projectid).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == projectid).Select(x => x.ProjectTypeName).FirstOrDefault();
            ViewBag.delaydays = delaydays;
            ViewBag.projectname = projectname;
            ViewBag.status = status;
            ViewBag.projectType = projectType;
            _logger.LogInformation("Loading Ended for GetBackLogChartsReportDetails for User: " + mUserId);
            return PartialView("_BackLogChartsReportDetails", lstBackLogChartReportDetails);
        }

        public IActionResult GetCodedChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetCodedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstCodedChartReport = clinicalcaseOperations.GetCodedChartsReport(ProjectId, range, StartDate.ToUtcDate(timeZoneCookie), EndDate.AddHours(23).AddMinutes(59).AddSeconds(59).ToUtcDate(timeZoneCookie));

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            _logger.LogInformation("Loading Ended for GetCodedChartsReport for User: " + mUserId);
            return PartialView("_CodedChartReport", lstCodedChartReport);
        }
        [HttpGet]
        public IActionResult GetCodedReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range)
        {
            _logger.LogInformation("Loading Started for GetCodedReportDetails for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetCodedChartReportDetails(date.ToUtcDate(timeZoneCookie), week, month, year, ProjectId, range);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.projectname = projectname;
            ViewBag.projectType = projectType;
            ViewBag.ChartName = "Coded Chart Details";
            _logger.LogInformation("Loading Ended for GetCodedReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstPendingReportDetails);
        }

        public IActionResult GetQAChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetQAChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstqaChartReport = clinicalcaseOperations.GetQAChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetQAChartsReport for User: " + mUserId);
            return PartialView("_QAChartReport", lstqaChartReport);
        }

        public IActionResult ExportQAChartReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetQAChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstqaChartReport = clinicalcaseOperations.GetQAChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            ExportToExcel(lstqaChartReport.Tables[0]);
            _logger.LogInformation("Loading Ended for GetQAChartsReport for User: " + mUserId);
            return PartialView("_QAChartReport", lstqaChartReport);
        }
        [HttpGet]
        public IActionResult GetQAReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range)
        {
            _logger.LogInformation("Loading Started for GetQAReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstQAReportDetails = clinicalcaseOperations.GetQAChartReportDetails(date, week, month, year, ProjectId, range);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.projectname = projectname;
            ViewBag.projectType = projectType;
            ViewBag.ChartName = "QA Chart Details";
            _logger.LogInformation("Loading Ended for GetQAReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstQAReportDetails);
        }

        public IActionResult GetPendingChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetPendingChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingChartReport = clinicalcaseOperations.GetPendingChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetPendingChartsReport for User: " + mUserId);
            return PartialView("_PendingChartsReport", lstPendingChartReport);
        }
        public IActionResult ExportPendingChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetPendingChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingChartReport = clinicalcaseOperations.GetPendingChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            ExportToExcel(lstPendingChartReport.Tables[0]);
            _logger.LogInformation("Loading Ended for GetPendingChartsReport for User: " + mUserId);
            return PartialView("_PendingChartsReport", lstPendingChartReport);
        }

        [HttpGet]
        public IActionResult GetPendingReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range)
        {
            _logger.LogInformation("Loading Started for GetPendingReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetPendingReportDetails(date, week, month, year, ProjectId, range);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType=clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.projectname = projectname;
            ViewBag.projectType = projectType;
            _logger.LogInformation("Loading Ended for GetPendingReportDetails for User: " + mUserId);
            return PartialView("_PendingReportDetails", lstPendingReportDetails);
        }

        public IActionResult GetProvidedpostedchartsChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetProvidedpostedchartsChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartReport = clinicalcaseOperations.GetProvidedpostedchartsChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetProvidedpostedchartsChartsReport for User: " + mUserId);
            return PartialView("_ProvidedpostedchartsChartReport", lstChartReport);
        }

        public IActionResult ExportProvidedpostedchartsChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetProvidedpostedchartsChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartReport = clinicalcaseOperations.GetProvidedpostedchartsChartsReport(ProjectId, range, StartDate, EndDate);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            ExportToExcel(lstChartReport.Tables[0]);
            _logger.LogInformation("Loading Ended for GetProvidedpostedchartsChartsReport for User: " + mUserId);
            return PartialView("_ProvidedpostedchartsChartReport", lstChartReport);
        }

        [HttpGet]
        public IActionResult GetProviderPostedReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range)
        {
            _logger.LogInformation("Loading Started for GetProviderPostedReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetProviderPostedChartReportDetails(date, week, month, year, ProjectId, range);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.projectname = projectname;
            ViewBag.projectType = projectType;
            ViewBag.ChartName = "Provider Posted Chart Details";
            _logger.LogInformation("Loading Ended for GetProviderPostedReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstPendingReportDetails);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            //var path = HttpContext
            //  .Features
            //  .Get<IExceptionHandlerPathFeature>();

            //if (path == null)
            //    return View();

            //// Use the information about the exception 
            //var exception = path.Error;
            //var pathString = path.Path;

            return View();// new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AuthenticationError()
        {
            return View();
        }
    }
}
