using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.SmsSender.Core.Domain;
using Lykke.Service.SmsSender.Core.Domain.SmsSenderSettings;
using Lykke.Service.SmsSender.Core.Services;

namespace Lykke.Service.SmsSender.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISmsSenderSettingsRepository _settingsRepository;
        private readonly SmsProvider _defaultProvider;

        public SettingsService(
            ISmsSenderSettingsRepository settingsRepository,
            SmsProvider defaultProvider)
        {
            _settingsRepository = settingsRepository;
            _defaultProvider = defaultProvider;
        }
        public async Task<SmsProvider> GetProviderByCountryAsync(string countryCode)
        {
            var providerCountries = await _settingsRepository.GetAsync<SmsProviderCountries>();

            if (providerCountries != null)
            {
                foreach (var pair in providerCountries.Countries)
                    if (pair.Value.Contains(countryCode, StringComparer.OrdinalIgnoreCase))
                        return pair.Key;
            }

            return _defaultProvider;
        }
    }
}
