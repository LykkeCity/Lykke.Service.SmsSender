using System.Dynamic;
using System.Runtime.CompilerServices;
using Lykke.Service.SmsSender.Core.Domain;
using Lykke.Service.SmsSender.Core.Domain.SmsProviderInfoRepository;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.SmsSender.AzureRepositories.SmsProviderInfoRepository
{
    public class SmsProviderInfoEntity : TableEntity, ISmsProviderInfo
    {
        public string Provider { get; set; }
        public string CountryCode { get; set; }
        public long DeliveredCount { get; set; }
        public long DeliveryFailedCount { get; set; }
        public long RetryCount { get; set; }

        internal static string GeneratePartitionKey(SmsProvider provider) => provider.ToString();
        internal static string GenerateRowKey(string countryCode) => countryCode;

        internal static SmsProviderInfoEntity Create(SmsProvider provider, string countryCode, SmsDeliveryStatus status)
        {
            return new SmsProviderInfoEntity
            {
                PartitionKey = GeneratePartitionKey(provider),
                RowKey = GenerateRowKey(countryCode),
                Provider = provider.ToString(),
                CountryCode = countryCode,
                DeliveredCount = status == SmsDeliveryStatus.Delivered ? 1 : 0,
                DeliveryFailedCount = status == SmsDeliveryStatus.Failed ? 1 : 0,
            };
        }
    }
}
