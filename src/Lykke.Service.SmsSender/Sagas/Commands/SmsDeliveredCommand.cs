using Lykke.Service.SmsSender.Core.Domain.SmsRepository;
using ProtoBuf;

namespace Lykke.Service.SmsSender.Sagas.Commands
{
    [ProtoContract]
    public class SmsDeliveredCommand
    {
        [ProtoMember(1)]
        public SmsMessage Message { get; set; }
    }
}
