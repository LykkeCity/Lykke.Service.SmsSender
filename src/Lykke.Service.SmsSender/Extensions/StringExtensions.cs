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
                log.WriteWarning(nameof(GetValidPhone),  new {phone}, "Error while phone validity check", e);
                return null;
            }
        }
    }
}
