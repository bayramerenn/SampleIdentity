using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SampleIdentity.UI.Helper;
using SampleIdentity.UI.Models.AppUser;
using SampleIdentity.UI.ViewModels;

namespace SampleIdentity.UI.Controllers
{
    public class HomeController : BaseController
    {
        public object ModelStateErrorHandler { get; private set; }

        public HomeController(UserManager<AppUser> _userManager, SignInManager<AppUser> _signInManager, IPasswordReset _passwordReset)
          : base(_userManager, _signInManager, _passwordReset)
        { }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Member");
            }
            return View();
        }
        public IActionResult LogIn(string ReturnUrl)
        {
            TempData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(loginViewModel.Email);

                if (user != null)
                {
                    var userLockResult = await _userManager.IsLockedOutAsync(user);
                    if (userLockResult)
                    {
                        ModelState.AddModelError("", "Kullanıcınız bir süreliğine kilitlenmiştir.");
                        return View(loginViewModel);
                    }

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, "Email adresiniz onaylanmamıştır.Lütfen epostanızı kontrol ediniz");
                        return View(loginViewModel);
                    }
                    await _signInManager.SignOutAsync();

                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, loginViewModel.RememberMe, false);

                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);

                        if (TempData["ReturnUrl"] != null)
                        {
                            return Redirect(TempData["ReturnUrl"].ToString());
                        }
                        return RedirectToAction("Index", "Member");
                    }
                    else
                    {
                        await _userManager.AccessFailedAsync(user);

                        int fail = await _userManager.GetAccessFailedCountAsync(user);

                        ModelState.AddModelError(string.Empty, $"{fail} kez başarısız giriş");

                        if (fail == 3)
                        {


                            await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.Now.AddMinutes(1)));
                            ModelState.AddModelError("", "Hesabınız 3 başarısız girişten dolayı 1 dakika süreliğine kilitlenmiştir.Lütfen daha sonra tekrar deneyiniz");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Geçersiz email adresi veya şifresi");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bu email adresine kayıtlı kullanıc bulunamamıştır");
                }
            }
            return View(loginViewModel);
        }
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                if (_userManager.Users.Any(x =>x.PhoneNumber == userViewModel.PhoneNumber))
                {
                    ModelState.AddModelError("", "Telefon numarası sisteme kayıtlıdır.Lütfen başka bir numara giriniz");
                    return View(userViewModel);
                }

                AppUser user = new AppUser
                {
                    UserName = userViewModel.UserName,
                    Email = userViewModel.Email,
                    PhoneNumber = userViewModel.PhoneNumber
                };

                IdentityResult result = await _userManager.CreateAsync(user, userViewModel.Password);
                if (result.Succeeded)
                {
                    string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    string link = Url.Action("ConfirmEmail", "Home", new
                    {
                        userId = user.Id,
                        token = confirmationToken
                    }, protocol: HttpContext.Request.Scheme);

                    var passwordResult = _passwordReset.SendEmail(user.Email, link);
                    if (passwordResult)
                    {
                        ViewBag.Success = true;
                    }
                    return RedirectToAction("LogIn");
                }
                else
                {
                    AddModelError(result);
                }
            }

            return View(userViewModel);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            AppUser user = _userManager.FindByIdAsync(userId).Result;

            await _userManager.ConfirmEmailAsync(user, token);

            return RedirectToAction("Login");
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword([Bind("Email")]PasswordResetViewModel passwordResetViewModel)
        {

            AppUser user = _userManager.FindByEmailAsync(passwordResetViewModel.Email).Result;

            if (user != null)
            {
                string passwordResetToken = _userManager.GeneratePasswordResetTokenAsync(user).Result;

                string passwordResetLink = Url.Action("ResetPasswordConfirm", "Home", new
                {
                    userId = user.Id,
                    token = passwordResetToken
                }, HttpContext.Request.Scheme);

                var result = _passwordReset.PasswordResetSendEmail(user.Email, passwordResetLink);

                if (result)
                {
                    ViewBag.Status = "success";
                }

            }
            else
            {
                ModelState.AddModelError("", "Sistemde kayıtlı mail adresi bulunamamıştır.");
            }
            return View();
        }

        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId ?? "";
            TempData["token"] = token ?? "";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("PasswordNew")]PasswordResetViewModel passwordResetViewModel)
        {

            var token = TempData["token"].ToString();
            var userId = TempData["userID"].ToString();


            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                IdentityResult result = await _userManager.ResetPasswordAsync(user, token.ToString(), passwordResetViewModel.PasswordNew);

                if (result.Succeeded)
                {
                    //Kullanıcı eğer şifre değiştirmişse web siteden çıkıp tekrar giriş yapmasını sağlayan kod
                    await _userManager.UpdateSecurityStampAsync(user);



                    ViewBag.Status = "success";
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Hata meydana gelmiştir.");
            }

            return View();

        }

        public IActionResult FacebookLogin(string ReturnUrl)
        {
            string redirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);

            return new ChallengeResult("Facebook", properties);
        }

        public IActionResult GoogleLogin(string ReturnUrl)
        {
            string redirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            return new ChallengeResult("Google", properties);
        }

        public IActionResult MicrosoftLogin(string ReturnUrl)
        {
            string redirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirectUrl);


            return new ChallengeResult("Microsoft", properties);
        }

        public async Task<IActionResult> ExternalResponse(string ReturnUrl = "/")
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();




            if (info == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                if (result.Succeeded)
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    AppUser user = new AppUser();

                    user.Email = info.Principal.FindFirst(ClaimTypes.Email).Value;

                    string ExternalUserId = info.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;

                    if (info.Principal.HasClaim(x => x.Type == ClaimTypes.Name))
                    {
                        string userName = info.Principal.FindFirst(ClaimTypes.Name).Value;
                        userName = userName.Replace(" ", "-").ToLower() + ExternalUserId.Substring(0, 5).ToString();

                        user.UserName = userName;
                    }
                    else
                    {
                        user.UserName = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    }

                    var userEmail = await _userManager.FindByEmailAsync(user.Email);

                    if (userEmail == null)
                    {
                        var createResult = await _userManager.CreateAsync(user);

                        if (createResult.Succeeded)
                        {
                            var loginResult = await _userManager.AddLoginAsync(user, info);


                            if (loginResult.Succeeded)
                            {
                                //await _signInManager.SignInAsync(user, true);

                                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                Errors = loginResult.Errors.Select(x => x.Description).ToList();
                            }

                        }
                        else
                        {
                            Errors = createResult.Errors.Select(x => x.Description).ToList();
                        }
                    }
                    else
                    {
                        var loginResult = await _userManager.AddLoginAsync(userEmail, info);

                        await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                        return Redirect(ReturnUrl);
                    }


                }
            }



            return View("Error", Errors);
        }

        public IActionResult Error()
        {
            return View();
        }
        public IActionResult Policy()
        {
            return View();
        }
    }
}