
using System.Threading.Tasks;

namespace Lykke.Service.SmsSender.Client
{
    public interface ISmsSenderClient
    {
        Task SendSmsAsync(string phone, string message, string reason, string outerRequestId);
    }
}
