using System;

namespace Lykke.Service.SmsSender.Core.Domain.SmsRepository
{
    public interface ISmsMessage
    {
        string Id { get; }
        string MessageId { get; }
        string Phone { get; }
        string Message { get; }
        SmsProvider Provider { get; }
        string CountryCode { get; }
        DateTime Created { get; }
    }
}
