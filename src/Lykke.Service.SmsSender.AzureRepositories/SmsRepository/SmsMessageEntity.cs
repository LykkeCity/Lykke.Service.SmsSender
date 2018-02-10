using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.SmsSender.Core.Domain;
using Lykke.Service.SmsSender.Core.Domain.SmsRepository;

namespace Lykke.Service.SmsSender.AzureRepositories.SmsRepository
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    public class SmsMessageEntity : AzureTableEntity, ISmsMessage
    {
        public string Id { get; set; }
        public string MessageId { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public SmsProvider Provider { get; set; }
        public string CountryCode { get; set; }
        public DateTime Created { get; set; }

        internal static string GenerateRowKey(string id) => id;
        internal static string GeneratePartitionKey(DateTime created) => created.ToString("yyyy-MM-dd");

        internal static SmsMessageEntity Create(ISmsMessage message)
        {
            string id = string.IsNullOrEmpty(message.Id) 
                ? Guid.NewGuid().ToString() 
                : message.Id;
            
            return new SmsMessageEntity
            {
                PartitionKey = GeneratePartitionKey(message.Created == DateTime.MinValue ? DateTime.UtcNow : message.Created),
                RowKey = GenerateRowKey(id),
                Id = id,
                CountryCode = message.CountryCode,
                Message = message.Message,
                MessageId = message.MessageId,
                Phone = message.Phone,
                Provider = message.Provider,
                Created = DateTime.UtcNow
            };
        }
    }
}
