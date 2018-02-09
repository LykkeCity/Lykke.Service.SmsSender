using System.Threading.Tasks;
using Common;
using Common.Log;
using Flurl.Http;
using Lykke.Service.SmsSender.Core.Services;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings.SenderSettings;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Twilio
{
    public class TwilioSmsSender : ISmsSender
    {
        private readonly string _baseUrl;
        private readonly ProviderSettings _settings;
        private readonly ILog _log;
        private const string BaseApiUrl = "https://api.twilio.com/2010-04-01";

        public TwilioSmsSender(
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
            try
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

                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    _log.WriteWarning(nameof(SendSmsAsync), new
                    {
                        response.Sid,
                        Phone = response.To.SanitizePhone(),
                        countryCode,
                        response.Status,
                        response.ErrorCode,
                        response.ErrorMessage
                    }, "twilio error response");
                }
                else
                {
                    return response.Sid;
                }
            }
            catch (FlurlHttpException ex)
            {
                var error = ex.GetResponseJson<TwilioErrorResponse>();
                _log.WriteWarning(nameof(SendSmsAsync), error, "twilio: error sending sms");
            }
            
            return null;
        }
    }
}
