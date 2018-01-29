using Lykke.Service.SmsSender.Core.Domain.SmsRepository;
using ProtoBuf;

namespace Lykke.Service.SmsSender.Sagas.Events
{
    [ProtoContract]
    public class SmsMessageDeliveryFailed
    {
        [ProtoMember(1)]
        public SmsMessage Message { get; set; }
    }
}
