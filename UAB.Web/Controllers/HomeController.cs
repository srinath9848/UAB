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

        [HttpPost]
        public IActionResult GetLevellingReport(int ProjectId, DateTime StartDate, DateTime EndDate)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstLelvellingReportDTO = clinicalcaseOperations.GetLevellingReport(ProjectId, StartDate, EndDate);
            return PartialView("_LevellingReport", lstLelvellingReportDTO);
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
            return PartialView("_ChartSummaryReport", lstReceivedChartReport);
        }

        public IActionResult GetPostedChartsReport(int ProjectId, string range)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetPostedChartsReport(ProjectId, range);
            return PartialView("_PostedChartReport", lstReceivedChartReport);
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
