using System;
using Lykke.Service.SmsSender.Services.SmsSenders.Routee;

namespace Lykke.Service.SmsSender.Models
{
    public class RouteeCallbackModel
    {
        public string MessageId { get; set; }
        public string SmsId { get; set; }
        public string CampaignTrackingId { get; set; }
        public int Part { get; set; }
        public int Parts { get; set; }
        public string Label { get; set; }
        public string Country { get; set; }
        public string Operator { get; set; }
        public RouteeStatusModel Status { get; set; }
        public string Message { get; set; }
        public string ApplicationName { get; set; }
        public int Latency { get; set; }
        public decimal Price { get; set; }
        public string Direction { get; set; }
        public string OriginatingService { get; set; }
    }

    public class RouteeStatusModel
    {
        public RouteeStaus Name { get; set; }
        public DateTime UpdatedDate { get; set; }
        public RouteeReason Reason { get; set; }
    }

    public class RouteeReason
    {
        public string DetailedStatus { get; set; }
        public string Description { get; set; }
    }
}
