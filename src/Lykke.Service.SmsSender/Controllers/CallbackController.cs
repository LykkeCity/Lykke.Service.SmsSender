using System.Threading.Tasks;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Service.SmsSender.Core.Domain.SmsRepository;
using Lykke.Service.SmsSender.Models;
using Lykke.Service.SmsSender.Sagas.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.SmsSender.Controllers
{
    [Route("callback")]
    [ApiExplorerSettings(IgnoreApi=true)]
    public class CallbackController : Controller
    {
        private readonly ISmsRepository _smsRepository;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ILog _log;

        public CallbackController(
            ISmsRepository smsRepository,
            ICqrsEngine cqrsEngine,
            ILog log
        )
        {
            _smsRepository = smsRepository;
            _cqrsEngine = cqrsEngine;
            _log = log;
        }

        [HttpPost]
        [Route("twilio")]
        public async Task<IActionResult> TwilioCallback([FromForm] TwilioCallbackModel model)
        {
            if (!string.IsNullOrEmpty(model?.MessageSid))
            {
                var sms = await _smsRepository.GetByMessageIdAsync(model.MessageSid);

                if (sms == null)
                {
                    _log.WriteWarning(nameof(TwilioCallback), model, $"Sms message with messageId = {model.MessageSid} not found");
                    return Ok();
                }
                
                switch (model.MessageStatus)
                {
                    case TwilioMessageStatus.Delivered:
                        _cqrsEngine.SendCommand(new SmsDeliveredCommand {Message = sms}, "sms", "sms");
                        break;
                    case TwilioMessageStatus.Failed:
                    case TwilioMessageStatus.Undelivered:
                        _cqrsEngine.SendCommand(new SmsNotDeliveredCommand {Message = sms, Error = $"status = {model.MessageStatus}"}, "sms", "sms");
                        break;
                    default:
                        _log.WriteWarning(nameof(TwilioCallback), model, $"status = {model.MessageStatus}");
                        break;
                }
            }

            return Ok();
        }

        [HttpPost]
        [HttpGet]
        [Route("nexmo")]
        public async Task<IActionResult> NexmoCallback(NexmoCallbackModel model)
        {
            if (!string.IsNullOrEmpty(model?.MessageId))
            {
                var sms = await _smsRepository.GetByMessageIdAsync(model.MessageId);

                if (sms == null)
                {
                    _log.WriteWarning(nameof(NexmoCallback), model, $"Sms message with messageId = {model.MessageId} not found");
                    return Ok();
                }
                
                if (model.Status == NexmoMessageStatus.Delivered && model.ErrorCode == NexmoErrorCode.Delivered)
                    _cqrsEngine.SendCommand(new SmsDeliveredCommand {Message = sms}, "sms", "sms");
                else
                    _cqrsEngine.SendCommand(new SmsNotDeliveredCommand {Message = sms, Error = $"status = {model.Status}, error = {model.ErrorCode}"}, "sms", "sms");
            }

            return Ok();
        }
    }
}
