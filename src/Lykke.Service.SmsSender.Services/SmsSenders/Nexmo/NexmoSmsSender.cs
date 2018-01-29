using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Lykke.Service.SmsSender.Core.Services;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings.SenderSettings;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Nexmo
{
    public class NexmoSmsSender : ISmsSender
    {
        private readonly string _baseUrl;
        private readonly ProviderSettings _settings;
        private const string BaseUrl = "https://rest.nexmo.com";
        
        public NexmoSmsSender(
            string baseUrl,
            ProviderSettings settings)
        {
            _baseUrl = baseUrl;
            _settings = settings;
        }

        public async Task<string> SendSmsAsync(string phone, string message)
        {
            return "test";
            var response = await $"{BaseUrl}/sms/json"
                .PostUrlEncodedAsync(new
                {
                    to = phone,
                    from = _settings.From,
                    text = message,
                    api_key = _settings.ApiKey,
                    api_secret = _settings.ApiSecret,
                    callback = $"{_baseUrl}/callback/nexmo",
                    
                }).ReceiveJson<NexmoResponse>();

            if (response.MessagesCount > 0)
                return response.Messages.FirstOrDefault(item => item.Status == NexmoStatus.Ok)?.MessageId;

            return null;
        }
    }
}
