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
using System.Globalization;

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
        public IActionResult GetLevellingReport(int ProjectId, DateTime StartDate, DateTime EndDate, string dateType)
        {
            _logger.LogInformation("Loading Started for GetLevellingReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstLelvellingReportDTO = clinicalcaseOperations.GetLevellingReport(ProjectId, StartDate, EndDate, dateType);

            ViewBag.ProjectId = ProjectId;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.DateType = dateType;
            _logger.LogInformation("Loading Ended for GetLevellingReport for User: " + mUserId);
            return PartialView("_LevellingReport", lstLelvellingReportDTO);
        }

        public IActionResult ExportLevellingReport(int ProjectId, DateTime StartDate, DateTime EndDate, string dateType)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstLelvellingReportDTO = clinicalcaseOperations.GetLevellingReport(ProjectId, StartDate, EndDate, dateType);
            int count = lstLelvellingReportDTO.Tables.Count;
            for(int i = 0; i < lstLelvellingReportDTO.Tables.Count; i++)
            {
                for(int x = 0; x < lstLelvellingReportDTO.Tables[i].Rows.Count; x++)
                {
                    for (int y = 1; y < lstLelvellingReportDTO.Tables[i].Columns.Count; y++)
                    {
                        if (string.IsNullOrEmpty(lstLelvellingReportDTO.Tables[i].Rows[x][y].ToString()))
                        {
                            lstLelvellingReportDTO.Tables[i].Rows[x][y] = "0";
                        }
                    }
                }
            }

            return ExportToExcelForLevellingReport(lstLelvellingReportDTO);
        }

        [HttpGet]
        public IActionResult GetAgingReport()
        {
            _logger.LogInformation("Loading Started for GetAgingReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstagingtDTO = clinicalcaseOperations.GetAgingReport();
            _logger.LogInformation("Loading Ended for GetAgingReport for User: " + mUserId);
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

            _logger.LogInformation("Loading Started for ExportToExcel for User: " + mUserId);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    _logger.LogInformation("Loading Ended for ExportToExcel for User: " + mUserId);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Export_{dataTable.TableName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx");
                }
            }
        }

        [HttpGet]
        public IActionResult ExportToExcelForLevellingReport(DataSet dataSet)
        {
            _logger.LogInformation("Loading Started for ExportToExcelForLevellingReport for User: " + mUserId);
            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet ws = wb.Worksheets.Add("Levelling Report");
                int colCount = dataSet.Tables[0].Columns.Count;
                var firstRow = ws.Row(1);
                firstRow.Cell(1).Value = "Levelling Report";
                firstRow.Cell(1).Style.Font.SetBold();
                firstRow.Cell(1).Style.Font.SetFontColor(XLColor.DarkRed);

                var secondRow = ws.Row(2);
                //secondRow.Style.DateFormat.Format = "mm/dd";
                    //ws.Style.DateFormat.Format = "mm/dd";
                for (int i = 0; i < colCount; i++)
                {
                    secondRow.Cell(i + 1).Value = "'"+dataSet.Tables[0].Columns[i].ToString();
                }
                secondRow.Cell(colCount + 1).Value = "Total";
                var thirdRow = ws.Row(3);
                thirdRow.Cell(1).Value = dataSet.Tables[0];

                int lastRow = ws.LastRowUsed().RowNumber();
                int lastColumn = ws.LastColumnUsed().CellCount();

                decimal total;
                int col = 0;
                for (int x = 3; x <= lastRow; x++)
                {
                    total = 0;
                    for (int y = 3; y <= colCount; y++)
                    {
                        total = total + Convert.ToDecimal(ws.Row(x).Cell(y).Value);
                        col = y;
                    }
                    ws.Row(x).Cell(col + 1).Value = total.ToString();
                }

                ws.Row(lastRow + 1).Cell(2).Value = "Total";

                for (int x = 3; x <= colCount + 1; x++)
                {
                    total = 0;
                    for (int y = 3; y <= lastRow; y++)
                    {
                        total = total + Convert.ToDecimal(ws.Row(y).Cell(x).Value);
                        col = y;
                    }
                    ws.Row(col + 1).Cell(x).Value = total.ToString();
                }

                var firstRowForPercentage = ws.Row(lastRow + 3);
                firstRowForPercentage.Cell(1).Value = "Levelling Report Percentage";
                firstRowForPercentage.Cell(1).Style.Font.SetBold();
                firstRowForPercentage.Cell(1).Style.Font.SetFontColor(XLColor.DarkRed);

                var secondRowForPercentage = ws.Row(lastRow + 4);
                int colCountForPercentage = dataSet.Tables[1].Columns.Count;
                secondRowForPercentage.Style.DateFormat.Format = "mm/dd";
                for (int i = 0; i < colCountForPercentage; i++)
                {
                    secondRowForPercentage.Cell(i + 1).Value = "'"+dataSet.Tables[1].Columns[i].ToString();
                }
                secondRowForPercentage.Cell(colCountForPercentage + 1).Value = "Average";
                var thirdRowForPercentage = ws.Row(lastRow + 5);
                thirdRowForPercentage.Cell(1).Value = dataSet.Tables[1];

                NumberFormatInfo setPrecision = new NumberFormatInfo();
                setPrecision.NumberDecimalDigits = 2;

                int lastRowForPercentage = ws.LastRowUsed().RowNumber();
                decimal avg;
                for (int x = lastRow + 5; x <= lastRowForPercentage; x++)
                {
                    total = 0;
                    for(int y = 3; y <= colCountForPercentage; y++)
                    {
                        total = total + Convert.ToDecimal(ws.Row(x).Cell(y).Value);
                        col = y;
                    }
                    avg = total / (colCountForPercentage - 2);
                    ws.Row(x).Cell(col + 1).Value = avg.ToString("N", setPrecision);
                }

                for (int x = lastRow + 5; x <= lastRowForPercentage; x++)
                {
                    total = 0;
                    for (int y = 3; y <= colCountForPercentage+1; y++)
                    {
                        ws.Row(x).Cell(y).Value= ws.Row(x).Cell(y).Value+"%";
                        col = y;
                    }
                }

                //ws.Style.DateFormat.Format = "mm/dd";

                var range = ws.Range(2, 1, lastRow + 1, colCount + 1);

                // create the actual table
                var table = range.CreateTable();

                // apply style
                table.Theme = XLTableTheme.TableStyleLight9;

                var range1 = ws.Range(lastRow + 4, 1, lastRowForPercentage + 1, colCountForPercentage + 1);

                // create the actual table
                var table1 = range1.CreateTable();

                // apply style
                table1.Theme = XLTableTheme.TableStyleLight9;

                string fileName = $"Export_LevellingReport_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx";
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    _logger.LogInformation("Loading Ended for ExportToExcelForLevellingReport for User: " + mUserId);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public IActionResult ExportAgingReportByProject()
        {
            _logger.LogInformation("Loading Started for ExportAgingReportByProject for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstagingtDTO = clinicalcaseOperations.GetAgingReport();
            _logger.LogInformation("Loading Ended for ExportAgingReportByProject for User: " + mUserId);
            return ExportToExcel(lstagingtDTO.Tables[0]);
        }

        public IActionResult ExportAgingReportByStatus()
        {
            _logger.LogInformation("Loading Started for ExportAgingReportByStatus for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstagingtDTO = clinicalcaseOperations.GetAgingReport();
            _logger.LogInformation("Loading Ended for ExportAgingReportByStatus for User: " + mUserId);
            return ExportToExcel(lstagingtDTO.Tables[1]);
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
            var table = clinicalcaseOperations.ToDataTable<CodingDTO>(agingList);
            table.Columns.Remove("ClinicalCaseID");
            if (ProjectType != "IP")
            {
                table.Columns.Remove("ListName");
            }
            _logger.LogInformation("Loading Ended for ExportAgingReportDetailByProject for User: " + mUserId);
            return ExportToExcel(table);
        }

        [HttpGet]
        public IActionResult ExportAgingReportDetailByStatus(string ColumnName, string ProjectType, string ProjectName)
        {
            _logger.LogInformation("Loading Started for ExportAgingReportDetailByProject for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int ProjectId = clinicalcaseOperations.GetProjects().Where(x => x.Name == ProjectName).Select(x => x.ProjectId).FirstOrDefault();
            var lstAgingReportDetails = clinicalcaseOperations.GetAgingReportDetailsByStatus(ColumnName, ProjectType, ProjectId);
            List<CodingDTO> agingList = new List<CodingDTO>();
            foreach (var agingData in lstAgingReportDetails)
            {
                CodingDTO data = new CodingDTO();
                data = agingData.CodingDTO;
                agingList.Add(data);
            }
            var table = clinicalcaseOperations.ToDataTable<CodingDTO>(agingList);
            table.Columns.Remove("ClinicalCaseID");
            if (ProjectType != "IP")
            {
                table.Columns.Remove("ListName");
            }
            _logger.LogInformation("Loading Ended for ExportAgingReportDetailByProject for User: " + mUserId);
            return ExportToExcel(table);
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
            return PartialView("_AgingBreakDownReportDetailsByStatus", lstAgingReportDetails);
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
            var lstReceivedChartReport = clinicalcaseOperations.GetReceivedChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetReceivedChartsReport for User: " + mUserId);
            return PartialView("_ReceivedChartReport", lstReceivedChartReport);
        }
        public IActionResult ExportReceivedChartReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for ExportReceivedChartReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedChartReport = clinicalcaseOperations.GetReceivedChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));
            lstReceivedChartReport.Tables[0].TableName = "ReceivedChartReport";
            clinicalcaseOperations.RemoveStartAndEndDate(lstReceivedChartReport, range);
            _logger.LogInformation("Loading Ended for ExportReceivedChartReport for User: " + mUserId);
            return ExportToExcel(lstReceivedChartReport.Tables[0]);
        }

        [HttpGet]
        public IActionResult GetReceivedReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range, DateTime StartDate, DateTime EndDate,DateTime weekStartDate, DateTime weekEndDate)
        {
            _logger.LogInformation("Loading Started for GetReceivedReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstReceivedReportDetails = clinicalcaseOperations.GetReceivedChartReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);
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
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.WeekStartDate = weekStartDate;
            ViewBag.WeekEndDate = weekEndDate;
            ViewBag.ChartName = "Received Chart Details";
            _logger.LogInformation("Loading Ended for GetReceivedReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstReceivedReportDetails);
        }

        public IActionResult ExportDetailedReport(DateTime date, int week, string month, string year, int ProjectId, string range, string ChartName, DateTime StartDate, DateTime EndDate,string ProjectType,DateTime weekStartDate,DateTime weekEndDate)
        {
            _logger.LogInformation("Loading Started for ExportDetailedReport for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            List<CodingDTO> codingList = new List<CodingDTO>();
            if (ChartName == "Received Chart Details")
            {
                var lstReceivedReportDetails = clinicalcaseOperations.GetReceivedChartReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);

                foreach (var receivedData in lstReceivedReportDetails)
                {
                    CodingDTO data = new CodingDTO();
                    data = receivedData.CodingDTO;
                    codingList.Add(data);
                }

                var table = clinicalcaseOperations.ToDataTable<CodingDTO>(codingList);
                table.TableName = "ReceivedChartDetailedReport";
                table.Columns.Remove("ClinicalCaseID");
                if (ProjectType != "IP")
                {
                    table.Columns.Remove("ListName");
                }
                _logger.LogInformation("Loading Ended for ExportDetailedReport for User: " + mUserId);
                return ExportToExcel(table);
            }
            else if (ChartName == "Posted Chart Details")
            {
                var lstPostedReportDetails = clinicalcaseOperations.GetPostedChartReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);

                foreach (var postedData in lstPostedReportDetails)
                {
                    CodingDTO data = new CodingDTO();
                    data = postedData.CodingDTO;
                    codingList.Add(data);
                }

                var table = clinicalcaseOperations.ToDataTable<CodingDTO>(codingList);
                table.TableName = "PostedChartDetails";
                table.Columns.Remove("ClinicalCaseID");
                if (ProjectType != "IP")
                {
                    table.Columns.Remove("ListName");
                }
                _logger.LogInformation("Loading Ended for ExportDetailedReport for User: " + mUserId);
                return ExportToExcel(table);
            }
            else if (ChartName == "Coded Chart Details")
            {
                var lstCodedReportDetails = clinicalcaseOperations.GetCodedChartReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);

                foreach (var codedData in lstCodedReportDetails)
                {
                    CodingDTO data = new CodingDTO();
                    data = codedData.CodingDTO;
                    codingList.Add(data);
                }

                var table = clinicalcaseOperations.ToDataTable<CodingDTO>(codingList);
                table.TableName = "CodedChartDetails";
                table.Columns.Remove("ClinicalCaseID");
                if (ProjectType != "IP")
                {
                    table.Columns.Remove("ListName");
                }
                _logger.LogInformation("Loading Ended for ExportDetailedReport for User: " + mUserId);
                return ExportToExcel(table);
            }
            else if (ChartName == "QA Chart Details")
            {
                var lstQAReportDetails = clinicalcaseOperations.GetQAChartReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);

                foreach (var qaData in lstQAReportDetails)
                {
                    CodingDTO data = new CodingDTO();
                    data = qaData.CodingDTO;
                    codingList.Add(data);
                }

                var table = clinicalcaseOperations.ToDataTable<CodingDTO>(codingList);
                table.TableName = "QAChartDetails";
                table.Columns.Remove("ClinicalCaseID");
                if (ProjectType != "IP")
                {
                    table.Columns.Remove("ListName");
                }
                _logger.LogInformation("Loading Ended for ExportDetailedReport for User: " + mUserId);
                return ExportToExcel(table);
            }
            else if (ChartName == "Provider Posted Chart Details")
            {
                var lstProviderPostedReportDetails = clinicalcaseOperations.GetProviderPostedChartReportDetails(date, week, month, year, ProjectId, range, weekStartDate, weekEndDate);

                foreach (var providerPostedData in lstProviderPostedReportDetails)
                {
                    CodingDTO data = new CodingDTO();
                    data = providerPostedData.CodingDTO;
                    codingList.Add(data);
                }

                var table = clinicalcaseOperations.ToDataTable<CodingDTO>(codingList);
                table.TableName = "ProviderPostedChartDetails";
                table.Columns.Remove("ClinicalCaseID");
                if (ProjectType != "IP")
                {
                    table.Columns.Remove("ListName");
                }
                _logger.LogInformation("Loading Ended for ExportDetailedReport for User: " + mUserId);
                return ExportToExcel(table);
            }
            else
            {
                _logger.LogInformation("Loading Ended for ExportDetailedReport for User: " + mUserId);
                return null;
            }
        }

        [HttpPost]
        public IActionResult GetChartSummaryReport(int ProjectId, DateTime StartDate, DateTime EndDate, string dateType)
        {
            _logger.LogInformation("Loading Started for GetChartSummaryReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartSummaryReport = clinicalcaseOperations.GetChartSummaryReport(ProjectId, StartDate, EndDate, dateType);
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();
            ViewBag.ProjectId = ProjectId;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.ProjectType = projectType;
            ViewBag.DateType = dateType;
            _logger.LogInformation("Loading Ended for GetChartSummaryReport for User: " + mUserId);
            return PartialView("_ChartSummaryReport", lstChartSummaryReport);
        }
        public IActionResult ExportChartSummaryReport(int ProjectId, DateTime StartDate, DateTime EndDate, string dateType)
        {
            _logger.LogInformation("Loading Started for ExportChartSummaryReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartSummaryReport = clinicalcaseOperations.GetChartSummaryReport(ProjectId, StartDate, EndDate, dateType);
            lstChartSummaryReport.Tables[0].TableName = "ChartSummaryReport";
            _logger.LogInformation("Loading Ended for ExportChartSummaryReport for User: " + mUserId);
            return ExportToExcel(lstChartSummaryReport.Tables[0]);
        }

        public IActionResult ExportDetailedChartSummaryReport(int ProjectId, DateTime StartDate, DateTime EndDate, string dateType)
        {
            _logger.LogInformation("Loading Started for ExportChartSummaryReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartSummaryReport = clinicalcaseOperations.GetDetailedChartSummaryReport(ProjectId, StartDate, EndDate, dateType);
            lstChartSummaryReport.Tables[0].TableName = "ChartSummaryReportAllColumn";
            _logger.LogInformation("Loading Ended for ExportChartSummaryReport for User: " + mUserId);
            return ExportToExcel(lstChartSummaryReport.Tables[0]);
        }

        [HttpGet]
        public IActionResult GetChartSummaryReportDetails(string ColumnName, DateTime dos, int ProjectId, string DateType)
        {
            _logger.LogInformation("Loading Started for GetChartSummaryReportDetails for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartSummaryReportDetails = clinicalcaseOperations.GetChartSummaryReportDetails(ProjectId, dos, ColumnName,DateType);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();

            ViewBag.ColumnName = ColumnName;
            ViewBag.dos = dos.ToString("MM/dd/yyyy");
            ViewBag.projectname = projectname;
            ViewBag.projectType = projectType;
            ViewBag.ProjectId = ProjectId;
            ViewBag.DateType = DateType;
            _logger.LogInformation("Loading Ended for GetChartSummaryReportDetails for User: " + mUserId);
            return PartialView("_ChartSummaryReportDetails", lstChartSummaryReportDetails);
        }
        public IActionResult ExportChartSummaryReportDetails(string ColumnName, DateTime dos, int ProjectId, string DateType)
        {
            _logger.LogInformation("Loading Started for ExportChartSummaryReportDetails for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartSummaryReportDetails = clinicalcaseOperations.GetChartSummaryReportDetails(ProjectId, dos, ColumnName, DateType);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();
            List<CodingDTO> chartSummaryList = new List<CodingDTO>();
            foreach (var chartSummaryData in lstChartSummaryReportDetails)
            {
                CodingDTO data = new CodingDTO();
                data = chartSummaryData.CodingDTO;
                chartSummaryList.Add(data);
            }

            var table = clinicalcaseOperations.ToDataTable<CodingDTO>(chartSummaryList);
            table.TableName = "ChartSummaryReportDetails";
            table.Columns.Remove("ClinicalCaseID");
            if (projectType != "IP")
            {
                table.Columns.Remove("ListName");
            }
            _logger.LogInformation("Loading Ended for ExportChartSummaryReportDetails for User: " + mUserId);
            return ExportToExcel(table);
        }

        public IActionResult GetPostedChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetPostedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPostedChartReport = clinicalcaseOperations.GetPostedChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));

            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetPostedChartsReport for User: " + mUserId);
            return PartialView("_PostedChartReport", lstPostedChartReport);
        }
        public IActionResult ExportPostedChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for ExportPostedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstRPostedChartReport = clinicalcaseOperations.GetPostedChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));
            lstRPostedChartReport.Tables[0].TableName = "PostedChartsReport";
            clinicalcaseOperations.RemoveStartAndEndDate(lstRPostedChartReport, range);
            _logger.LogInformation("Loading Ended for ExportPostedChartsReport for User: " + mUserId);
            return ExportToExcel(lstRPostedChartReport.Tables[0]);
        }

        [HttpGet]
        public IActionResult GetPostedReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range, DateTime StartDate, DateTime EndDate, DateTime weekStartDate, DateTime weekEndDate)
        {
            _logger.LogInformation("Loading Started for GetPostedReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPostedReportDetails = clinicalcaseOperations.GetPostedChartReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);
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
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.WeekStartDate = weekStartDate;
            ViewBag.WeekEndDate = weekEndDate;
            ViewBag.ChartName = "Posted Chart Details";
            _logger.LogInformation("Loading Ended for GetPostedReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstPostedReportDetails);
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
            _logger.LogInformation("Loading Started for ExportBackLogChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstBacklogChartReport = clinicalcaseOperations.GetBacklogChartsReport(ProjectId, range, StartDate, EndDate);
            lstBacklogChartReport.Tables[0].TableName = "BackLogChartsReport";
            _logger.LogInformation("Loading Ended for ExportBackLogChartsReport for User: " + mUserId);
            return ExportToExcel(lstBacklogChartReport.Tables[0]);
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
            ViewBag.projectId = projectid;
            _logger.LogInformation("Loading Ended for GetBackLogChartsReportDetails for User: " + mUserId);
            return PartialView("_BackLogChartsReportDetails", lstBackLogChartReportDetails);
        }

        public IActionResult ExportBackLogChartsReportDetails(int delaydays, string status, int projectid)
        {
            _logger.LogInformation("Loading Started for ExportBackLogChartsReportDetails for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            int statusid = clinicalcaseOperations.GetStatusList().Where(x => x.Name == status).Select(x => x.StatusId).FirstOrDefault();
            var lstBackLogChartReportDetails = clinicalcaseOperations.GetBacklogChartsReportDetails(delaydays, statusid, projectid);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == projectid).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == projectid).Select(x => x.ProjectTypeName).FirstOrDefault();
            List<CodingDTO> backlogList = new List<CodingDTO>();
            foreach (var backlogData in lstBackLogChartReportDetails)
            {
                CodingDTO data = new CodingDTO();
                data = backlogData.CodingDTO;
                backlogList.Add(data);
            }

            var table = clinicalcaseOperations.ToDataTable<CodingDTO>(backlogList);
            table.TableName = "BackLogChartsReportDetails";
            table.Columns.Remove("ClinicalCaseID");
            if (projectType != "IP")
            {
                table.Columns.Remove("ListName");
            }
            _logger.LogInformation("Loading Ended for ExportBackLogChartsReportDetails for User: " + mUserId);
            return ExportToExcel(table);

        }

        public IActionResult GetCodedChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetCodedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstCodedChartReport = clinicalcaseOperations.GetCodedChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));

            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            _logger.LogInformation("Loading Ended for GetCodedChartsReport for User: " + mUserId);
            return PartialView("_CodedChartReport", lstCodedChartReport);
        }
        public IActionResult ExportCodedChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for ExportCodedChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstCodedChartReport = clinicalcaseOperations.GetCodedChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));
            lstCodedChartReport.Tables[0].TableName = "CodedChartsReport";
            clinicalcaseOperations.RemoveStartAndEndDate(lstCodedChartReport, range);
            _logger.LogInformation("Loading Ended for ExportCodedChartsReport for User: " + mUserId);
            return ExportToExcel(lstCodedChartReport.Tables[0]);
        }

        [HttpGet]
        public IActionResult GetCodedReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range, DateTime StartDate, DateTime EndDate,DateTime weekStartDate,DateTime weekEndDate)
        {
            _logger.LogInformation("Loading Started for GetCodedReportDetails for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstCodedReportDetails = clinicalcaseOperations.GetCodedChartReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);
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
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.WeekStartDate = weekStartDate;
            ViewBag.WeekEndDate = weekEndDate;
            ViewBag.ChartName = "Coded Chart Details";
            _logger.LogInformation("Loading Ended for GetCodedReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstCodedReportDetails);
        }

        public IActionResult GetQAChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetQAChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstqaChartReport = clinicalcaseOperations.GetQAChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));
            
            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetQAChartsReport for User: " + mUserId);
            return PartialView("_QAChartReport", lstqaChartReport);
        }

        public IActionResult ExportQAChartReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for ExportQAChartReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstqaChartReport = clinicalcaseOperations.GetQAChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));
            lstqaChartReport.Tables[0].TableName = "QAChartReport";
            clinicalcaseOperations.RemoveStartAndEndDate(lstqaChartReport, range);
            _logger.LogInformation("Loading Ended for ExportQAChartReport for User: " + mUserId);
            return ExportToExcel(lstqaChartReport.Tables[0]);
        }
        [HttpGet]
        public IActionResult GetQAReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range, DateTime StartDate, DateTime EndDate, DateTime weekStartDate, DateTime weekEndDate)
        {
            _logger.LogInformation("Loading Started for GetQAReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstQAReportDetails = clinicalcaseOperations.GetQAChartReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);
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
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.WeekStartDate = weekStartDate;
            ViewBag.WeekEndDate = weekEndDate;
            ViewBag.ChartName = "QA Chart Details";
            _logger.LogInformation("Loading Ended for GetQAReportDetails for User: " + mUserId);
            return PartialView("_DetailedReport", lstQAReportDetails);
        }

        public IActionResult GetPendingChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for GetPendingChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingChartReport = clinicalcaseOperations.GetPendingChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));

            ViewBag.ProjectId = ProjectId;
            ViewBag.range = range;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            _logger.LogInformation("Loading Ended for GetPendingChartsReport for User: " + mUserId);
            return PartialView("_PendingChartsReport", lstPendingChartReport);
        }
        public IActionResult ExportPendingChartsReport(int ProjectId, string range, DateTime StartDate, DateTime EndDate)
        {
            _logger.LogInformation("Loading Started for ExportPendingChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingChartReport = clinicalcaseOperations.GetPendingChartsReport(ProjectId, range, StartDate, EndDate.AddHours(23).AddMinutes(59).AddSeconds(59), Convert.ToDouble(timeZoneCookie));
            lstPendingChartReport.Tables[0].TableName = "PendingChartsReport";
            clinicalcaseOperations.RemoveStartAndEndDate(lstPendingChartReport, range);
            _logger.LogInformation("Loading Ended for ExportPendingChartsReport for User: " + mUserId);
            return ExportToExcel(lstPendingChartReport.Tables[0]);
        }

        [HttpGet]
        public IActionResult GetPendingReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range, DateTime StartDate, DateTime EndDate, DateTime weekStartDate, DateTime weekEndDate)
        {
            _logger.LogInformation("Loading Started for GetPendingReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetPendingReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);
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
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.WeekStartDate = weekStartDate;
            ViewBag.WeekEndDate = weekEndDate;
            _logger.LogInformation("Loading Ended for GetPendingReportDetails for User: " + mUserId);
            return PartialView("_PendingReportDetails", lstPendingReportDetails);
        }

        public IActionResult ExportPendingReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range, DateTime StartDate, DateTime EndDate, DateTime weekStartDate, DateTime weekEndDate)
        {
            _logger.LogInformation("Loading Started for ExportPendingReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetPendingReportDetails(date, week, month, year, ProjectId, range, Convert.ToDouble(timeZoneCookie), StartDate, EndDate, weekStartDate, weekEndDate);
            string projectname = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.Name).FirstOrDefault();
            string projectType = clinicalcaseOperations.GetProjects().Where(x => x.ProjectId == ProjectId).Select(x => x.ProjectTypeName).FirstOrDefault();
            List<CodingDTO> pendingList = new List<CodingDTO>();
            foreach (var pendingData in lstPendingReportDetails)
            {
                CodingDTO data = new CodingDTO();
                data = pendingData.CodingDTO;
                pendingList.Add(data);
            }

            var table = clinicalcaseOperations.ToDataTable<CodingDTO>(pendingList);
            table.TableName = "PendingReportDetails";
            table.Columns.Remove("ClinicalCaseID");
            if (projectType != "IP")
            {
                table.Columns.Remove("ListName");
            }
            _logger.LogInformation("Loading Ended for ExportPendingReportDetails for User: " + mUserId);
            return ExportToExcel(table);
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
            _logger.LogInformation("Loading Started for ExportProvidedpostedchartsChartsReport for User: " + mUserId);
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstChartReport = clinicalcaseOperations.GetProvidedpostedchartsChartsReport(ProjectId, range, StartDate, EndDate);
            lstChartReport.Tables[0].TableName = "ProviderPostedChartsReport";
            clinicalcaseOperations.RemoveStartAndEndDate(lstChartReport, range);
            _logger.LogInformation("Loading Ended for ExportProvidedpostedchartsChartsReport for User: " + mUserId);
            return ExportToExcel(lstChartReport.Tables[0]);
        }

        [HttpGet]
        public IActionResult GetProviderPostedReportDetails(DateTime date, int week, string month, string year, int ProjectId, string range, DateTime weekStartDate, DateTime weekEndDate)
        {
            _logger.LogInformation("Loading Started for GetProviderPostedReportDetails for User: " + mUserId);
            string createdDate = date.ToString();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(mUserId);
            var lstPendingReportDetails = clinicalcaseOperations.GetProviderPostedChartReportDetails(date, week, month, year, ProjectId, range, weekStartDate, weekEndDate);
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
            ViewBag.WeekStartDate = weekStartDate;
            ViewBag.WeekEndDate = weekEndDate;
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
