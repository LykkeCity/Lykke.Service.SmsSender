using Lykke.Service.SmsSender.Core.Domain;

namespace Lykke.Service.SmsSender.Core.Services
{
    public interface ISmsSenderFactory
    {
        ISmsSender GetSender(SmsProvider provider);
    }
}
