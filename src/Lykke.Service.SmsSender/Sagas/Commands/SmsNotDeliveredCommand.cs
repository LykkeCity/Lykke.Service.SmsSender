using Lykke.Service.SmsSender.Core.Domain.SmsRepository;
using ProtoBuf;

namespace Lykke.Service.SmsSender.Sagas.Commands
{
    [ProtoContract]
    public class SmsNotDeliveredCommand
    {
        [ProtoMember(1)]
        public SmsMessage Message { get; set; }
        [ProtoMember(2)]
        public string Error { get; set; }
    }
}
