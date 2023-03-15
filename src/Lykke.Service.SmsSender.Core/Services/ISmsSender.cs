using System.Threading.Tasks;

namespace Lykke.Service.SmsSender.Core.Services
{
    public interface ISmsSender
    {
        /// <summary>
        /// Sends sms
        /// </summary>
        /// <param name="phone">phone number in E164 format</param>
        /// <param name="message">mmessage to send</param>
        /// <param name="countryCode">countryCode of the phone number</param>
        /// <returns>message id</returns>
        Task<string> SendSmsAsync(string commandId, string phone, string message, string countryCode);
    }
}
