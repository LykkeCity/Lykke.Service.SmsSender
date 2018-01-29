using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.SmsSender.Core.Settings.ServiceSettings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
    }
}
