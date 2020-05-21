using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleIdentity.UI.CustomValidation
{
    public class CustomIdentityErrorDescriber:IdentityErrorDescriber
    {
        public override IdentityError InvalidUserName(string userName)
        {
            return Error("InvalidUserName", $"{userName} kullanıcı adı daha önce alınmıştır");
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return Error("DuplicateEmail", $"{email} adresi daha önce alınmıştır");
        }
        public override IdentityError PasswordTooShort(int length)
        {
            return Error("PasswordTooShort", "Parolar en az 4 karakterden oluşmalıdır.");
        }
        public override IdentityError DuplicateUserName(string userName)
        {
            return Error("DuplicateUserName", $"Kullanıcı adı {userName} kullanılmaktadır!");
        }

        public IdentityError Error(string code,string description)
        {
            return new IdentityError
            {
                Code = code,
                Description = description
            };
        }
    }
}
