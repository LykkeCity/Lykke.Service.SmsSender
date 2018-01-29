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
        private const string BaseUrl = "https://rest.nexmo.com";
        
        public NexmoSmsSender(
            string baseUrl,
            ProviderSettings settings,
            ILog log)
        {
            _baseUrl = baseUrl;
            _settings = settings;
            _log = log;
        }

        public async Task<string> SendSmsAsync(string phone, string message, string countryCode)
        {
            var response = await $"{BaseUrl}/sms/json"
                .PostUrlEncodedAsync(new
                {
                    to = phone,
                    from = _settings.GetFrom(countryCode),
                    text = message,
                    api_key = _settings.ApiKey,
                    api_secret = _settings.ApiSecret,
                    callback = $"{_baseUrl}/callback/nexmo"
                }).ReceiveJson<NexmoResponse>();

            if (response.MessagesCount > 0)
            {
                var errors = response.Messages
                    .Where(item => item.Status != NexmoStatus.Ok)
                    .Select(item => new
                        {
                            Phone = item.To.SanitizePhone(),
                            item.MessagePrice,
                            item.RemainingBalance,
                            item.Status
                        })
                    .ToList();

                if (errors.Any())
                {
                    _log.WriteWarning(nameof(SendSmsAsync), errors, "nexmo error messages");
                }
                
                return response.Messages.FirstOrDefault(item => item.Status == NexmoStatus.Ok)?.MessageId;
            }

            return null;
        }
    }
}
