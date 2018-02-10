using System;
using Lykke.Service.SmsSender.Core.Domain;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings.SenderSettings;

namespace Lykke.Service.SmsSender.Core.Settings.ServiceSettings
{
    public class SmsSenderSettings
    {
        public string BaseUrl { get; set; }
        public DbSettings Db { get; set; }
        public RabbitMqSettings Rabbit { get; set; }
        public SmsSettings SmsSettings { get; set; }
        public SendersSettings Senders { get; set; }
    }

    public class SmsSettings
    {
        public SmsProvider DefaultSmsProvider { get; set; }
        public TimeSpan SmsRetryTimeout { get; set; }
    }
}
