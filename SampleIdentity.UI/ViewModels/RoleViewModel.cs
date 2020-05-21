using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SampleIdentity.UI.ViewModels
{
    public class RoleViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "Role ismi")]
        [Required(ErrorMessage = "Role alanı gereklidir")]
        public string Name { get; set; }
    }
}
