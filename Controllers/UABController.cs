using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UAB.Models;
using Microsoft.EntityFrameworkCore;

namespace UAB.Controllers
{
    public class UABController : Controller
    {
        UAB.DAL.Models.UABContext _dbcontext = null;
        public IActionResult CodingSummary()
        {
            List<UAB.Models.UABDashboardDetails> lstworkitem = new List<UAB.Models.UABDashboardDetails>();

            lstworkitem.Add(new UABDashboardDetails() { ProjectName = "Phase1 - Ambulatory", AvailableCount = 1, IncorrectCount = 0, ApprovedCount = 0 });
            lstworkitem.Add(new UABDashboardDetails() { ProjectName = "Phase1 - IP", AvailableCount = 0, IncorrectCount = 0, ApprovedCount = 0 });
            lstworkitem.Add(new UABDashboardDetails() { ProjectName = "Phase2 - Pulmonology", AvailableCount = 0, IncorrectCount = 0, ApprovedCount = 0 });



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
            return View(lstworkitem);
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
