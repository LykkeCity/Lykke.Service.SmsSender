using System;
using Common.Log;
using PhoneNumbers;

namespace Lykke.Service.SmsSender.Extensions
{
    public static class StringExtensions
    {
        public static PhoneNumber GetValidPhone(this string phone, ILog log)
        {
            if (string.IsNullOrEmpty(phone))
                return null;

            try
            {
                var phoneUtils = PhoneNumberUtil.GetInstance();

                var validPhone = phoneUtils.Parse(phone, null);

                return validPhone != null && phoneUtils.IsValidNumber(validPhone)
                    ? validPhone
                    : null;
            }
            catch (Exception e)
            {
                log.WriteError(nameof(GetValidPhone),  new {phone}, e);
                return null;
            }
        }
    }
}
