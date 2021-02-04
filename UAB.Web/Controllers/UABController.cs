using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UAB.Models;
using Microsoft.EntityFrameworkCore;
using UAB.DAL;
using UAB.DTO;

namespace UAB.Controllers
{
    public class UABController : Controller
    {
        public IActionResult CodingSummary()
        {
            List<DashboardDTO> lstDto = getCodingSummary();

            return View(lstDto);
        }
        List<DashboardDTO> getCodingSummary()
        {
            List<DashboardDTO> lstDto = new List<DashboardDTO>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            lstDto = clinicalcaseOperations.GetChartCountByStatus();

            return lstDto;
        }

        private object UABDashboardDetails()
        {
            throw new NotImplementedException();
        }

        public IActionResult Coding(int StatusID, int ProjectID)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            CodingSubmitDTO codingSubmitDTO = new CodingSubmitDTO();
            codingSubmitDTO.CodingDTO = clinicalcaseOperations.GetNext(StatusID, ProjectID);

            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList(); 
            #endregion

            return View(codingSubmitDTO);
        }
        [HttpPost]
        public IActionResult Submit(CodingSubmitDTO codingSubmitDTO, string codingSubmit, string hold)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

            if (!string.IsNullOrEmpty(codingSubmit))
                clinicalcaseOperations.SubmitCoding(codingSubmitDTO);
            else if (!string.IsNullOrEmpty(hold))
                submitHold();

            List<DashboardDTO> lstDto = getCodingSummary();

            return View("CodingSummary", lstDto);
        }
        void submitHold() { }
        public IActionResult IncorrectCharts()
        {
            return View();
        }

        public IActionResult ApprovedCharts()
        {
            return View();
        }

        public IActionResult QASummary()
        {
            return View();
        }
        public IActionResult QA(int StatusID, int ProjectID)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            CodingSubmitDTO codingSubmitDTO = new CodingSubmitDTO();
            codingSubmitDTO.CodingDTO = clinicalcaseOperations.GetNext(StatusID, ProjectID);

            return View();
        }

        public IActionResult RebuttalCharts()
        {
            return View();
        }

        public IActionResult ShadowQASummary()
        {
            return View();
        }
        public IActionResult ShadowQA()
        {
            return View();
        }
    }
}
