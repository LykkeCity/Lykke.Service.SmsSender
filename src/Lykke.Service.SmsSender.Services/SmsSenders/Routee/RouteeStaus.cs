using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Routee
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RouteeStaus
    {
        Queued,
        Sent,
        Delivered,
        Undelivered,
        Failed,
        Unsent
    }
}
