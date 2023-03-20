using Lykke.Service.SmsSender.Core.Settings.ServiceSettings;
using Lykke.Service.SmsSender.Core.Settings.SlackNotifications;

namespace Lykke.Service.SmsSender.Core.Settings
{
    public class AppSettings
    {
        public SmsSenderSettings SmsSenderService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }

        public void Validate()
        {
            SmsSenderService.Validate();
        }
    }
}
