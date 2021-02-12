using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using UAB.DAL.LoginDTO;

namespace UAB.Controllers
{
    public class AccountController : Controller
    {

        private readonly IAuthenticationService _mAuthenticationService;
        //private readonly UserManager<IdentityUser> _userManager;
        //private readonly SignInManager<IdentityUser> _signInManager;
        public AccountController(IAuthenticationService mAuthenticationService
            //, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager
            )
        {
            _mAuthenticationService = mAuthenticationService;
            //_userManager = userManager;
            //_signInManager = signInManager;

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


        [HttpGet]
        public IActionResult Logout()
        {
            Auth.isAuth = false;
            // _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
