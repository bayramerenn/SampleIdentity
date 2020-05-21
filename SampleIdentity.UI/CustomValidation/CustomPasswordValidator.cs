using Microsoft.AspNetCore.Identity;
using SampleIdentity.UI.Models.AppUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleIdentity.UI.CustomValidation
{
    public class CustomPasswordValidator : IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();
            if (password.ToLower().Contains(user.UserName.ToLower()) && !password.ToLower().Contains("@"))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsUserName",
                    Description = "Şifre alanı kullanıcı adı içeremez"
                });
            }
            if (password.ToLower().Contains(user.Email))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsEmail",
                    Description = "Şifre alanı kayıtlı mail adresiniz içeremez"
                });
            }
            if (password.ToLower().Contains("1234"))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordContains1234",
                    Description = "Şifre alanı ardışık sayı içeremez"
                });
            }
            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

        }
    }
}
