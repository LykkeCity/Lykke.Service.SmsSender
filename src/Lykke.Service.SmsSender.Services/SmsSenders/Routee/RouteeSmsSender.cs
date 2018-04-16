using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Flurl.Http;
using Lykke.Service.SmsSender.Core.Services;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings.SenderSettings;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Routee
{
    public class RouteeSmsSender : ISmsSender
    {
        private readonly IMemoryCache _memoryCache;
        private readonly string _baseUrl;
        private readonly ProviderSettings _settings;
        private readonly ILog _log;

        private const string TokenKey = "Token";
        private const int RetryCount = 5;

        public RouteeSmsSender(
            IMemoryCache memoryCache,
            string baseUrl,
            ProviderSettings settings,
            ILog log)
        {
            _memoryCache = memoryCache;
            _baseUrl = baseUrl;
            _settings = settings;
            _log = log.CreateComponentScope(nameof(RouteeSmsSender));
        }
        
        public async Task<string> SendSmsAsync(string phone, string message, string countryCode)
        {
            string token = await GetTokenAsync();

            if (token == null)
            {
                _log.WriteWarning(nameof(SendSmsAsync), new { phone = phone?.SanitizePhone(), message = message, countryCode = countryCode }, "No access token");
                return null;
            }

            int retryCount = 0;

            do
            {
                try
                {
                    RouteeResponse response = await _settings.BaseUrl
                        .WithHeader("authorization", $"Bearer {token}")
                        .PostJsonAsync(new
                        {
                            from = _settings.GetFrom(countryCode),
                            body = message,
                            to = phone,
                            callback = new
                            {
                                url = $"{_baseUrl}/callback/routee",
                                strategy = RouteeCallbackStrategy.OnCompletion
                            }
                        })
                        .ReceiveJson<RouteeResponse>();

                    return response.TrackingId;
                }
                catch (FlurlHttpException ex)
                {
                    if (ex.Call.HttpStatus == HttpStatusCode.Unauthorized)
                    {
                        token = await GetTokenAsync(true);
                        retryCount++;
                    }
                    else
                    {
                        var error = await ex.GetResponseJsonAsync<RouteeErrorResponse>();

                        if (error.Code == RouteeError.NotEnoughBalance)
                            _log.WriteError(nameof(SendSmsAsync), error, ex);
                        else
                            _log.WriteWarning(nameof(SendSmsAsync), error, "error sending sms", ex);
                    }
                }
            } while (retryCount > 0 && retryCount <= RetryCount);

            return null;
        }

        private async Task<string> GetTokenAsync(bool forceUpdate = false)
        {
            if (!forceUpdate && _memoryCache.TryGetValue(TokenKey, out RouteeToken token))
                return token.AccessToken;
            
            string base64Token = $"{_settings.ApiKey}:{_settings.ApiSecret}".ToBase64();

            try
            {
                token = await _settings.AuthUrl
                    .WithHeader("authorization", $"Basic {base64Token}")
                    .PostUrlEncodedAsync(new { grant_type = "client_credentials"})
                    .ReceiveJson<RouteeToken>();

                if (token != null)
                {
                    _memoryCache.Set(TokenKey, token, TimeSpan.FromSeconds(token.ExpiresIn));
                    return token.AccessToken;
                }
            }
            catch (FlurlHttpException ex)
            {
                _log.WriteWarning(nameof(GetTokenAsync), ex.Message, "error getting access token", ex);
            }

            return null;
        }
    }
}
