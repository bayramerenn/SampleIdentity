using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SampleIdentity.UI.Helper;
using SampleIdentity.UI.Models.AppUser;

namespace SampleIdentity.UI.Controllers
{
    public class BaseController : Controller
    {
        protected UserManager<AppUser> _userManager;
        protected SignInManager<AppUser> _signInManager;
        protected RoleManager<AppRole> _roleManager;
        protected IPasswordReset _passwordReset;


        protected AppUser CurrentUser => _userManager.FindByNameAsync(User.Identity.Name).Result;

        public BaseController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IPasswordReset passwordReset, RoleManager<AppRole> roleManager = null)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _passwordReset = passwordReset;
            _roleManager = roleManager;
        }

        protected List<string> Errors { get; set; }
        public void AddModelError(IdentityResult result)
        {
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }
        }

        public async void SignInAndOut()
        {
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(CurrentUser, true);
        }
    }
}