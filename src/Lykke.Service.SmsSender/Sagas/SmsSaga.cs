﻿using System.Threading.Tasks;
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
            _log.WriteInfo(
                nameof(SmsProviderProcessed),
                new
                {
                    Phone = evt.Phone.SanitizePhone(),
                    evt.Id,
                    evt.Provider,
                    evt.CountryCode,
                    evt.Reason,
                    evt.OuterRequestId
                },
                "Sms processed");

            var sendSmsCommand = new SendSmsCommand
            {
                Phone = evt.Phone,
                Message = evt.Message,
                Provider = evt.Provider,
                CountryCode = evt.CountryCode,
                Id = evt.Id,
                Reason = evt.Reason,
                OuterRequestId = evt.OuterRequestId
            };

            commandSender.SendCommand(sendSmsCommand, "sms");
        }
        
        public async Task Handle(SmsMessageDeliveryFailed evt, ICommandSender commandSender)
        {
            _log.WriteInfo(nameof(SmsMessageDeliveryFailed), new
            {
                Phone = evt.Message.Phone.SanitizePhone(), evt.Message.Id, evt.Message.Provider, evt.Message.CountryCode
            }, $"Retrying sms delivery. Sending using {evt.Message.Provider}, country code: {evt.Message.CountryCode}");

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
