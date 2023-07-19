using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.SmsSender.Client.AutorestClient;
using Lykke.Service.SmsSender.Client.AutorestClient.Models;
using Microsoft.Rest;

namespace Lykke.Service.SmsSender.Client
{
    public class SmsSenderClient : ISmsSenderClient, IDisposable
    {
        private SmsSenderAPI _service;

        public SmsSenderClient(string serviceUrl)
        {
            _service = new SmsSenderAPI(new Uri(serviceUrl));
        }

        public void Dispose()
        {
            _service?.Dispose();
            _service = null;
        }

        public async Task SendSmsAsync(string phone, string message, string reason, string outerRequestId)
        {
            try
            {
                var result = await _service.SendAsync(new SmsModel
                {
                    Message = message,
                    Phone = phone,
                    Reason = reason,
                    OuterRequestId = outerRequestId
                });
                
                if (result != null && result is ErrorResponse resp)
                    throw new SmsServiceException(resp.ErrorMessage);
            }
            catch (ValidationException ex)
            {
                throw new SmsServiceException(ex.Message);
            }
        }
    }
}
