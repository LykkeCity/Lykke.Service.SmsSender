using System;
using Autofac;

namespace Lykke.Service.SmsSender.Client
{
    public static class AutofacExtension
    {
        public static void RegisterSmsSenderClient(this ContainerBuilder builder, string serviceUrl)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterType<SmsSenderClient>()
                .WithParameter("serviceUrl", serviceUrl)
                .As<ISmsSenderClient>()
                .SingleInstance();
        }

        public static void RegisterSmsSenderClient(this ContainerBuilder builder, SmsSenderServiceClientSettings settings)
        {
            builder.RegisterSmsSenderClient(settings?.ServiceUrl);
        }
    }
}
