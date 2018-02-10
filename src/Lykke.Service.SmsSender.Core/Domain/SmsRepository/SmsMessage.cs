using System;
using Autofac;
using ProtoBuf;

namespace Lykke.Service.SmsSender.Core.Domain.SmsRepository
{
    [ProtoContract]
    public class SmsMessage : ISmsMessage
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string MessageId { get; set; }
        [ProtoMember(3)]
        public string Phone { get; set; }
        [ProtoMember(4)]
        public string Message { get; set; }
        [ProtoMember(5)]
        public SmsProvider Provider { get; set; }
        [ProtoMember(6)]
        public string CountryCode { get; set; }
        [ProtoMember(7)]
        public DateTime Created { get; set; }

        public bool IsExpired(TimeSpan period) => (DateTime.UtcNow - Created).TotalMilliseconds > period.TotalMilliseconds;

        public static SmsMessage Create(ISmsMessage src)
        {
            if (src == null)
                return null;
            
            return new SmsMessage
            {
                Id = src.Id,
                MessageId = src.MessageId,
                Phone = src.Phone,
                Message = src.Message,
                Provider = src.Provider,
                CountryCode = src.CountryCode,
                Created = src.Created
            };
        }
    }
}
