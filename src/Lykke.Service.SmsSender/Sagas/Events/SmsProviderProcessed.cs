using Lykke.Service.SmsSender.Core.Domain;
using ProtoBuf;

namespace Lykke.Service.SmsSender.Sagas.Events
{
    [ProtoContract]
    public class SmsProviderProcessed
    {
        [ProtoMember(1)]
        public string Phone { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }
        [ProtoMember(3)]
        public SmsProvider Provider { get; set; }
        [ProtoMember(4)]
        public string CountryCode { get; set; }
        [ProtoMember(5)]
        public string Id { get; set; }
    }
}
