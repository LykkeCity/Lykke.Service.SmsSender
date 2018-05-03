using System;
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

            var phone = command.Phone.GetValidPhone();
            
            if (phone != null)
            {
                var phoneUtils = PhoneNumberUtil.GetInstance();
                string countryCode = phoneUtils.GetRegionCodeForCountryCode(phone.CountryCode);
                var provider = await _settingsService.GetProviderByCountryAsync(countryCode);

                string id = await _smsRepository.AddAsync(new SmsMessage
                {
                    CountryCode = countryCode,
                    Message = command.Message,
                    Phone = command.Phone,
                    Provider = provider
                });

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
                string messageId = await sender.SendSmsAsync(command.Phone, command.Message, command.CountryCode);

                if (!string.IsNullOrEmpty(messageId))
                {
                    await _smsRepository.SetMessageIdAsync(messageId, command.Id);
                    _log.WriteInfo(nameof(SendSmsCommand), new { command.Id, MessageId = messageId}, "Message has been sent");
                }
                else
                {
                    await _smsRepository.DeleteAsync(command.Id, messageId);
                    _log.WriteInfo(nameof(SendSmsCommand), new { command.Id }, "Sms message has been deleted");
                }
            }
            catch (Exception)
            {
                await _smsProviderInfoRepository.AddAsync(command.Provider, command.CountryCode, SmsDeliveryStatus.Failed);
                return CommandHandlingResult.Fail(TimeSpan.FromSeconds(5));
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
