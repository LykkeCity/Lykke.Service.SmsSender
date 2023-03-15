using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Flurl.Http;
using Lykke.Service.SmsSender.Core.Services;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings.SenderSettings;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Nexmo
{
    public class NexmoSmsSender : ISmsSender
    {
        private readonly string _baseUrl;
        private readonly ProviderSettings _settings;
        private readonly ILog _log;

        public NexmoSmsSender(
            string baseUrl,
            ProviderSettings settings,
            ILog log)
        {
            _baseUrl = baseUrl;
            _settings = settings;
            _log = log.CreateComponentScope(nameof(NexmoSmsSender));
        }

        public async Task<string> SendSmsAsync(string commandId, string phone, string message, string countryCode)
        {
            int index = 0;
            while (++index <= 3)
            {
                try
                {
                    var sw = new Stopwatch();
                    _log.WriteInfo(nameof(SendSmsAsync), new {Id = commandId, Phone = phone.SanitizePhone(), CountryCode = countryCode},
                        $"Sending sms to nexmo endpoint {_settings.BaseUrl}/sms/json");

                    sw.Start();
                    var response = await $"{_settings.BaseUrl}/sms/json"
                        .WithTimeout(10)
                        .PostUrlEncodedAsync(new
                        {
                            to = phone,
                            from = _settings.GetFrom(countryCode),
                            text = message,
                            api_key = _settings.ApiKey,
                            api_secret = _settings.ApiSecret,
                            callback = $"{_baseUrl}/callback/nexmo"
                        }).ReceiveJson<NexmoResponse>();

                    sw.Stop();

                    _log.WriteInfo(nameof(SendSmsAsync), new {Id = commandId, messagesCount = response.MessagesCount, ElapsedMsec = sw.ElapsedMilliseconds },
                        $"Sms has been sent to nexmo endpoint {_settings.BaseUrl}/sms/json");

                    if (response.MessagesCount > 0)
                    {
                        var errors = response.Messages
                            .Where(item => item.Status != NexmoStatus.Ok)
                            .Select(item => new
                            {
                                Phone = phone.SanitizePhone(), item.Status, item.Error, item.RemainingBalance
                            })
                            .ToList();

                        if (errors.Any())
                        {
                            var notEnoughFunds =
                                errors.FirstOrDefault(item => item.Status == NexmoStatus.PartnerQuotaExceeded);

                            if (notEnoughFunds != null)
                                _log.WriteWarning(nameof(SendSmsAsync), new { Id = commandId, Error = notEnoughFunds }, "Not enough funds on Nexmo provider");
                            else
                                _log.WriteWarning(nameof(SendSmsAsync), new { Id = commandId, Errors = errors }, "Error sending SMS");
                        }

                        return response.Messages.FirstOrDefault(item => item.Status == NexmoStatus.Ok)?.MessageId;
                    }
                    else
                    {
                        _log.WriteWarning(nameof(SendSmsAsync), new { Id = commandId }, "Unexpected messages count in Nexmo response");
                    }
                }
                catch (Exception ex)
                {
                    _log.WriteWarning(nameof(SendSmsAsync), new { Id = commandId }, "Error sending SMS", ex);
                }

                _log.WriteWarning(nameof(SendSmsAsync), new { Id = commandId }, "Failed to send SMS via Nexmo. Will be retried in 1 second");

                await Task.Delay(1000);
            }

            _log.WriteWarning(nameof(SendSmsAsync), new { Id = commandId }, "First-level retries of SMS sendind are exhausted");

            return null;
        }
    }
}
