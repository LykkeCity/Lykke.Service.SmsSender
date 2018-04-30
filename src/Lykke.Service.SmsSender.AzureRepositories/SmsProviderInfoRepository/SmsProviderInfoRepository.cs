using System;
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
                        switch (status)
                        {
                            case SmsDeliveryStatus.Delivered:
                                infoEntity.DeliveredCount++;
                                break;
                            case SmsDeliveryStatus.Failed:
                                infoEntity.DeliveryFailedCount++;
                                break;
                            case SmsDeliveryStatus.Unknown:
                                infoEntity.UnknownCount++;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(status), status, null);
                        }
                            
                        return infoEntity;
                    });
            }
        }

        public async Task<IEnumerable<ISmsProviderInfo>> GetAllByProviderAsync(SmsProvider provider)
        {
            return await _tableStorage.GetDataAsync(SmsProviderInfoEntity.GeneratePartitionKey(provider));
        }

        public async Task<IEnumerable<ISmsProviderInfo>> GetAllAsync()
        {
            return await _tableStorage.GetDataAsync();
        }
    }
}
