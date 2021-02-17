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
            if (Auth.IsAuth)
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
                Auth.IsAuth = false;
                TempData["Error"] = "Invalid sign-in. Please try again.";
                return View();
            }
            else
            {
                var userInfo = _mAuthenticationService.GetUserInfoByEmail(Email);
                if (userInfo.IsActiveUser)
                {
                    Auth.IsAuth = true;
                    Auth.EmailId = Email;
                    Auth.CurrentRole = userInfo.RoleName;
                    Auth.UserId = userInfo.UserId;
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
            List<ApplicationUser> users = new List<ApplicationUser>();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

            users = clinicalcaseOperations.GetUsers();
            ViewBag.users = users;

            return View();
        }
        [HttpGet]
        public ActionResult Add_EditUser(int UserId = 0)
        {
            ApplicationUser obj = new ApplicationUser();
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            ViewBag.Roles = clinicalcaseOperations.GetRolesList();
            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();
            if (UserId != 0)
            {
                List<ApplicationUser> users = new List<ApplicationUser>();
                users = clinicalcaseOperations.GetUsers();
                obj = users.Find(a => a.UserId == UserId);
            }
            return PartialView("_AddEditUser", obj);
        }

        [HttpPost]
        public ActionResult Add_EditUser(ApplicationUser model, string ProjectAndRole = null)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            ApplicationUser applicationUser = new ApplicationUser();

            if (model.UserId == 0)
            {
                int UserId = clinicalcaseOperations.AddUser(model);//adding user to user table
                if (!string.IsNullOrEmpty(ProjectAndRole))
                {
                    foreach (string item in ProjectAndRole.Split(','))
                    {
                        model.ProjectName = item.Split('^')[0];
                        model.RoleName = item.Split('^')[1];

                        model.UserId = UserId;

                        clinicalcaseOperations.AddProjectUser(model); //adding user and projects,roles to projectuser table
                    }
                }
            }
            else
            {

            }

            return RedirectToAction("ManageUsers");
        }
        [HttpGet]
        public IActionResult DeleteUser(int UserId)
        {
            ApplicationUser obj = new ApplicationUser();
            if (UserId != 0)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                var res = clinicalcaseOperations.Getuser(UserId);
                obj = res;
            }
            return PartialView("_DeleteUser", obj);
        }

        [HttpPost]
        public IActionResult DeleteUser(ApplicationUser applicationUser)
        {

            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            Auth.IsAuth = false;
            return RedirectToAction("Login", "Account");
        }
    }
}
