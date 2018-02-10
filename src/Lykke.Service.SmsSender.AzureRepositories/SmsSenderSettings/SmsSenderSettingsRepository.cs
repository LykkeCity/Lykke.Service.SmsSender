using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.SmsSender.Core.Domain.SmsSenderSettings;

namespace Lykke.Service.SmsSender.AzureRepositories.SmsSenderSettings
{
    public class SmsSenderSettingsRepository : ISmsSenderSettingsRepository
    {
        private readonly INoSQLTableStorage<SmsSenderSettingsEntity> _tableStorage;

        public SmsSenderSettingsRepository(INoSQLTableStorage<SmsSenderSettingsEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<T> GetAsync<T>() where T : SmsSenderSettingsBase, new()
        {
            var partitionKey = SmsSenderSettingsEntity.GeneratePartitionKey();
            var defaultValue = SmsSenderSettingsBase.CreateDefault<T>();
            var rowKey = SmsSenderSettingsEntity.GenerateRowKey(defaultValue);
            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);
            return entity == null ? defaultValue : entity.GetSmsSenderSettings<T>();
        }

        public async Task SetAsync<T>(T settings) where T : SmsSenderSettingsBase, new()
        {
            var newEntity = SmsSenderSettingsEntity.Create(settings);
            await _tableStorage.InsertOrReplaceAsync(newEntity);
        }

        public async Task DeleteAsync<T>() where T : SmsSenderSettingsBase, new()
        {
            var partitionKey = SmsSenderSettingsEntity.GeneratePartitionKey();
            var defaultValue = SmsSenderSettingsBase.CreateDefault<T>();
            var rowKey = SmsSenderSettingsEntity.GenerateRowKey(defaultValue);
            await _tableStorage.DeleteIfExistAsync(partitionKey, rowKey);
        }
    }
}
