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

namespace UAB.Controllers
{
    public class UABController : Controller
    {
        public IActionResult CodingSummary()
        {
            List<DashboardDTO> lstDto = new List<DashboardDTO>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

            List<int> lstStatus = new List<int> { (int)Statuses.ReadyForCoding, (int)Statuses.QARejected, (int)Statuses.ShadowQARejected,
                (int)Statuses.PostingCompleted };

            lstDto = clinicalcaseOperations.GetChartCountByStatus(string.Join(",", lstStatus));

            return View(lstDto);
        }
        private object UABDashboardDetails()
        {
            throw new NotImplementedException();
        }

        public IActionResult Coding(string StatusIDs, int ProjectID)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            CodingSubmitDTO codingSubmitDTO = new CodingSubmitDTO();
            codingSubmitDTO.CodingDTO = clinicalcaseOperations.GetNext(StatusIDs, ProjectID);

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

            List<int> lstStatus = new List<int> { (int)Statuses.ReadyForCoding, (int)Statuses.QARejected, (int)Statuses.ShadowQARejected,
                (int)Statuses.PostingCompleted };

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByStatus(string.Join(",", lstStatus));

            TempData["Success"] = "Chats Details submitted succesfully !";
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
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            List<int> lstStatus = new List<int> { (int)Statuses.CodingCompleted,(int)Statuses.ShadowQARejected,
                (int)Statuses.CoderRejected };

            List<DashboardDTO> lstDto = clinicalcaseOperations.GetChartCountByStatus(string.Join(",", lstStatus));

            return View(lstDto);
        }
        public IActionResult QA(string StatusIDs, int ProjectID)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            CodingSubmitDTO codingSubmitDTO = new CodingSubmitDTO();
            codingSubmitDTO.CodingDTO = clinicalcaseOperations.GetNext(StatusIDs, ProjectID);
            #region binding data
            ViewBag.Payors = clinicalcaseOperations.GetPayorsList();
            ViewBag.Providers = clinicalcaseOperations.GetProvidersList();
            ViewBag.ProviderFeedbacks = clinicalcaseOperations.GetProviderFeedbacksList();
            #endregion
            return View(codingSubmitDTO);
        }

        public IActionResult RebuttalCharts(string StatusIDs, int ProjectID)
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

        [HttpPost]
        public IActionResult AddSettingsProvider(Provider provider)
        {
            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                if (provider.ProviderId == null)
                    clinicalcaseOperations.AddProvider(provider);
                else
                    clinicalcaseOperations.AddProvider(provider); // Update
            }
            return RedirectToAction("SettingsProvider");
        }

        [HttpGet]
        public IActionResult SettingsProvider()
        {
            List<Provider> lstProvider = new List<Provider>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
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
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
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
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
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
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                if (provider.ProviderId != 0)
                    clinicalcaseOperations.AddProvider(provider); // Delete
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
            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                clinicalcaseOperations.AddPayor(payor);
                List<Payor> lstPayor = new List<Payor>();
                lstPayor = clinicalcaseOperations.GetPayors();
                ViewBag.lstPayor = lstPayor;
            }
            return View("SettingsPayor");
        }
        [HttpGet]
        public IActionResult SettingsPayor()
        {
            List<Payor> lstPayor = new List<Payor>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            lstPayor = clinicalcaseOperations.GetPayors();
            ViewBag.lstPayor = lstPayor;
            return View();
        }

        [HttpPost]
        public IActionResult AddSettingsProviderFeedback(ProviderFeedback providerFeedback)
        {
            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                clinicalcaseOperations.AddProviderFeedback(providerFeedback);
                List<BindDTO> lstProviderFeedback = new List<BindDTO>();
                lstProviderFeedback = clinicalcaseOperations.GetProviderFeedbacksList();
                ViewBag.lstProviderFeedback = lstProviderFeedback;
            }
            return View("SettingsProviderFeedback");
        }
        [HttpGet]
        public IActionResult SettingsProviderFeedback()
        {
            List<BindDTO> lstProviderFeedback = new List<BindDTO>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            lstProviderFeedback = clinicalcaseOperations.GetProviderFeedbacksList();
            ViewBag.lstProviderFeedback = lstProviderFeedback;
            return View();
        }
    }
}
