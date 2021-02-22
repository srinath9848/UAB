using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using UAB.DAL;
using UAB.DAL.LoginDTO;
using UAB.DAL.Models;

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

        [HttpGet]
        public IActionResult ManageUsers()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

            var users = clinicalcaseOperations.GetManageUsers();
            ViewBag.users = users;

            return View();
        }
        [HttpGet]
        public ActionResult AddUser()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

            ViewBag.Identityusers = clinicalcaseOperations.GetIdentityUsersList();
            ViewBag.Roles = clinicalcaseOperations.GetRolesList();
            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();

            return PartialView("_AddUser");
        }
        [HttpPost]
        public ActionResult AddUser(ApplicationUser model, string ProjectAndRole = null)
        {
            try
            {
                if (model.Email != null && !string.IsNullOrEmpty(ProjectAndRole))
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

                   
                        int UserId = clinicalcaseOperations.AddUser(model); //adding user to user table

                        if (UserId != 0)
                        {
                            foreach (string item in ProjectAndRole.Split(','))
                            {
                                model.ProjectName = item.Split('^')[0];
                                model.RoleName = item.Split('^')[1];

                                model.UserId = UserId;

                                clinicalcaseOperations.AddProjectUser(model); //adding user to projectuser table
                            }
                            TempData["Success"] = "Successfully Added User";
                        }
                        else
                        {
                            TempData["Warning"] = "Unable to add user to projects :User not there in UAB";
                        }
                    
                }
                else
                {
                    TempData["Error"] = "Unable to add user : Select Email or Project or Role";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public ActionResult UpdateUser(int userId) 
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

            ViewBag.Identityusers = clinicalcaseOperations.GetIdentityUsersList();
            ViewBag.Roles = clinicalcaseOperations.GetRolesList();
            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();

            var users = clinicalcaseOperations.GetUsers(userId);

            
            if (users != null)
            {

                    return PartialView("_UpdateUser", users);
               
            }
            else
            {
                TempData["Error"] = "Unable to get user ";
                return RedirectToAction("ManageUsers");
            }
            
        }
        [HttpPost]
        public ActionResult UpdateUser(ApplicationUser model, string ProjectAndRole = null)
        {
            try
            {
                ProjectAndRole = model.hdnProjectAndRole;
                if (model.Email != null && !string.IsNullOrEmpty(ProjectAndRole))
                {
                    List<ApplicationUser> list = new List<ApplicationUser>();
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

                    foreach (string item in ProjectAndRole.Split(','))
                    {
                        ApplicationUser applicationUser = new ApplicationUser();

                        applicationUser.ProjectName = item.Split('^')[0];
                        applicationUser.RoleName = item.Split('^')[1];
                        applicationUser.UserId = model.UserId;

                        list.Add(applicationUser);
                    }
                    if (list != null)
                    {
                        clinicalcaseOperations.UpdateProjectUser(list);
                        TempData["Success"] = "Successfully Updated  User";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public IActionResult DeleteUser(int userId)
        {
            
            if (userId != 0)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                 var user  = clinicalcaseOperations.Getuser(userId);
                if(user != null)
                {
                    return PartialView("_DeleteUser", user);
                }
                else
                {
                    TempData["Error"] = "Unable to Delete user ";
                }
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public IActionResult DeleteUser(User user)
        {
            try
            {
                if (user.UserId!=0)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                    clinicalcaseOperations.DeletetUser(user.UserId);
                    TempData["Success"] = "Successfully Deleted User";
                    return RedirectToAction("ManageUsers");
                }

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
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
