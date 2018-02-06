using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.SmsSender.Models
{
    public class TwilioCallbackModel
    {
        public string MessageSid { get; set; }
        public string AccountSid { get; set; }
        public string MessagingServiceSid { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TwilioMessageStatus MessageStatus { get; set; }
    }

    public enum TwilioMessageStatus
    {
        Accepted,
        Queued,
        Sending,
        Sent,
        Receiving,
        Received,
        Delivered,
        Undelivered,
        Failed
    }
}
