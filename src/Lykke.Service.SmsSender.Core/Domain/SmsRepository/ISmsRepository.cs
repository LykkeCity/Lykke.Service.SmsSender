using System.Threading.Tasks;

namespace Lykke.Service.SmsSender.Core.Domain.SmsRepository
{
    public interface ISmsRepository
    {
        Task<string> AddAsync(SmsMessage message);
        Task<SmsMessage> GetAsync(string id);
        Task SetMessageIdAsync(string messageId, string id);
        Task<SmsMessage> GetByMessageIdAsync(string messageId);
        Task DeleteAsync(string id, string messageId);
    }
}
