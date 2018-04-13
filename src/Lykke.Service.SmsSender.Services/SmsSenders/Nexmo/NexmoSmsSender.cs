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

        public async Task<string> SendSmsAsync(string phone, string message, string countryCode)
        {
            try
            {
                var response = await $"{_settings.BaseUrl}/sms/json"
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
                                Phone = phone.SanitizePhone(),
                                item.Status,
                                item.Error
                            })
                        .ToList();

                    if (errors.Any())
                    {
                        _log.WriteWarning(nameof(SendSmsAsync), errors, "nexmo error messages");
                    }
                
                    return response.Messages.FirstOrDefault(item => item.Status == NexmoStatus.Ok)?.MessageId;
                }
            }
            catch (FlurlHttpException ex)
            {
                _log.WriteWarning(nameof(SendSmsAsync), ex.Message, "nexmo: error sending sms", ex);
            }

            return null;
        }
    }
}
