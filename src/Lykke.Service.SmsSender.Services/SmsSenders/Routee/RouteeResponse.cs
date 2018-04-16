using System;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Routee
{
    public class RouteeResponse
    {
        public string TrackingId { get; set; }
        public DateTime CreateAt { get; set; }
        public RouteeStaus Status { get; set; }
    }
}
