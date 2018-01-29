using System.Threading.Tasks;

namespace Lykke.Service.SmsSender.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}