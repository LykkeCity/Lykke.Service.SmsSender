using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.SmsSender.Core.Domain;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings.SenderSettings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.SmsSender.Core.Settings.ServiceSettings
{
    public class SmsSenderSettings
    {
        public string BaseUrl { get; set; }
        public DbSettings Db { get; set; }
        public RabbitMqSettings Rabbit { get; set; }
        public SmsSettings SmsSettings { get; set; }
        public SendersSettings Senders { get; set; }

        public void Validate()
        {
            SmsSettings.Validate();
        }
    }

    public class SmsSettings
    {
        public SmsProvider DefaultSmsProvider { get; set; }
        public TimeSpan SmsRetryTimeout { get; set; }
        public TimeSpan SmsSendDelay { get; set; }
        [Optional]
        public List<string> BlockedCountries { get; set; } = new List<string>();
        [Optional]
        public List<string> AllowedCountries { get; set; } = new List<string>();

        public void Validate()
        {
            if(BlockedCountries.Any() && AllowedCountries.Any())
            {
                throw new InvalidOperationException("Either BlockedCountries or AllowedCountries can be specified but not both!");
            }
        }
    }
}
