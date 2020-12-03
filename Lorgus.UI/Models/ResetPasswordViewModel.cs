using System;

namespace Lorgus.UI.Models
{
    public class ResetPasswordViewModel
    {
        public String RequestChangePasswordId { get; set; }
        public DateTime? Date { get; set; }
        public int? UserId { get; set; }
        public string Password1 { get; set; }
        public string Password2 { get; set; }

    }
}
