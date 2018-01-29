using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Lykke.Service.SmsSender.Core.Services;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings.SenderSettings;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Twilio
{
    public class TwilioSmsSender : ISmsSender
    {
        private readonly string _baseUrl;
        private readonly ProviderSettings _settings;
        private const string BaseApiUrl = "https://api.twilio.com/2010-04-01";

        public TwilioSmsSender(
            string baseUrl,
            ProviderSettings settings)
        {
            _baseUrl = baseUrl;
            _settings = settings;
        }

        public async Task<string> SendSmsAsync(string phone, string message, string countryCode)
        {
            var response = await $"{BaseApiUrl}/Accounts/{_settings.ApiKey}/Messages.json"
                .WithBasicAuth(_settings.ApiKey, _settings.ApiSecret)
                .PostUrlEncodedAsync(new
                {
                    To = phone,
                    From = _settings.GetFrom(countryCode),
                    Body = message,
                    StatusCallback = $"{_baseUrl}/callback/twilio"
                }).ReceiveJson<TwilioResponse>();

            if (string.IsNullOrEmpty(response.ErrorMessage))
                return response.Sid;

            return null;
        }
    }
}
