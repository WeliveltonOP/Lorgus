using System;
using System.ComponentModel.DataAnnotations;

namespace Lorgus.UI.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public String Email { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public String Password { get; set; }
    }
}
