﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.SmsSender.Models
{
    public class NexmoCallbackModel
    {
        public string MessageId { get; set; }
        public NexmoMessageStatus Status { get; set; }
        [JsonProperty("err-code")]
        public NexmoErrorCode ErrorCode { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NexmoMessageStatus
    {
        Delivered,
        Expired,
        Failed,
        Rejected,
        Accepted,
        Buffered,
        Unknown
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NexmoErrorCode
    {
        Delivered = 0,
        Unknown = 1,
        AbsentSubscriberTemporary = 2,
        AbsentSubscriberPermanent = 3,
        CallBarredByUser = 4,
        PortabilityError = 5,
        AntiSpamRejection = 6,
        HandsetBusy = 7,
        NetworkError = 8,
        IllegalNumber = 9,
        InvalidMessage = 10,
        Unroutable = 11,
        DestinationUnreachable = 12,
        SubscriberAgeRestriction = 13,
        NumberBlockedByCarrier = 14,
        PrePaid = 15,
        GeneralError = 99
    }
}
