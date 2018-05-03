using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.SmsSender.Core.Domain.SmsProviderInfoRepository;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.SmsSender.Controllers
{
    [Route("statistics")]
    public class StatisticsController : Controller
    {
        private readonly ISmsProviderInfoRepository _smsProviderInfoRepository;

        public StatisticsController(ISmsProviderInfoRepository smsProviderInfoRepository)
        {
            _smsProviderInfoRepository = smsProviderInfoRepository;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var info = (await _smsProviderInfoRepository.GetAllAsync()).ToList();

            if (info.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("Provider;Country code;Delivered count;Failed count;Unknown count");

                foreach (var line in info)
                {
                    sb.AppendLine($"{line.Provider};{line.CountryCode};{line.DeliveredCount.ToString()};{line.DeliveryFailedCount.ToString()};{line.UnknownCount.ToString()}");
                }

                var data = Encoding.UTF8.GetBytes(sb.ToString());

                return File(data, "application/vnd.ms-excel", "provider_stats.csv");
            }

            return NoContent();
        }
    }
}
