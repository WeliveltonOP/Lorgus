﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace Lorgus.UI.Models
{
    public partial class RequestChangePassword
    {
        public string RequestChangePasswordId { get; set; }
        public DateTime? Date { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}