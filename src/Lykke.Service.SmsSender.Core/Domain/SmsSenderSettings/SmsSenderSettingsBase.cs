namespace Lykke.Service.SmsSender.Core.Domain.SmsSenderSettings
{
    public abstract class SmsSenderSettingsBase
    {
        public abstract string GetKey();
        
        public static T CreateDefault<T>() where T : SmsSenderSettingsBase, new()
        {
            if (typeof(T) == typeof(SmsProviderCountries))
                return SmsProviderCountries.CreateDefault() as T;

            return new T();
        }
    }
}
