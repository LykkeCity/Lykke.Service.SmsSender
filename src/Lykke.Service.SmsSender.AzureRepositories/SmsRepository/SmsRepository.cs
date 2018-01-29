using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.SmsSender.Core.Domain.SmsRepository;

namespace Lykke.Service.SmsSender.AzureRepositories.SmsRepository
{
    public class SmsRepository : ISmsRepository
    {
        private readonly INoSQLTableStorage<SmsMessageEntity> _tableStorage;
        private readonly INoSQLTableStorage<AzureIndex> _index;
        private const string MessageIndex = "MessageIndex";


        public SmsRepository(INoSQLTableStorage<SmsMessageEntity> tableStorage, 
            INoSQLTableStorage<AzureIndex> index)
        {
            _tableStorage = tableStorage;
            _index = index;
        }
        
        public async Task<string> AddAsync(SmsMessage message)
        {
            var entity = SmsMessageEntity.Create(message);
            await _tableStorage.TryInsertAsync(entity);
            return entity.Id;
        }

        public async Task<SmsMessage> GetAsync(string id)
        {
            var entity = await _tableStorage.GetDataAsync(SmsMessageEntity.GeneratePartitionKey(), SmsMessageEntity.GenerateRowKey(id));
            return SmsMessage.Create(entity);
        }

        public async Task SetMessageIdAsync(string messageId, string id)
        {
            var entity = await _tableStorage.GetDataAsync(SmsMessageEntity.GeneratePartitionKey(), SmsMessageEntity.GenerateRowKey(id));
            
            if (!string.IsNullOrEmpty(entity.MessageId))
                await _tableStorage.DeleteIfExistAsync(MessageIndex, entity.MessageId);

            await _tableStorage.MergeAsync(SmsMessageEntity.GeneratePartitionKey(), SmsMessageEntity.GenerateRowKey(id), messageEntity =>
            {
                messageEntity.MessageId = messageId;
                return messageEntity;
            });
            
            var indexEntity = AzureIndex.Create(MessageIndex, messageId, entity);
            await _index.InsertAsync(indexEntity);
        }

        public async Task<SmsMessage> GetByMessageIdAsync(string messageId)
        {
            var entity = await _tableStorage.GetDataAsync(_index, MessageIndex, messageId);
            return SmsMessage.Create(entity);
        }

        public async Task DeleteAsync(string id, string messageId)
        {
            await _tableStorage.DeleteIfExistAsync(SmsMessageEntity.GeneratePartitionKey(), SmsMessageEntity.GenerateRowKey(id));
            
            if (!string.IsNullOrEmpty(messageId))
                await _index.DeleteIfExistAsync(MessageIndex, messageId);
        }
    }
}
