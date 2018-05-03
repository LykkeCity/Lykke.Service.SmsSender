using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.SmsSender.Core.Domain.SmsProviderInfoRepository
{
    public interface ISmsProviderInfoRepository
    {
        Task AddAsync(SmsProvider provider, string countryCode, SmsDeliveryStatus status);
        Task<IEnumerable<ISmsProviderInfo>> GetAllByProviderAsync(SmsProvider provider);
        Task<IEnumerable<ISmsProviderInfo>> GetAllAsync();
    }
}
