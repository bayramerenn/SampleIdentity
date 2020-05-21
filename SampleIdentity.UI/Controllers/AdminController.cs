using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SampleIdentity.UI.Models.AppUser;
using SampleIdentity.UI.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;
using SampleIdentity.UI.Enums;
using SampleIdentity.UI.Middleware;

namespace SampleIdentity.UI.Controllers
{
    // [Authorize(Roles ="Admin")]
    public class AdminController : BaseController
    {

        public AdminController(UserManager<AppUser> _userManager,SignInManager<AppUser> _signInManager, RoleManager<AppRole> _roleManager) : base(_userManager, _signInManager, null, _roleManager)
        { }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Roles()
        {
            return View(_roleManager.Roles.ToList());
        }
        public IActionResult RoleCreate(string? id)
        {
            if (id != null)
            {
                var role = _roleManager.FindByIdAsync(id).Result;

                var roleViewModel = new RoleViewModel
                {
                    Name = role.Name
                };

                return View(roleViewModel);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RoleCreate(RoleViewModel roleViewModel)
        {

            if (ModelState.IsValid)
            {


                if (roleViewModel.Id != null)
                {   //Güncelleme işlemlerini yapıyorum

                    AppRole updateRole = await _roleManager.FindByIdAsync(roleViewModel.Id);

                    updateRole.Name = roleViewModel.Name;

                    await _roleManager.UpdateAsync(updateRole);

                    return RedirectToAction("Roles");
                }


                AppRole role = new AppRole(roleViewModel.Name);
                IdentityResult result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
                else
                {
                    AddModelError(result);
                }

            }
            return View(roleViewModel);
        }

        public IActionResult Claims()
        {
            return View(User.Claims.ToList());
        }
        public IActionResult Users()
        {
            return View(_userManager.Users.ToList());
        }
        public IActionResult RoleDelete(string id)
        {
            AppRole role = _roleManager.FindByIdAsync(id).Result;
            if (role != null)
            {
                IdentityResult result = _roleManager.DeleteAsync(role).Result;
                if (result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
                else
                {
                    AddModelError(result);
                }
            }

            return View();
        }

        public IActionResult RoleAssign(string id)
        {
            TempData["userId"] = id;
            AppUser user = _userManager.FindByIdAsync(id).Result;

            ViewBag.UserName = user.UserName;

            IQueryable<AppRole> roles = _roleManager.Roles;

            List<string> userRoles = _userManager.GetRolesAsync(user).Result as List<string>;

            List<RoleAssignViewModel> roleAssignViewModels = new List<RoleAssignViewModel>();

            foreach (var role in roles)
            {
                RoleAssignViewModel r = new RoleAssignViewModel();
                r.RoleId = role.Id;
                r.RoleName = role.Name;
                if (userRoles.Contains(role.Name))
                {
                    r.Exist = true;
                }
                else
                {
                    r.Exist = false;
                }
                roleAssignViewModels.Add(r);
            }

            return View(roleAssignViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> RoleAssign(List<RoleAssignViewModel> roleAssignViews)
        {

            AppUser user = _userManager.FindByIdAsync(TempData["userId"].ToString()).Result;

            foreach (var item in roleAssignViews)
            {
                if (item.Exist)
                {
                    await _userManager.AddToRoleAsync(user, item.RoleName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }
            return RedirectToAction("Users");
        }

        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordChange(PasswordChangeViewModel passwordChangeViewModel,string id)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByIdAsync(id);

                var passwordCheck =await _userManager.CheckPasswordAsync(user, passwordChangeViewModel.PasswordOld);

                if (passwordCheck)
                {
                    var result =await _userManager.ChangePasswordAsync(user, passwordChangeViewModel.PasswordOld, passwordChangeViewModel.PasswordNew);
                    if (result.Succeeded)
                    {
                      
                        await _userManager.UpdateSecurityStampAsync(user);
                        await _signInManager.SignOutAsync();
                        await _signInManager.PasswordSignInAsync(user, passwordChangeViewModel.PasswordNew, true, false);
                        ViewBag.AdminSuccess = true;
                    }
                    else
                    {
                        AddModelError(result);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Girmiş olduğunuz parola hatalıdır");
                }
            }
            else
            {
                ModelState.AddModelError("", "Lütfen bilgileri doldurunuz");
            }
          
            return View();
        }
    }
}