using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.SmsSender.Core.Settings.ServiceSettings.SenderSettings
{
    public class ProviderSettings
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string From { get; set; }
        [Optional]
        public IReadOnlyDictionary<string, string> FromMap { get; set; } = new Dictionary<string, string>();

        public string GetFrom(string countryCode)
        {
            return FromMap.ContainsKey(countryCode)
                ? FromMap[countryCode]
                : From;
        }
    }
}
