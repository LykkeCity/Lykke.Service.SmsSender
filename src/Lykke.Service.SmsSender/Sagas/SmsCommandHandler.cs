using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Service.SmsSender.Core.Domain;
using Lykke.Service.SmsSender.Core.Domain.SmsProviderInfoRepository;
using Lykke.Service.SmsSender.Core.Domain.SmsRepository;
using Lykke.Service.SmsSender.Core.Services;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings;
using Lykke.Service.SmsSender.Extensions;
using Lykke.Service.SmsSender.Sagas.Commands;
using Lykke.Service.SmsSender.Sagas.Events;
using PhoneNumbers;

namespace Lykke.Service.SmsSender.Sagas
{
    public class SmsCommandHandler
    {
        private readonly SmsSettings _smsSettings;
        private readonly ISettingsService _settingsService;
        private readonly ISmsSenderFactory _smsSenderFactory;
        private readonly ISmsRepository _smsRepository;
        private readonly ISmsProviderInfoRepository _smsProviderInfoRepository;
        private readonly ILog _log;

        public SmsCommandHandler(
            SmsSettings smsSettings,
            ISettingsService settingsService,
            ISmsSenderFactory smsSenderFactory,
            ISmsRepository smsRepository,
            ISmsProviderInfoRepository smsProviderInfoRepository,
            ILog log)
        {
            _smsSettings = smsSettings;
            _settingsService = settingsService;
            _smsSenderFactory = smsSenderFactory;
            _smsRepository = smsRepository;
            _smsProviderInfoRepository = smsProviderInfoRepository;
            _log = log.CreateComponentScope(nameof(SmsCommandHandler));
        }

        public async Task<CommandHandlingResult> Handle(ProcessSmsCommand command, IEventPublisher eventPublisher)
        {
            _log.WriteInfo(nameof(ProcessSmsCommand), new { Phone = command.Phone.SanitizePhone() }, "Processing sms");

            var phone = command.Phone.GetValidPhone(_log);

            if (phone != null)
            {
                var phoneUtils = PhoneNumberUtil.GetInstance();
                var countryCode = phoneUtils.GetRegionCodeForCountryCode(phone.CountryCode);

                if (_smsSettings.AllowedCountries.Any() && !_smsSettings.AllowedCountries.Contains(countryCode))
                {
                    _log.WriteWarning(nameof(ProcessSmsCommand),
                        new { CountryCode = countryCode },
                        $"Country {countryCode} is not allowed in the settings. SMS sending to the phone {command.Phone.SanitizePhone()} will be aborted");

                    return CommandHandlingResult.Ok();
                }

                if (_smsSettings.BlockedCountries.Contains(countryCode))
                {
                    _log.WriteWarning(nameof(ProcessSmsCommand),
                        new { CountryCode = countryCode },
                        $"Country {countryCode} is blocked in the settings. SMS sending to the phone {command.Phone.SanitizePhone()} will be aborted");

                    return CommandHandlingResult.Ok();
                }

                var provider = await _settingsService.GetProviderByCountryAsync(countryCode);

                var id = await _smsRepository.AddAsync(new SmsMessage
                {
                    CountryCode = countryCode,
                    Message = command.Message,
                    Phone = command.Phone,
                    Provider = provider
                });

                _log.WriteInfo(nameof(ProcessSmsCommand), 
                    new { Id = id, CountryCode = countryCode, Provider = provider.GetType().Name }, 
                    "Country code and provider has been determined for the SMS");

                eventPublisher.PublishEvent(new SmsProviderProcessed
                {
                    Phone = command.Phone,
                    Message = command.Message,
                    Provider = provider,
                    CountryCode = countryCode,
                    Id = id
                });
            }

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(SendSmsCommand command, IEventPublisher eventPublisher)
        {
            var message = await _smsRepository.GetAsync(command.Id);

            var msg = new
            {
                Phone = command.Phone.SanitizePhone(),
                command.Id,
                command.Provider,
                command.CountryCode
            };

            if (message == null)
            {
                _log.WriteInfo(nameof(SendSmsCommand), msg, $"Sms message with messageId = {command.Id} not found");
                return CommandHandlingResult.Ok();
            }

            if (message.IsExpired(_smsSettings.SmsRetryTimeout))
            {
                await _smsRepository.DeleteAsync(message.Id, message.MessageId);
                _log.WriteInfo(nameof(SendSmsCommand), msg, "Sms message expired and has been deleted");
                return CommandHandlingResult.Ok();
            }

            var sender = _smsSenderFactory.GetSender(command.Provider);

            _log.WriteInfo(nameof(SendSmsCommand), msg, "Sending sms");

            try
            {
                string messageId = await sender.SendSmsAsync(command.Id, command.Phone, command.Message, command.CountryCode);

                if (!string.IsNullOrEmpty(messageId))
                {
                    await _smsRepository.SetMessageIdAsync(messageId, command.Id);
                    _log.WriteInfo(nameof(SendSmsCommand), new { command.Id, MessageId = messageId}, "Message has been sent");
                }
                else
                {
                    await _smsRepository.DeleteAsync(command.Id, messageId);
                    _log.WriteInfo(nameof(SendSmsCommand), new { command.Id }, "Sms message has been deleted");
                    await _smsProviderInfoRepository.AddAsync(command.Provider, command.CountryCode, SmsDeliveryStatus.Failed);
                }
            }
            catch (Exception e)
            {
                _log.WriteWarning(nameof(SendSmsCommand), new { command.Id }, $"Failed to send sms. It will be retried in {_smsSettings.SmsSendDelay}", e);
                await _smsProviderInfoRepository.AddAsync(command.Provider, command.CountryCode, SmsDeliveryStatus.Failed);

                return CommandHandlingResult.Fail(_smsSettings.SmsSendDelay);
            }

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(SmsDeliveredCommand command, IEventPublisher eventPublisher)
        {
            _log.WriteInfo(nameof(SmsDeliveredCommand), new { Phone = command.Message.Phone.SanitizePhone(),  command.Message.Id,
                command.Message.MessageId, command.Message.Provider, command.Message.CountryCode }, "Sms delivered");
            await _smsProviderInfoRepository.AddAsync(command.Message.Provider, command.Message.CountryCode, SmsDeliveryStatus.Delivered);
            await _smsRepository.DeleteAsync(command.Message.Id, command.Message.MessageId);

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(SmsNotDeliveredCommand command, IEventPublisher eventPublisher)
        {
            _log.WriteWarning(nameof(SmsNotDeliveredCommand), new { Phone = command.Message.Phone.SanitizePhone(), command.Message.Id, command.Message.MessageId,
                command.Message.Provider, command.Message.CountryCode }, $"Sms delivery failed: {command.Error}");

            if (command.Message.IsExpired(_smsSettings.SmsRetryTimeout))
            {
                await _smsProviderInfoRepository.AddAsync(command.Message.Provider, command.Message.CountryCode, SmsDeliveryStatus.Failed);
                await _smsRepository.DeleteAsync(command.Message.Id, command.Message.MessageId);
            }
            else
            {
                eventPublisher.PublishEvent(new SmsMessageDeliveryFailed{ Message = command.Message});
            }

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(SmsDeliveryUnknownCommand command, IEventPublisher eventPublisher)
        {
            _log.WriteWarning(nameof(SmsDeliveryUnknownCommand), new { Phone = command.Message.Phone.SanitizePhone(), command.Message.Id, command.Message.MessageId,
                command.Message.Provider, command.Message.CountryCode }, $"Sms delivery unknown: {command.Error}");

            await _smsProviderInfoRepository.AddAsync(command.Message.Provider, command.Message.CountryCode, SmsDeliveryStatus.Unknown);
            await _smsRepository.DeleteAsync(command.Message.Id, command.Message.MessageId);

            return CommandHandlingResult.Ok();
        }
    }
}
