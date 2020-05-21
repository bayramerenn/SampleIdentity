using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SampleIdentity.UI.Enums;
using SampleIdentity.UI.Helper;
using SampleIdentity.UI.Middleware;
using SampleIdentity.UI.Models.AppUser;
using SampleIdentity.UI.ViewModels;

namespace SampleIdentity.UI.Controllers
{

    public class MemberController : BaseController
    {

        public MemberController(UserManager<AppUser> _userManager, SignInManager<AppUser> _signInManager, IPasswordReset _passwordReset)
            : base(_userManager, _signInManager, _passwordReset)
        { }


        public IActionResult Index()
        {
            List<AppUser> appUsers = new List<AppUser>();

            AppUser user = CurrentUser;
            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel);
        }

        public IActionResult UserEdit()
        {
            AppUser user = CurrentUser;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();
            ViewBag.Gender = new SelectList(Enum.GetValues(typeof(Gender)));

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel userViewModel, IFormFile userPicture)
        {
            ViewBag.Gender = new SelectList(Enum.GetValues(typeof(Gender)));
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                //AppUser user = userViewModel.Adapt<AppUser>();

                AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user.PhoneNumber != userViewModel.PhoneNumber)
                {
                    if (_userManager.Users.Any(x => x.PhoneNumber == userViewModel.PhoneNumber))
                    {
                        ModelState.AddModelError("", "Sisteme kayıtlı bir telefon girdiniz");
                        return View(userViewModel);
                    }
                }


                if (userPicture != null && userPicture.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + userPicture.FileName;

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);

                        user.Picture = "/UserPicture/" + fileName;

                    }
                }

                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;
                user.City = userViewModel.City;
                user.BirthDay = userViewModel.BirthDay;
                user.Gender = (int)userViewModel.Gender;
                IdentityResult result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user, true);
                    ViewBag.success = true;
                }
                else
                {
                    AddModelError(result);
                }
            }

            return View(userViewModel);
        }

        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PasswordChange(PasswordChangeViewModel passwordChangeViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;


                bool passwordExist = _userManager.CheckPasswordAsync(user, passwordChangeViewModel.PasswordOld).Result;
                if (passwordExist)
                {
                    IdentityResult result = _userManager.ChangePasswordAsync
                            (user, passwordChangeViewModel.PasswordOld, passwordChangeViewModel.PasswordNew).Result;

                    if (result.Succeeded)
                    {
                        _userManager.UpdateSecurityStampAsync(user);
                        _signInManager.SignOutAsync();
                        _signInManager.PasswordSignInAsync(user, passwordChangeViewModel.PasswordNew, true, false);
                        ViewBag.success = true;
                    }
                    else
                    {
                        AddModelError(result);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Girmiş olduğunuz eski şifreniz hatalıdır");
                }
            }


            return View(passwordChangeViewModel);
        }

        public void Logout()
        {
            _signInManager.SignOutAsync();
        }

        public IActionResult AccessDenied(string ReturnUrl)
        {
            if (ReturnUrl.Contains("ViolencePage"))
            {
                ViewBag.Message = "Bu sayfada şiddet içeren videolar olduğundan dolayı 15 yaşından küçüklerin girmesi yasaktır";
            }
            else if (ReturnUrl.Contains("GiresunPage"))
            {
                ViewBag.Message = "Giresunlular sayfasına erişim izniniz yoktur";
            }
            else if(ReturnUrl.Contains("Exchange"))
            {
                ViewBag.Message = "30 günlük giriş süreniz dolmuştur";
            }
            else
            {
                ViewBag.Message = "Bu sayfaya erişim izniniz yoktur.Erişim izni almanız için site yönteciniz ile görüşünüz";
            }
            return View();
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Editor()
        {
            return View();
        }

        [Authorize(Roles = "Manager,Admin")]
        public IActionResult Manager()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }
        [Authorize(Policy = "GiresunPolicy")]
        public IActionResult GiresunPage()
        {
            return View();
        }

        [Authorize(Policy = "ViolencePolicy")]
        public IActionResult ViolencePage()
        {
            return View();
        }

        public async Task<IActionResult> ExchangeRedirect()
        {
            bool result = User.HasClaim(x => x.Type == "ExpireDateExchange");

            if (!result)
            {
                Claim ExpireDateExchange = new Claim("ExpireDateExchange", DateTime.Now.AddDays(30).Date.ToShortDateString(), ClaimValueTypes.String, "Internal");

                await _userManager.AddClaimAsync(CurrentUser, ExpireDateExchange);

                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(CurrentUser, true);


            }

            return RedirectToAction("Exchange");
        }

        [Authorize(Policy = "ExchangePolicy")]
        public IActionResult Exchange()
        {
            return View();
        }
    }
}