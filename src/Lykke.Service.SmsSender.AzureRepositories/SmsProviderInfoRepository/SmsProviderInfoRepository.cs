using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.SmsSender.Core.Domain;
using Lykke.Service.SmsSender.Core.Domain.SmsProviderInfoRepository;

namespace Lykke.Service.SmsSender.AzureRepositories.SmsProviderInfoRepository
{
    public class SmsProviderInfoRepository : ISmsProviderInfoRepository
    {
        private readonly INoSQLTableStorage<SmsProviderInfoEntity> _tableStorage;

        public SmsProviderInfoRepository(INoSQLTableStorage<SmsProviderInfoEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
        
        public async Task AddAsync(SmsProvider provider, string countryCode, SmsDeliveryStatus status)
        {
            var entity = await _tableStorage.GetDataAsync(SmsProviderInfoEntity.GeneratePartitionKey(provider),
                SmsProviderInfoEntity.GenerateRowKey(countryCode));

            if (entity == null)
            {
                entity = SmsProviderInfoEntity.Create(provider, countryCode, status);
                await _tableStorage.TryInsertAsync(entity);
            }
            else
            {
                await _tableStorage.MergeAsync(SmsProviderInfoEntity.GeneratePartitionKey(provider), SmsProviderInfoEntity.GenerateRowKey(countryCode),
                    infoEntity =>
                    {
                        if (status == SmsDeliveryStatus.Delivered)
                            entity.DeliveredCount++;
                        else
                        {
                            entity.DeliveryFailedCount++;
                            entity.RetryCount++;
                        }
                            
                        return entity;
                    });
            }
        }

        public async Task<IEnumerable<ISmsProviderInfo>> GetAllByProvider(SmsProvider provider)
        {
            return await _tableStorage.GetDataAsync(SmsProviderInfoEntity.GeneratePartitionKey(provider));
        }
    }
}
