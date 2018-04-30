namespace Lykke.Service.SmsSender.Core.Domain.SmsProviderInfoRepository
{
    public interface ISmsProviderInfo
    {
        string Provider { get; }
        string CountryCode { get; }
        long DeliveredCount { get; }
        long DeliveryFailedCount { get; }
        long UnknownCount { get; }
    }
}
