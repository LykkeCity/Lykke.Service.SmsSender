using System.Threading.Tasks;
using Lykke.Service.SmsSender.Core.Domain;

namespace Lykke.Service.SmsSender.Core.Services
{
    public interface ISettingsService
    {
        Task<SmsProvider> GetProviderByCountryAsync(string countryCode);
    }
}
