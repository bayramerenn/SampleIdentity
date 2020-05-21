using Microsoft.AspNetCore.Mvc.Rendering;
using SampleIdentity.UI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SampleIdentity.UI.ViewModels
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adınızı giriniz")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        [Display(Name = "Tel No")]
        [RegularExpression(@"^(05(\d{9}))$",ErrorMessage ="Telefon numarası uygun formatta değil")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email adresinizi giriniz")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email adresiniz doğru formatta değil")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifrenizi giriniz")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Şehir")]
        public string City { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDay { get; set; }

        [Display(Name = "Cinsiyet")]
        public Gender Gender { get; set; }
        public string Picture { get; set; }

    }
}
