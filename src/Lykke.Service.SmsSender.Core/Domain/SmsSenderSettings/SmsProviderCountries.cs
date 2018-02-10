using System.Collections.Generic;

namespace Lykke.Service.SmsSender.Core.Domain.SmsSenderSettings
{
    public class SmsProviderCountries : SmsSenderSettingsBase
    {
        public Dictionary<SmsProvider, string[]> Countries { get; set; } = new Dictionary<SmsProvider, string[]>();
        public override string GetKey() => "SmsProviderCountries";
        public static SmsProviderCountries CreateDefault() => new SmsProviderCountries();
    }
}
