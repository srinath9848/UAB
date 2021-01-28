using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace UAB.Controllers
{
    public class UABController : Controller
    {
        public IActionResult CodingSummary()
        {
            return View();
        }

        public IActionResult Coding()
        {
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
