using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lorgus.UI.Models
{
    public class LorgusConfig
    {
        public string ContactEmail { get; set; }

        public EmailSettings EmailSettings { get; set; }
    }

    public class EmailSettings
    {
        public string From { get; set; }
        public string DeliveryMethod { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string DisplayName { get; set; }
        public bool EnableSsl { get; set; }
    }
}
