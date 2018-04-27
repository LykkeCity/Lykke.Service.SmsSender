namespace Lykke.Service.SmsSender.Core.Settings.ServiceSettings.SenderSettings
{
    public class SendersSettings
    {
        public ProviderSettings Twilio { get; set; }
        public ProviderSettings Nexmo { get; set; }
        public ProviderSettings Routee { get; set; }
    }
}
