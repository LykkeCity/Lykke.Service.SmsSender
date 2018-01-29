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
            _log = log;
        }

        public async Task<CommandHandlingResult> Handle(ProcessSmsCommand processSmsCommand, IEventPublisher eventPublisher)
        {
            _log.WriteInfo(nameof(ProcessSmsCommand), new { Phone = processSmsCommand.Phone.SanitizePhone() }, "Processing sms");

            var phoneUtils = PhoneNumberUtil.GetInstance();

            var phone = phoneUtils.Parse(processSmsCommand.Phone, null);

            if (phone != null && phoneUtils.IsValidNumber(phone))
            {
                string countryCode = phoneUtils.GetRegionCodeForCountryCode(phone.CountryCode);
                var provider = await _settingsService.GetProviderByCountryAsync(countryCode);

                string id = await _smsRepository.AddAsync(new SmsMessage
                {
                    CountryCode = countryCode,
                    Message = processSmsCommand.Message,
                    Phone = processSmsCommand.Phone,
                    Provider = provider
                });

                eventPublisher.PublishEvent(new SmsProviderProcessed
                {
                    Phone = processSmsCommand.Phone,
                    Message = processSmsCommand.Message,
                    Provider = provider,
                    CountryCode = countryCode,
                    Id = id
                });
            }

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(SendSmsCommand sendSmsCommand, IEventPublisher eventPublisher)
        {
            var message = await _smsRepository.GetAsync(sendSmsCommand.Id);

            if (message.IsExpired(_smsSettings.SmsRetryTimeout))
            {
                await _smsRepository.DeleteAsync(message.Id, message.MessageId);
                return CommandHandlingResult.Ok();
            }
            
            var sender = _smsSenderFactory.GetSender(sendSmsCommand.Provider);

            _log.WriteInfo(nameof(SendSmsCommand), new { Phone = sendSmsCommand.Phone.SanitizePhone(), sendSmsCommand.Id, sendSmsCommand.Provider, sendSmsCommand.CountryCode }, "Sending sms");
            
            try
            {
                string messageId = await sender.SendSmsAsync(sendSmsCommand.Phone, sendSmsCommand.Message, sendSmsCommand.CountryCode);

                if (!string.IsNullOrEmpty(messageId))
                {
                    await _smsRepository.SetMessageIdAsync(messageId, sendSmsCommand.Id);
                }
            }
            catch (Exception)
            {
                await _smsProviderInfoRepository.AddAsync(sendSmsCommand.Provider, sendSmsCommand.CountryCode, SmsDeliveryStatus.Failed);
                return CommandHandlingResult.Fail(TimeSpan.FromSeconds(5));
            }

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(SmsDeliveredCommand smsDeliveredCommand, IEventPublisher eventPublisher)
        {
            _log.WriteInfo(nameof(SmsDeliveredCommand), new { Phone = smsDeliveredCommand.Message.Phone.SanitizePhone(),  smsDeliveredCommand.Message.Id,
                smsDeliveredCommand.Message.MessageId, smsDeliveredCommand.Message.Provider, smsDeliveredCommand.Message.CountryCode }, "Sms delivered");
            await _smsProviderInfoRepository.AddAsync(smsDeliveredCommand.Message.Provider, smsDeliveredCommand.Message.CountryCode, SmsDeliveryStatus.Delivered);
            await _smsRepository.DeleteAsync(smsDeliveredCommand.Message.Id, smsDeliveredCommand.Message.MessageId);
            
            return CommandHandlingResult.Ok();
        }
        
        public async Task<CommandHandlingResult> Handle(SmsNotDeliveredCommand command, IEventPublisher eventPublisher)
        {
            _log.WriteInfo(nameof(SmsNotDeliveredCommand), new { Phone = command.Message.Phone.SanitizePhone(), command.Message.Id, command.Message.MessageId,
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
    }
}
