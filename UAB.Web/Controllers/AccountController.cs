using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using UAB.DAL;
using UAB.DAL.LoginDTO;

namespace UAB.Controllers
{
    public class AccountController : Controller
    {

        private readonly IAuthenticationService _mAuthenticationService;
        public AccountController(IAuthenticationService mAuthenticationService
            )
        {
            _mAuthenticationService = mAuthenticationService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (Auth.isAuth)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            var signInResult = _mAuthenticationService.SignIn(Email, Password);
            if (signInResult.Result != 0)
            {
                Auth.isAuth = false;
                TempData["Error"] = "Invalid sign-in. Please try again.";
                return View();
            }
            else
            {
                var userInfo = _mAuthenticationService.GetUserInfoByEmail(Email);
                if (userInfo.IsActiveUser) {
                    Auth.isAuth = true;
                    Auth.CurrentUserName = Email;
                    Auth.CurrentRole = userInfo.RoleName;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Error"] = "Invalid sign-in.You are not a UAB user.";
                    return View();
                }
            }
        }
       
        public IActionResult ManageUsers()
        {
            List<ApplicationUser> users  = new List<ApplicationUser>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

            users = clinicalcaseOperations.GetUsers(); 
            ViewBag.users = users;

            return View();
        }
        [HttpGet]
        public ActionResult Add_EditUser (int UserId  = 0)
        {
            ApplicationUser obj = new ApplicationUser();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            ViewBag.Roles = clinicalcaseOperations.GetRolesList();
            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();
            if (UserId != 0)
            {
                List<ApplicationUser> users = new List<ApplicationUser>();
                users = clinicalcaseOperations.GetUsers();
                var res = users.Find(a => a.UserId == UserId);
                obj = res;
            }
            return PartialView("_AddEditUser", obj);
        }
        [HttpGet]
        public IActionResult DeleteUser (int UserId)
        {
            ApplicationUser obj = new ApplicationUser();
            if (UserId != 0)
            {
                List<ApplicationUser> users = new List<ApplicationUser>();
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                users = clinicalcaseOperations.GetUsers();             
                var res = users.Find(a => a.UserId == UserId);
                obj = res;
            }
            return PartialView("_DeleteUser", obj);
        }

        [HttpPost]
        public IActionResult DeleteUser (ApplicationUser applicationUser )
        {
            try
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                if (applicationUser.UserId != 0)
                    clinicalcaseOperations.DeleteUser(applicationUser);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            Auth.isAuth = false;
            return RedirectToAction("Login", "Account");
        }
    }
}
