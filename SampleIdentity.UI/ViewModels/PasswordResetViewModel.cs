﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

using SampleIdentity.UI.Models.AppUser;
using SampleIdentity.UI.ViewModels;

namespace SampleIdentity.UI.ViewModels
{
    public class PasswordResetViewModel
    {
        [Display(Name = "Email adresiniz")]
        [Required(ErrorMessage = "Email alanı gereklidir")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Yeni şifreniz")]
        [Required(ErrorMessage = "Şifre alanı gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4,ErrorMessage = "Şifreniz en az 4 karakterli olmalıdır")]
        public string PasswordNew { get; set; }



    }
}
