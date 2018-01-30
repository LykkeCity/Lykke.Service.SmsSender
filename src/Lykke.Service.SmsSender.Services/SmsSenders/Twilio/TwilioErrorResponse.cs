using Newtonsoft.Json;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Twilio
{
    public class TwilioErrorResponse
    {
        public string Status { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        [JsonProperty("more_info")]
        public string MoreInfo { get; set; }
    }
}
