using System.Linq;
using Lykke.Common.Api.Contract.Responses;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lykke.Service.SmsSender.Extensions
{
    public static class ModelStateExtensions
    {
        public static ErrorResponse GetError(this ModelStateDictionary modelState)
        {
            var response = new ErrorResponse();

            foreach (var state in modelState)
            {
                var message = state.Value.Errors
                    .Where(e => !string.IsNullOrWhiteSpace(e.ErrorMessage))
                    .Select(e => e.ErrorMessage)
                    .Concat(state.Value.Errors
                        .Where(e => string.IsNullOrWhiteSpace(e.ErrorMessage))
                        .Select(e => $"{e.Exception.Message} : '{state.Value.RawValue}'"))
                    .ToList()
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(message))
                    continue;

                response.ErrorMessage = message;
                break;
            }

            return response;
        }
    }
}
