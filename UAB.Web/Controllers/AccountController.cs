using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UAB.DAL;
using UAB.DAL.LoginDTO;
using UAB.DAL.Models;
using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using identity = Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace UAB.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly int _mUserId;
        private readonly IAuthenticationService1 _mAuthenticationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _mUserRole;

        public AccountController(IAuthenticationService1 mAuthenticationService, IHttpContextAccessor httpContextAccessor)
        {
            _mAuthenticationService = mAuthenticationService;
            _httpContextAccessor = httpContextAccessor;
            _mUserId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value);
            _mUserRole = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        }

        public IActionResult SignIn()
        {
            string offSet = Request.Form["hdnOffset"].ToString();
            CreateCookie(offSet);
            if (_mUserRole.Split(",").Contains("Manager"))
                return RedirectToAction("GetAgingReport", "Home");
            else if (_mUserRole.Split(",").Contains("Supervisor"))
                return RedirectToAction("GetAgingReport", "Home");
            else if (_mUserRole.Split(",").Contains("Coder"))
                return RedirectToAction("CodingSummary", "UAB");
            else if (_mUserRole.Split(",").Contains("QA"))
                return RedirectToAction("QASummary", "UAB");
            else if (_mUserRole.Split(",").Contains("ShadowQA"))
                return RedirectToAction("ShadowQASummary", "UAB");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(string Email, string Password)
        {
            //var signInResult = _mAuthenticationService.SignIn(Email, Password);
            //if (signInResult.Result != 0)
            //{
            //    TempData["Error"] = "Invalid sign-in. Please try again.";
            //    return View();
            //}
            //else
            //{
            //   var userInfo = _mAuthenticationService.GetUserInfoByEmail(Email);
            //    if (userInfo.Email != null)
            //    {
            //        string offSet = Request.Form["hdnOffset"].ToString();
            //        CreateCookie(offSet);
            //        _mUserId = userInfo.UserId;
            //        var claims = new List<Claim>
            //            {
            //                new Claim(ClaimTypes.Sid, userInfo.UserId.ToString()),
            //                new Claim(ClaimTypes.Email, Email),
            //                new Claim(ClaimTypes.Role, userInfo.RoleName)
            //            };

            //        var claimsIdentity = new ClaimsIdentity(
            //            claims,
            //            CookieAuthenticationDefaults.AuthenticationScheme);

            //        await HttpContext.SignInAsync(
            //            CookieAuthenticationDefaults.AuthenticationScheme,
            //            new ClaimsPrincipal(claimsIdentity),
            //            new AuthenticationProperties
            //            {
            //                IsPersistent = true,
            //                ExpiresUtc = DateTime.UtcNow.AddMinutes(20)
            //            });

            //        if (userInfo.RoleName.Split(",").Contains("Manager"))
            //            return RedirectToAction("GetAgingReport", "Home");
            //        else if (userInfo.RoleName.Split(",").Contains("Supervisor"))
            //            return RedirectToAction("GetAgingReport", "Home");
            //        else if (userInfo.RoleName.Split(",").Contains("Coder"))
            //            return RedirectToAction("CodingSummary", "UAB");
            //        else if (userInfo.RoleName.Split(",").Contains("QA"))
            //            return RedirectToAction("QASummary", "UAB");
            //        else if (userInfo.RoleName.Split(",").Contains("ShadowQA"))
            //            return RedirectToAction("ShadowQASummary", "UAB");

            //        return RedirectToAction("Index", "Home");
            //    }
            //    else
            //    {
            //        TempData["Error"] = "Invalid sign-in.You are not a UAB user.";
            //        return View();
            //    }
            //}
            return RedirectToAction("Index", "Home");
        }
        void CreateCookie(string offSet)
        {
            string timeZoneCookie = _httpContextAccessor.HttpContext.Request.Cookies["UAB_TimeZoneOffset"];

            if (timeZoneCookie == null)
            {
                CookieOptions option = new CookieOptions();
                option.Expires = new DateTimeOffset(DateTime.Now.AddDays(1));
                Response.Cookies.Append("UAB_TimeZoneOffset", offSet, option);
            }
        }

        [HttpGet]
        public IActionResult ManageUsers()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);

            var users = clinicalcaseOperations.GetManageUsers();
            ViewBag.users = users;

            return View();
        }
        [HttpGet]
        public ActionResult AddUser()
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);

            ViewBag.Identityusers = clinicalcaseOperations.GetIdentityUsersList();
            ViewBag.Roles = clinicalcaseOperations.GetRolesList();
            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();

            return PartialView("_AddUser");
        }
        bool ValidateEmaildID(string emailID, ref AzureADResult azureADResult)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            string SecretKey = @"-m2XU-K8~n2bghSP5VB6d3-_sPEOeHaAeQ";// root.GetSection("SecretKey").Value;
            string clientId = root.GetSection("AzureAd:ClientId").Value;
            string tenant = root.GetSection("AzureAd:TenantId").Value;

            identity.IConfidentialClientApplication confidentialClientApplication = identity.ConfidentialClientApplicationBuilder
                            .Create(clientId)
                            .WithTenantId(tenant)
                            .WithClientSecret(SecretKey)
                            .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            var confidentialClient = confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync();

            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", confidentialClient.Result.AccessToken);

            string environmentsUri = "https://graph.microsoft.com/v1.0/users/" + emailID;

            var response = httpClient.GetAsync(environmentsUri).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            azureADResult = JsonConvert.DeserializeObject<AzureADResult>(content);

            return response.IsSuccessStatusCode;
        }
        public class AzureADResult
        {
            //equal to FirstName in Azure Portal
            public string givenName { get; set; }
            //equal to LastName in Azure Portal
            public string surname { get; set; }
            //equal to User Office Phone in Azure Portal
            public List<string> businessPhones { get; set; }
        }
        [HttpPost]
        public ActionResult AddUser(ApplicationUser model, string ProjectAndRole = null)
        {
            try
            {
                if (model.Email != null && !string.IsNullOrEmpty(ProjectAndRole))
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);

                    AzureADResult azureADResult = new AzureADResult();
                    bool emailFound = ValidateEmaildID(model.Email, ref azureADResult);

                    if (emailFound)
                    {
                        model.FirstName = azureADResult.givenName;
                        model.LastName = azureADResult.surname;

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
                        TempData["Error"] = "Unable to add user : Email not existed in AzureAD";
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
            if (userId != 0)
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);
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
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);
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
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);
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
        [HttpPost]
        public IActionResult changeStatus(int userID, bool IsActive)
        {
            try
            {
                if (userID != 0)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);
                    clinicalcaseOperations.ChangeStatus(userID, IsActive);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return Content(IsActive == true ? "User Status Changed to Active" : "User Status Changed to InActive");
        }

        [HttpGet]
        public ActionResult AddProjectUser(int userId)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);

            ViewBag.Roles = clinicalcaseOperations.GetRolesList();
            ViewBag.Projects = clinicalcaseOperations.GetProjectsList();

            var UserProjects = clinicalcaseOperations.GetUserProjects(userId);
            string hdnroleproject = null;
            if (UserProjects.Count > 1)
            {
                hdnroleproject = UserProjects.FirstOrDefault().hdnProjectAndRole.ToString();
            }
            ApplicationUser appuser = new ApplicationUser();
            var user = clinicalcaseOperations.Getuser(userId);
            appuser.Email = user.Email;
            appuser.UserId = user.UserId;
            appuser.hdnProjectAndRole = hdnroleproject;
            return PartialView("_AddProjectUser", appuser);
        }
        [HttpPost]
        public ActionResult AddProjectUser(ApplicationUser model, string ProjectAndRole = null)
        {

            if (model.UserId != 0 && !string.IsNullOrEmpty(ProjectAndRole))
            {
                ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);

                try
                {
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
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
                return RedirectToAction("UserDetails", new { UserId = model.UserId });
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public IActionResult UpdateProjectUser(int projectuserid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);
            var projectuser = clinicalcaseOperations.GetProjectUser(projectuserid);
            ViewBag.Roles = clinicalcaseOperations.GetRolesList();

            return PartialView("_UpdateProjectUser", projectuser);
        }
        [HttpPost]
        public IActionResult UpdateProjectUser(ApplicationUser model, string user = null)
        {

            try
            {
                if (model.RoleId != 0 && !string.IsNullOrWhiteSpace(model.SamplePercentage)
                && model.ProjectUserId != 0 && model.UserId != 0)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);
                    clinicalcaseOperations.UpdateProjectUser(model);
                    TempData["Success"] = "Successfully Project User Updated";
                }
                else
                {
                    TempData["error"] = "Please enter the data";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("UserDetails", new { UserId = model.UserId });
        }

        [HttpGet]
        public IActionResult DeleteProjectUser(int projectuserid)
        {
            ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);
            var projectuser = clinicalcaseOperations.GetProjectUser(projectuserid);
            ViewBag.Roles = clinicalcaseOperations.GetRolesList();


            return PartialView("_DeleteProjectUser", projectuser);
        }
        [HttpPost]
        public IActionResult DeleteProjectUser(ApplicationUser model, string user = null)
        {

            try
            {
                if (model.ProjectUserId != 0)
                {
                    ClinicalcaseOperations clinicalcaseOperations = new ClinicalcaseOperations(_mUserId);
                    clinicalcaseOperations.DeletetProjectUser(model.ProjectUserId);
                    TempData["Success"] = "Successfully Project User Deleted";
                    return RedirectToAction("UserDetails", new { UserId = model.UserId });
                }

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("UserDetails", new { UserId = model.UserId });
        }

        //[HttpGet]
        //public async Task<IActionResult> SignOut()
        public void SignOut()
        {
            HttpContext.SignOutAsync(
                OpenIdConnectDefaults.AuthenticationScheme);

            HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
            _httpContextAccessor.HttpContext.Session.Remove("PayorsList");
            _httpContextAccessor.HttpContext.Session.Remove("ProvidersList");
            _httpContextAccessor.HttpContext.Session.Remove("FeedbackList");
            // return RedirectToAction("Login", "Account");
        }
    }
}
