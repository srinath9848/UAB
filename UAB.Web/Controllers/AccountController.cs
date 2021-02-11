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

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromForm] LoginViewModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.Password))
                    {
                        return RedirectToAction("Login", "Account");
                    }
                    var signInResult = _mAuthenticationService.SignIn(model.Email, model.Password);
                    if (signInResult.Result != 0)
                    {
                        Auth.isAuth = false;
                        throw new ApplicationException("Invalid credentails");
                        
                    }
                    Auth.isAuth = true;
                    Auth.CurrentUserName = model.Email;
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["LoginFailed"] = ex.Message;
            }
            return View();
            
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
