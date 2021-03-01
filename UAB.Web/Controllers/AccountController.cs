﻿using Microsoft.AspNetCore.Authorization;
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
                if (userInfo.Email != null)
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
                            model.SamplePercentage = item.Split('^')[2];

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
        [Route("Account/UserDetails")]
        [Route("UserDetails/{userId}")]
        public ActionResult UserDetails(int userId)
        {
            if (userId!=0)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                var UserProjects = clinicalcaseOperations.GetUserProjects(userId);
                ViewBag.UserProjects = UserProjects;
                return View("UserDetails", UserProjects);
            }
            return RedirectToAction("ManageUsers");
            
        }
        [HttpGet]
        public IActionResult DeleteUser(int userId)
        {

            if (userId != 0)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                var user = clinicalcaseOperations.Getuser(userId);
                if (user != null)
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
                if (user.UserId != 0)
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
        public ActionResult AddProjectUser(int userId)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();

            ViewBag.Roles = clinicalcaseOperations.GetRolesList();
            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();

            ApplicationUser appuser = new ApplicationUser();
            var user= clinicalcaseOperations.Getuser(userId);
            appuser.Email = user.Email;
            appuser.UserId = user.UserId;
            return PartialView("_AddProjectUser", appuser);
        }
        [HttpPost]
        public ActionResult AddProjectUser(ApplicationUser model, string ProjectAndRole = null)
        {
            
                if (model.UserId != 0 && !string.IsNullOrEmpty(ProjectAndRole))
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                   
                        foreach (string item in ProjectAndRole.Split(','))
                        {
                            model.ProjectName = item.Split('^')[0];
                            model.RoleName = item.Split('^')[1];
                            model.SamplePercentage = item.Split('^')[2];

                            clinicalcaseOperations.AddProjectUser(model);
                        }
                        TempData["Success"] = "Successfully Project Added User";
                    return RedirectToAction("UserDetails", new { UserId = model.UserId });
                }
                return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public IActionResult UpdateProjectUser(int projectuserid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            var projectuser = clinicalcaseOperations.GetProjectUser(projectuserid);
            ViewBag.Roles = clinicalcaseOperations.GetRolesList();

            return PartialView("_UpdateProjectUser", projectuser);
        }
        [HttpPost]
        public IActionResult UpdateProjectUser(ApplicationUser model, string user  = null)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            int i =clinicalcaseOperations.UpdateProjectUser(model);
            if (i==1)
            {
                TempData["Success"] = "Successfully Project User Updated";
                return RedirectToAction("UserDetails", new { UserId = model.UserId });
            }
            else
            {
                TempData["Warning"] = "Unable to  update  project user :no change is there";
                return RedirectToAction("UserDetails", new { UserId = model.UserId });
            }
           

        }
        [HttpGet]
        public IActionResult DeleteProjectUser(int projectuserid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
            var projectuser = clinicalcaseOperations.GetProjectUser(projectuserid);
            ViewBag.Roles = clinicalcaseOperations.GetRolesList();


            return PartialView("_DeleteProjectUser", projectuser);
        }
        [HttpPost]
        public IActionResult DeleteProjectUser(ApplicationUser model, string user  = null)
        {

            try
            {
                if (model.ProjectUserId != 0)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations();
                    clinicalcaseOperations.DeletetProjectUser(model.ProjectUserId);
                    TempData["Success"] = "Successfully Project User Deleted";
                    return RedirectToAction("UserDetails", new { UserId  = model.UserId });
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
