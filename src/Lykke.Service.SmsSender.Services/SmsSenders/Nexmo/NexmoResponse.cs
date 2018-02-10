using Newtonsoft.Json;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Nexmo
{
    public class NexmoResponse
    {
        [JsonProperty("message-count")]
        public int MessagesCount { get; set; }
        public NexmoMessage[] Messages { get; set; }
    }
}
