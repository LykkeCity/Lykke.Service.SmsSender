using ProtoBuf;

namespace Lykke.Service.SmsSender.Sagas.Commands
{
    [ProtoContract]
    public class ProcessSmsCommand
    {
        [ProtoMember(1)]
        public string Phone { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }
        [ProtoMember(3)]
        public string Reason { get; set; }        
        [ProtoMember(4)]
        public string OuterRequestId { get; set; }
    }
}
