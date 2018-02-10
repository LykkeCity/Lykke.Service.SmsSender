namespace Lykke.Service.SmsSender.Services.SmsSenders.Nexmo
{
    public enum NexmoStatus
    {
        Ok = 0,
        Throttled = 1,
        MissingParams = 2,
        InvalidParams = 3,
        InvalidCredentials = 4,
        InternalError = 5,
        InvalidMessage = 6,
        NumberBarred = 7,
        PartnerAccountBarred = 8,
        PartnerQuotaExceeded = 9,
        AccountRestDisabled = 11,
        MessageToLong = 12,
        CommunicationFailed = 13,
        InvalidSignature = 14,
        IllegalSenderAddress  = 15,
        InvalidTtl = 16,
        FacilityNotAllowed = 19,
        InvalidMessageClass = 20,
        BadCallbackNoHttps  = 23,
        ToParamNotWhitelisted = 29,
        InvalidMsisdn = 34
    }
}
