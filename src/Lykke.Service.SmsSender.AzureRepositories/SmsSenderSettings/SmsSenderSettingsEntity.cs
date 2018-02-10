using Lykke.Service.SmsSender.Core.Domain.SmsSenderSettings;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.SmsSender.AzureRepositories.SmsSenderSettings
{
    public class SmsSenderSettingsEntity : TableEntity
    {
        internal static string GeneratePartitionKey() => "SmsSenderSettings";

        internal static string GenerateRowKey(SmsSenderSettingsBase settings) => settings.GetKey();

        public string Data { get; set; }

        internal T GetSmsSenderSettings<T>() where T : SmsSenderSettingsBase
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Data);
        }

        internal void SetSettings(SmsSenderSettingsBase settings)
        {
            Data = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
        }

        public static SmsSenderSettingsEntity Create(SmsSenderSettingsBase settings)
        {
            var result = new SmsSenderSettingsEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(settings),
            };
            
            result.SetSettings(settings);
            return result;
        }
    }
}
