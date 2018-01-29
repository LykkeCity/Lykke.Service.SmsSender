using System.Collections.Generic;
using System.Linq;
using Lykke.Service.SmsSender.Core.Domain;
using Lykke.Service.SmsSender.Core.Services;
using Lykke.Service.SmsSender.Services.SmsSenders.Nexmo;
using Lykke.Service.SmsSender.Services.SmsSenders.Twilio;

namespace Lykke.Service.SmsSender.Services
{
    public class SmsSenderFactory : ISmsSenderFactory
    {
        private readonly IEnumerable<ISmsSender> _smsSenders;

        public SmsSenderFactory(IEnumerable<ISmsSender> smsSenders)
        {
            _smsSenders = smsSenders;
        }

        public ISmsSender GetSender(SmsProvider provider)
        {
            switch (provider)
            {
                case SmsProvider.Nexmo:
                    return _smsSenders.FirstOrDefault(item => item is NexmoSmsSender);
                case SmsProvider.Twilio:
                    return _smsSenders.FirstOrDefault(item => item is TwilioSmsSender);
                default:
                    return null;
            }
        }
    }
}
