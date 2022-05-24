using System;
using System.Net;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Cqrs;
using Lykke.Service.SmsSender.Extensions;
using Lykke.Service.SmsSender.Models;
using Lykke.Service.SmsSender.Sagas.Commands;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.SmsSender.Controllers
{
    [Route("api/[controller]")]
    public class SmsController : Controller
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ILog _log;

        public SmsController(
            ICqrsEngine cqrsEngine,
            ILog log)
        {
            _cqrsEngine = cqrsEngine;
            _log = log;
        }
        
        [HttpPost]
        [SwaggerOperation("Send")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public IActionResult SendSms([FromBody]SmsModel model)
        {
            if (model == null)
                return BadRequest("Model is null");

            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetError());

            var phone = model.Phone.GetValidPhone();

            if (phone == null)
            {
                ModelState.AddModelError(nameof(model.Phone), "invalid phone number");
                return BadRequest(ModelState.GetError());
            }
            
            if (model.Message.Length > 160)
            {
                ModelState.AddModelError(nameof(model.Message), "Message length is too long (max. 160 chars)");
                return BadRequest(ModelState.GetError());
            }

            try
            {
                _cqrsEngine.SendCommand(new ProcessSmsCommand {Message = model.Message, Phone = model.Phone.Replace(" ","")}, "sms", "sms");
                return Ok();
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(SendSms), new { Phone = model.Phone.SanitizePhone()}, ex);
                return BadRequest(ErrorResponse.Create("Technical problems"));
            }
        }
    }
}
