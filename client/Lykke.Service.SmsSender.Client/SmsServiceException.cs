using System;

namespace Lykke.Service.SmsSender.Client
{
    public class SmsServiceException : Exception
    {
        public SmsServiceException(string message) : base (message)
        {
        }
    }
}