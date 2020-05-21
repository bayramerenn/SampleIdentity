using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SampleIdentity.UI.Models.AppUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleIdentity.UI.ClaimProvider
{
    public class ClaimProvider : IClaimsTransformation
    {
        private UserManager<AppUser> _userManager;

        public ClaimProvider(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            //login olup olmadığını kontrol ediliyor
            if (principal != null && principal.Identity.IsAuthenticated)
            {
                //login olduktan sonra alınan değerleri ClaimsIdentiy aktarılıyor 
                //Çünki bu sınıf sayesinde tekrar cookiye bilgi ekliyoruz
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity;

                AppUser user = await _userManager.FindByNameAsync(identity.Name);

                if (user != null)
                {
                    if (user.City != null)
                    {
                        //Böyle bir claim yoksa kullanıcı login iken şehirde değişiklik yapsa 
                        //if bloğu çalışmaz çıkıp girmesi gerekecek
                        if (!principal.HasClaim(c => c.Type == "city"))
                        {
                            //issuer(yayınlamayı) internal yani içerden yaptık
                            Claim CityClaim = new Claim("city", user.City, ClaimValueTypes.String, "Internal");
                            identity.AddClaim(CityClaim);
                        }
                    }

                    if (user.BirthDay != null)
                    {
                        var today = DateTime.Today;
                        var age = today.Year - user.BirthDay?.Year;

                        if (age > 15)
                        {
                            Claim ViolenceClaim = new Claim("violence", true.ToString(), ClaimValueTypes.String, "Internal");
                            identity.AddClaim(ViolenceClaim);
                        }

                    }

                }
            }
            return principal;
        }
    }
}
