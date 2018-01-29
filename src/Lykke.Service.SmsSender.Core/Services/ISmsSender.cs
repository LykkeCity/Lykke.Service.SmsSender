using System.Threading.Tasks;

namespace Lykke.Service.SmsSender.Core.Services
{
    public interface ISmsSender
    {
        /// <summary>
        /// Sends sms
        /// </summary>
        /// <param name="phone">phone number in E164 format</param>
        /// <param name="message">message to send</param>
        /// <returns>message id</returns>
        Task<string> SendSmsAsync(string phone, string message);
    }
}
