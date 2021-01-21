using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace UAB.Controllers
{
    public class UABController : Controller
    {
        public IActionResult Coding()
        {
            return View();
        }
        public IActionResult GlobalAuditing()
        {
            return View();
        }

        public IActionResult DomesticAuditing()
        {
            return View();
        }

    }
}
