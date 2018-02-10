using Newtonsoft.Json;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Nexmo
{
    public class NexmoMessage
    {
        public string To { get; set; }
        [JsonProperty("message-id")]
        public string MessageId { get; set; }
        public NexmoStatus Status { get; set; }
        [JsonProperty("remaining-balance")]
        public decimal RemainingBalance { get; set; }
        [JsonProperty("message-price")]
        public decimal MessagePrice { get; set; }
        [JsonProperty("error-text")]
        public string Error { get; set; }
    }
}
