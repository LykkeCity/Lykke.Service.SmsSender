using System.Threading.Tasks;

namespace Lykke.Service.SmsSender.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}