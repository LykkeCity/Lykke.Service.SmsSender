﻿using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.SmsSender.Models
{
    public class SmsModel
    {
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Message { get; set; }
        public string Reason { get; set; }
        public string OuterRequestId { get; set; }
    }
}
