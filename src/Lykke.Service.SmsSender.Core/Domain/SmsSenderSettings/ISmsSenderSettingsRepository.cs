using System.Threading.Tasks;

namespace Lykke.Service.SmsSender.Core.Domain.SmsSenderSettings
{
    public interface ISmsSenderSettingsRepository
    {
        Task<T> GetAsync<T>() where T : SmsSenderSettingsBase, new();
        Task SetAsync<T>(T data) where T : SmsSenderSettingsBase, new();
        Task DeleteAsync<T>() where T : SmsSenderSettingsBase, new();
    }
}
