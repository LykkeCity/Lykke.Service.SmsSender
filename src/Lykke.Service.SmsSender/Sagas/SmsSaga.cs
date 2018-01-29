using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Service.SmsSender.Sagas.Commands;
using Lykke.Service.SmsSender.Sagas.Events;

namespace Lykke.Service.SmsSender.Sagas
{
    public class SmsSaga
    {
        private readonly ILog _log;

        public SmsSaga(ILog log)
        {
            _log = log;
        }
        
        public async Task Handle(SmsProviderProcessed evt, ICommandSender commandSender)
        {
            await _log.WriteInfoAsync(nameof(SmsSaga), nameof(SmsProviderProcessed), $"{evt.Phone.SanitizePhone()}", $"Sms processed. Sending using {evt.Provider.ToString()}, country code: {evt.CountryCode}");

            var sendSmsCommand = new SendSmsCommand
            {
                Phone = evt.Phone,
                Message = evt.Message,
                Provider = evt.Provider,
                CountryCode = evt.CountryCode,
                Id = evt.Id
            };

            commandSender.SendCommand(sendSmsCommand, "sms");
        }
        
        public async Task Handle(SmsMessageDeliveryFailed evt, ICommandSender commandSender)
        {
            await _log.WriteInfoAsync(nameof(SmsSaga), nameof(SmsMessageDeliveryFailed), 
                $"{evt.Message.Phone.SanitizePhone()}", $"Retrying sms delivery. Sending using {evt.Message.Provider.ToString()}, country code: {evt.Message.CountryCode}");

            var sendSmsCommand = new SendSmsCommand
            {
                Phone = evt.Message.Phone,
                Message = evt.Message.Message,
                Provider = evt.Message.Provider,
                CountryCode = evt.Message.CountryCode,
                Id = evt.Message.Id
            };

            commandSender.SendCommand(sendSmsCommand, "sms");
        }
    }
}
