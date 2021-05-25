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

namespace UAB.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private int mUserId;
        private string mUserRole;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(IHttpContextAccessor httpContextAccessor, ILogger<HomeController> logger)
        {
            mUserId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value);
            mUserRole = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult LevellingSummaryReport ()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult ReceivedChartsReport()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult ChartSummaryReport ()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult CodedChartsReport ()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult QAChartsReport ()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult PostedChartsReport()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult PendingChartsReport ()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult ProvidedPostedChartsReport()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }
        public IActionResult BacklogChartsReport()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            ViewBag.Projects = clinicalcaseOperations.GetProjects();
            return View();
        }

        [HttpPost]
        public IActionResult GetLevellingReport(int ProjectId, DateTime StartDate, DateTime EndDate)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstLelvellingReportDTO = clinicalcaseOperations.GetLevellingReport(ProjectId, StartDate, EndDate);
            return PartialView("_LevellingReport", lstLelvellingReportDTO);
        }

        [HttpGet]
        public IActionResult GetAgingReport ()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstagingtDTO = clinicalcaseOperations.GetAgingReport();

            //return PartialView("_AgingReport", lstagingtDTO);
            return View("AgingReport", lstagingtDTO);
        }

        [HttpGet]
        public IActionResult GetAgingReportDetails(string ColumnName, string ProjectType, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int ProjectId = clinicalcaseOperations.GetProjects().Where(x => x.Name == ProjectName).Select(x => x.ProjectId).FirstOrDefault();
            var lstAgingReportDetails = clinicalcaseOperations.GetAgingReportDetails(ColumnName,ProjectType, ProjectId);

            ViewBag.ColumnName = ColumnName;
            ViewBag.projectname = ProjectName;
            return PartialView("_AgingBreakDownReportDetailsByDay", lstAgingReportDetails);
        }
        [HttpGet]
        public IActionResult GetAgingReportDetailsByStatus(string ColumnName, string ProjectType, string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int ProjectId = clinicalcaseOperations.GetProjects().Where(x => x.Name == ProjectName).Select(x => x.ProjectId).FirstOrDefault();
            var lstAgingReportDetails = clinicalcaseOperations.GetAgingReportDetailsByStatus(ColumnName, ProjectType, ProjectId);

            ViewBag.ColumnName = ColumnName;
            ViewBag.projectname = ProjectName;
            return PartialView("_AgingBreakDownReportDetailsByDay", lstAgingReportDetails);
        }
        
        [HttpGet]
        public IActionResult GetAgingReportDetailsForBlockedCharts(string ColumnName,string ProjectName)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int ProjectId = clinicalcaseOperations.GetProjects().Where(x => x.Name == ProjectName).Select(x => x.ProjectId).FirstOrDefault();
            var lstAgingReportDetails = clinicalcaseOperations.GetAgingReportDetailsForBlockedCharts(ColumnName,ProjectId);

            ViewBag.ColumnName = ColumnName;
            ViewBag.projectname = ProjectName;
            return PartialView("_AgingReportDetails", lstAgingReportDetails);
        }

        [HttpPost]
        public IActionResult SaveOrUnblocktheChart(int cid, string ManagerResponse,string flag)
        {
            ClinicalcaseOperations clinicalcaseOperations  = new ClinicalcaseOperations(mUserId);
            clinicalcaseOperations.SaveOrUnblockchart(cid, ManagerResponse,flag);
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult GetReceivedChartsReport(int ProjectId, string range)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetReceivedChartsReport(ProjectId, range);
            return PartialView("_ReceivedChartReport", lstReceivedChartReport);
        }
        [HttpPost]
        public IActionResult GetChartSummaryReport(int ProjectId, DateTime StartDate, DateTime EndDate)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetChartSummaryReport(ProjectId, StartDate, EndDate);
            ViewBag.ProjectId = ProjectId;
            return PartialView("_ChartSummaryReport", lstReceivedChartReport);
        }
        [HttpGet]
        public IActionResult GetChartSummaryReportDetails (string ColumnName,DateTime dos, int ProjectId)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartSummaryReportDetails = clinicalcaseOperations.GetChartSummaryReportDetails(ProjectId, dos, ColumnName);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();

            ViewBag.ColumnName = ColumnName;
            ViewBag.dos = dos.ToString("MM/dd/yyyy");
            ViewBag.projectname = projectname;
            return PartialView("_ChartSummaryReportDetails", lstChartSummaryReportDetails);
        }

        public IActionResult GetPostedChartsReport(int ProjectId, string range)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetPostedChartsReport(ProjectId, range);
            return PartialView("_PostedChartReport", lstReceivedChartReport);
        }
        public IActionResult GetBackLogChartsReport(int ProjectId, string range)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstBacklogChartReport = clinicalcaseOperations.GetBacklogChartsReport (ProjectId, range);
            ViewBag.ProjectId = ProjectId;
            return PartialView("_BacklogChartReport",lstBacklogChartReport);
        }
        [HttpGet] 
        public IActionResult GetBackLogChartsReportDetails(int delaydays, string status, int projectid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int statusid = clinicalcaseOperations.GetStatusList().Where(x => x.Name == status).Select(x => x.StatusId).FirstOrDefault();
           var lstBackLogChartReportDetails = clinicalcaseOperations.GetBacklogChartsReportDetails(delaydays, statusid, projectid);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == projectid).Select(x => x.Name).FirstOrDefault();
            ViewBag.delaydays = delaydays;
            ViewBag.projectname = projectname;
            ViewBag.status = status;

            return PartialView("_BackLogChartsReportDetails",lstBackLogChartReportDetails);
        }

        public IActionResult GetCodedChartsReport(int ProjectId, string range)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetCodedChartsReport(ProjectId, range);
            return PartialView("_CodedChartReport", lstReceivedChartReport);
        }
        public IActionResult GetQAChartsReport (int ProjectId, string range)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstqaChartReport = clinicalcaseOperations.GetQAChartsReport(ProjectId, range);
            return PartialView("_QAChartReport", lstqaChartReport);
        }
        public IActionResult GetPendingChartsReport(int ProjectId, string range)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingChartReport = clinicalcaseOperations.GetPendingChartsReport(ProjectId, range);

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            return PartialView("_PendingChartsReport", lstPendingChartReport);
        }
        [HttpGet]
        public IActionResult GetPendingReportDetails(DateTime date,int week,string month,string year, int ProjectId,string range)
        {
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetPendingReportDetails(date,week,month,year,ProjectId,range);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();

            ViewBag.projectname = projectname;
            return PartialView("_PendingReportDetails", lstPendingReportDetails);
        }

        public IActionResult GetProvidedpostedchartsChartsReport(int ProjectId, string range)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartReport = clinicalcaseOperations.GetProvidedpostedchartsChartsReport(ProjectId, range);
            return PartialView("_ProvidedpostedchartsChartReport", lstChartReport);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AuthenticationError()
        {
            return View();
        }
    }
}
