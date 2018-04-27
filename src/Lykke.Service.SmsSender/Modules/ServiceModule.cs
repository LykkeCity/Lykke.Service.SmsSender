using System;
using System.Collections.Generic;
using Autofac;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Service.SmsSender.AzureRepositories.SmsProviderInfoRepository;
using Lykke.Service.SmsSender.AzureRepositories.SmsRepository;
using Lykke.Service.SmsSender.AzureRepositories.SmsSenderSettings;
using Lykke.Service.SmsSender.Core.Domain.SmsProviderInfoRepository;
using Lykke.Service.SmsSender.Core.Domain.SmsRepository;
using Lykke.Service.SmsSender.Core.Domain.SmsSenderSettings;
using Lykke.Service.SmsSender.Core.Services;
using Lykke.Service.SmsSender.Core.Settings.ServiceSettings;
using Lykke.Service.SmsSender.Sagas;
using Lykke.Service.SmsSender.Sagas.Commands;
using Lykke.Service.SmsSender.Sagas.Events;
using Lykke.Service.SmsSender.Services;
using Lykke.Service.SmsSender.Services.SmsSenders.Nexmo;
using Lykke.Service.SmsSender.Services.SmsSenders.Routee;
using Lykke.Service.SmsSender.Services.SmsSenders.Twilio;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Hosting;

namespace Lykke.Service.SmsSender.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<SmsSenderSettings> _settings;
        private readonly IHostingEnvironment _env;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<SmsSenderSettings> settings, IHostingEnvironment env, ILog log)
        {
            _settings = settings;
            _env = env;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance();

            builder.RegisterInstance(_settings.CurrentValue.SmsSettings).SingleInstance();
            
            builder.RegisterType<SettingsService>()
                .As<ISettingsService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.SmsSettings.DefaultSmsProvider))
                .SingleInstance();
            
            builder.RegisterType<SmsSenderFactory>()
                .As<ISmsSenderFactory>()
                .SingleInstance();
            
            builder.RegisterType<TwilioSmsSender>()
                .As<ISmsSender>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Senders.Twilio))
                .WithParameter("baseUrl", _settings.CurrentValue.BaseUrl)
                .SingleInstance();
            
            builder.RegisterType<NexmoSmsSender>()
                .As<ISmsSender>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Senders.Nexmo))
                .WithParameter("baseUrl", _settings.CurrentValue.BaseUrl)
                .SingleInstance();
            
            builder.RegisterType<RouteeSmsSender>()
                .As<ISmsSender>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Senders.Routee))
                .WithParameter("baseUrl", _settings.CurrentValue.BaseUrl)
                .SingleInstance();

            builder.RegisterInstance<ISmsSenderSettingsRepository>(
                new SmsSenderSettingsRepository(
                    AzureTableStorage<SmsSenderSettingsEntity>.Create(
                        _settings.ConnectionString(x => x.Db.DataConnString), "SmsSenderSettings", _log))
            ).SingleInstance();
            
            builder.RegisterInstance<ISmsRepository>(
                new SmsRepository(
                    AzureTableStorage<SmsMessageEntity>.Create(
                        _settings.ConnectionString(x => x.Db.DataConnString), "SmsMessages", _log),
                AzureTableStorage<AzureIndex>.Create(_settings.ConnectionString(x => x.Db.DataConnString), "SmsMessages", _log))
            ).SingleInstance();
            
            builder.RegisterInstance<ISmsProviderInfoRepository>(
                new SmsProviderInfoRepository(
                    AzureTableStorage<SmsProviderInfoEntity>.Create(
                        _settings.ConnectionString(x => x.Db.DataConnString), "SmsProvierInfo", _log))
            ).SingleInstance();
            
            RegisterSagas(builder);
        }
        
        private void RegisterSagas(ContainerBuilder builder)
        {
            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.Rabbit.ConnectionString };

            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {"RabbitMq", new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq")}
                }),
                new RabbitMqTransportFactory());
            
            builder.RegisterType<SmsSaga>();

            builder.RegisterType<SmsCommandHandler>();

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            var smsCommands = new[]
            {
                typeof(ProcessSmsCommand), 
                typeof(SendSmsCommand), 
                typeof(SmsDeliveredCommand), 
                typeof(SmsNotDeliveredCommand)
            };

            var smsEvents = new[]
            {
                typeof(SmsProviderProcessed),
                typeof(SmsMessageDeliveryFailed)
            };

            builder.Register(ctx => new CqrsEngine(_log,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,

                Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver("RabbitMq", "protobuf", environment: _env.EnvironmentName)),
                
                Register.BoundedContext("sms")
                    .FailedCommandRetryDelay((long)TimeSpan.FromSeconds(10).TotalMilliseconds)
                    .ListeningCommands(smsCommands)
                    .On("sms-commands")
                    .PublishingEvents(smsEvents)
                    .With("sms-events")
                    .WithCommandsHandler<SmsCommandHandler>(),

                Register.Saga<SmsSaga>("sms-saga")
                    .ListeningEvents(smsEvents)
                    .From("sms").On("sms-events")
                    .PublishingCommands(smsCommands)
                    .To("sms").With("sms-saga-commands"),

                Register.DefaultRouting
                    .PublishingCommands(smsCommands)
                    .To("sms").With("sms-saga-commands"))
            ).As<ICqrsEngine>().SingleInstance();
        }
    }
}
