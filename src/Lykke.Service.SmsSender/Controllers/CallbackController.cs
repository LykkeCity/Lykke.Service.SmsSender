using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Service.SmsSender.Core.Domain.SmsRepository;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings;
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
        [HttpGet]
        [Route("twilio")]
        public async Task<IActionResult> TwilioCallback([FromBody] TwilioCallbackModel model)
        {
            if (!string.IsNullOrEmpty(model?.MessageSid))
            {
                var sms = await _smsRepository.GetByMessageIdAsync(model.MessageSid);


                if (sms == null)
                {
                    await _log.WriteWarningAsync(nameof(CallbackController), nameof(TwilioCallback), model.ToJson());
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
                        await _log.WriteWarningAsync(nameof(CallbackController), nameof(TwilioCallback), model.ToJson(), $"status = {model.MessageStatus}");
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
                    await _log.WriteWarningAsync(nameof(CallbackController), nameof(NexmoCallback), model.ToJson());
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
