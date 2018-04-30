using System.Threading.Tasks;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Service.SmsSender.Core.Domain.SmsRepository;
using Lykke.Service.SmsSender.Models;
using Lykke.Service.SmsSender.Sagas.Commands;
using Lykke.Service.SmsSender.Services.SmsSenders.Routee;
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
            _log = log.CreateComponentScope(nameof(CallbackController));
        }

        [HttpPost]
        [Route("twilio")]
        public async Task<IActionResult> TwilioCallback([FromForm] TwilioCallbackModel model)
        {
            if (model == null)
                return Ok();
            
            _log.WriteInfo(nameof(TwilioCallback), model.Sanitize(), "Twilio callback");
            
            if (!string.IsNullOrEmpty(model.MessageSid))
            {
                var sms = await _smsRepository.GetByMessageIdAsync(model.MessageSid);

                if (sms == null)
                {
                    _log.WriteInfo(nameof(TwilioCallback), model.MessageSid, $"Sms message with messageId = {model.MessageSid} not found");
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
                        _log.WriteWarning(nameof(TwilioCallback), model.MessageSid, $"status = {model.MessageStatus}, callback processing is skipped");
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
            if (model == null)
                return Ok();
            
            _log.WriteInfo(nameof(NexmoCallback), model, "Nexmo callback");
            
            if (!string.IsNullOrEmpty(model.MessageId))
            {
                var sms = await _smsRepository.GetByMessageIdAsync(model.MessageId);

                if (sms == null)
                {
                    _log.WriteInfo(nameof(NexmoCallback), model.MessageId, $"Sms message with messageId = {model.MessageId} not found");
                    return Ok();
                }

                switch (model.Status)
                {
                    case NexmoMessageStatus.Delivered:
                        _cqrsEngine.SendCommand(new SmsDeliveredCommand {Message = sms}, "sms", "sms");
                        break;
                    case NexmoMessageStatus.Expired:
                    case NexmoMessageStatus.Failed:
                    case NexmoMessageStatus.Rejected:
                        _cqrsEngine.SendCommand(new SmsNotDeliveredCommand {Message = sms, Error = $"status = {model.Status}, error = {model.ErrorCode}"}, "sms", "sms");
                        break;
                    case NexmoMessageStatus.Unknown:
                        _cqrsEngine.SendCommand(new SmsDeliveryUnknownCommand {Message = sms, Error = $"status = {model.Status}, error = {model.ErrorCode}"}, "sms", "sms");
                        break;
                    default:
                        _log.WriteWarning(nameof(NexmoCallback), model.MessageId, $"status = {model.Status}, callback processing is skipped");
                        break;
                }
            }

            return Ok();
        }
        
        [HttpPost]
        [Route("routee")]
        public async Task<IActionResult> RouteeCallback([FromBody]RouteeCallbackModel model)
        {
            if (model == null)
                return Ok();
            
            _log.WriteInfo(nameof(RouteeCallback), model, "Routee callback");
            
            if (!string.IsNullOrEmpty(model.MessageId))
            {
                var sms = await _smsRepository.GetByMessageIdAsync(model.MessageId);

                if (sms == null)
                {
                    _log.WriteInfo(nameof(RouteeCallback), model.MessageId, $"Sms message with messageId = {model.MessageId} not found");
                    return Ok();
                }

                switch (model.Status.Name)
                {
                    case RouteeStaus.Delivered:
                        _cqrsEngine.SendCommand(new SmsDeliveredCommand {Message = sms}, "sms", "sms");
                        break;
                    case RouteeStaus.Undelivered:
                    case RouteeStaus.Failed:
                        if (model.Status.Reason?.DetailedStatus == RouteeDetailedStatus.UnknownStatus)
                            _cqrsEngine.SendCommand(new SmsDeliveryUnknownCommand {Message = sms, Error = $"status = {model.Status.Name}, error = {model.Status.Reason?.DetailedStatus} : {model.Status.Reason?.Description}"}, "sms", "sms");
                        else    
                            _cqrsEngine.SendCommand(new SmsNotDeliveredCommand {Message = sms, Error = $"status = {model.Status.Name}, error = {model.Status.Reason?.DetailedStatus} : {model.Status.Reason?.Description}"}, "sms", "sms");
                        break;
                    default:
                        _log.WriteWarning(nameof(NexmoCallback), model.MessageId, $"status = {model.Status}, callback processing is skipped");
                        break;
                }
            }

            return Ok();
        }
    }
}
