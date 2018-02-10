using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.SmsSender.Models
{
    public class SmsModel
    {
        [Required]
        [RegularExpression("^\\+\\d+", ErrorMessage = "Wrong phone format")]
        public string Phone { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
