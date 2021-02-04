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

namespace UAB.Controllers
{
    public class UABController : Controller
    {
        UAB.DAL.Models.UABContext _dbcontext = null;
        public IActionResult CodingSummary()
        {
            List<DashboardDTO> lstDto = new List<DashboardDTO>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            lstDto = clinicalcaseOperations.GetChartCountByStatus();

            //lstworkitem.Add(new DashboardDTO() { ProjectName = "Phase1 - Ambulatory", AvailableCount = 1, IncorrectCount = 0, ApprovedCount = 0 });
            //lstworkitem.Add(new DashboardDTO() { ProjectName = "Phase1 - IP", AvailableCount = 0, IncorrectCount = 0, ApprovedCount = 0 });
            //lstworkitem.Add(new DashboardDTO() { ProjectName = "Phase2 - Pulmonology", AvailableCount = 0, IncorrectCount = 0, ApprovedCount = 0 });



            //using (_dbcontext = new DAL.Models.UABContext())
            //{

            //lstworkitem = (from cc in _dbcontext.ClinicalCase
            //               select new UABDashboardDetails()
            //               {
            //                   ProjectName = cc.Project.Name,
            //                   AvailableCount = cc.WorkItem.Select(a => a.StatusId == 1).Count(),
            //                   IncorrectCount = cc.WorkItem.Select(a => a.StatusId == 2).Count(),
            //                   ApprovedCount = cc.WorkItem.Select(a => a.StatusId == 3).Count()
            //               }).ToList();

            //var qry = (from pr in _dbcontext.Project
            //           join cc in _dbcontext.ClinicalCase on pr.ProjectId equals cc.ProjectId
            //           into jrs
            //           from jrResult in jrs.DefaultIfEmpty()
            //           join w in _dbcontext.WorkItem on jrResult.ClinicalCaseId equals w.ClinicalCaseId
            //            into jr
            //           from jrresult in jrs.DefaultIfEmpty().Select(
            //           new UABDashboardDetails()
            //           (
            //               ProjectName = pr.Name,
            //               AvailableCount = jrresult.WorkItem.Select(a => a.StatusId == 1).Count(),
            //               IncorrectCount = jrresult.WorkItem.Select(a => a.StatusId == 2).Count(),
            //               ApprovedCount = jrresult.WorkItem.Select(a => a.StatusId == 3).Count()

            //           )).ToList();

            //}
            return View(lstDto);
        }

        private object UABDashboardDetails()
        {
            throw new NotImplementedException();
        }

        public IActionResult Coding()
        {
            using (_dbcontext = new DAL.Models.UABContext())
            {

            }
            return View();
        }

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
        public IActionResult QA()
        {
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

        [HttpPost]
        public IActionResult AddSettingsProvider(Provider provider)
        {
            if (ModelState.IsValid)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                clinicalcaseOperations.AddProvider(provider);
                List<Provider> lstProvider = new List<Provider>();
                lstProvider = clinicalcaseOperations.GetProviders();
                ViewBag.lstProvider = lstProvider;
            }
            return View("SettingsProvider");
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
        public IActionResult Provider()
        {
            using (_dbcontext = new DAL.Models.UABContext())
            {
                //Provider provider = new Provider();
                //var providerList = _dbcontext.Provider.Select(x => x).ToList();
                //return View(providerList);
                return View();
            }
        }

        public IActionResult Payor()
        {
            using (_dbcontext = new DAL.Models.UABContext())
            {
                //Payor payor = new Payor();
                //var payorList = _dbcontext.Payor.Select(x => x).ToList();
                //return View(payorList);
                return View();
            }
        }
    }
}
