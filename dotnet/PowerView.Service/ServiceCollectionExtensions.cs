using System;
using Microsoft.Extensions.DependencyInjection;
using PowerView.Service.DisconnectControl;
using PowerView.Service.EventHub;
using PowerView.Service.Mailer;
using PowerView.Service.Mappers;
using PowerView.Service.Controllers;
using PowerView.Service.Mqtt;
using PowerView.Service.Translations;

namespace PowerView.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IEventQueue, EventQueue>();
            serviceCollection.AddTransient<IReadingAccepter, ReadingAccepter>();
            serviceCollection.AddTransient<IHttpWebRequestFactory, HttpWebRequestFactory>();
            serviceCollection.AddTransient<IUrlProvider, UrlProvider>();

            // Translations
            serviceCollection.AddTransient<ITranslation, Translation>();

            // DisconnectControl
            serviceCollection.AddSingleton<DisconnectWarden>();
            serviceCollection.AddTransient<IDisconnectWarden>(sp => sp.GetRequiredService<DisconnectWarden>());
            serviceCollection.AddTransient<IDisconnectControlCache>(sp => sp.GetRequiredService<DisconnectWarden>());
            serviceCollection.AddTransient<IDisconnectCalculator, DisconnectCalculator>();

            // EventHub
            serviceCollection.AddSingleton<IHub, Hub>();
            serviceCollection.AddTransient<IIntervalTrigger, IntervalTrigger>();
            serviceCollection.AddTransient<IReadingPiper, ReadingPiper>();
            serviceCollection.AddTransient<ILocationResolver, LocationResolver>();
            serviceCollection.AddTransient<IMeterEventCoordinator, MeterEventCoordinator>();
            serviceCollection.AddTransient<IMeterEventDetector, MeterEventDetector>();
            serviceCollection.AddTransient<IMeterEventNotifier, MeterEventNotifier>();
            serviceCollection.AddTransient<IMqttPublisherFactory, MqttPublisherFactory>();
            serviceCollection.AddTransient<IMqttPublisher, MqttPublisher>();
            serviceCollection.AddTransient<IMqttMapper, MqttMapper>();
            serviceCollection.AddTransient<IDisconnectControlFactory, DisconnectControlFactory>();
            serviceCollection.AddTransient<IHealthCheck, HealthCheck>();

            // Mailer
            serviceCollection.AddTransient<IMailer, SmtpMailer>();
            serviceCollection.AddTransient<IMailMediator, MailMediator>();

            // Mappers
            serviceCollection.AddTransient<ILiveReadingMapper, LiveReadingMapper>();
            serviceCollection.AddTransient<ISerieMapper, SerieMapper>();
            serviceCollection.AddTransient<IDisconnectRuleMapper, DisconnectRuleMapper>();


            return serviceCollection;
        }
        /*      
                public static void Register(ContainerBuilder containerBuilder, Uri baseUri, Uri pvOutputAddStatusUri, string pvDeviceLabel, string pvDeviceId, string pvDeviceIdParam,
                                            string actualPowerP23L1Param, string actualPowerP23L2Param, string actualPowerP23L3Param)
                {
                    containerBuilder.RegisterType<ExitSignal>().As<IExitSignal>().As<IExitSignalProvider>().SingleInstance();




                    // Modules
                    containerBuilder.RegisterType<PvOutputFacadeModuleConfigProvider>().As<IPvOutputFacadeModuleConfigProvider>()
                      .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.Position == 0 && pi.ParameterType == typeof(Uri),
                        (pi, ctx) => pvOutputAddStatusUri
                      ))
                      .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.Position == 1 && pi.ParameterType == typeof(string),
                        (pi, ctx) => pvDeviceLabel
                      ))
                      .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.Position == 2 && pi.ParameterType == typeof(string),
                        (pi, ctx) => pvDeviceId
                      ))
                      .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.Position == 3 && pi.ParameterType == typeof(string),
                        (pi, ctx) => pvDeviceIdParam
                      ))
                      .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.Position == 4 && pi.ParameterType == typeof(string),
                        (pi, ctx) => actualPowerP23L1Param
                      ))
                      .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.Position == 5 && pi.ParameterType == typeof(string),
                        (pi, ctx) => actualPowerP23L2Param
                      ))
                      .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.Position == 6 && pi.ParameterType == typeof(string),
                        (pi, ctx) => actualPowerP23L3Param
                      ));


                    // "Root"
                    containerBuilder.RegisterType<UrlProvider>().As<IUrlProvider>()
                      .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.Position == 0 && pi.ParameterType == typeof(Uri),
                        (pi, ctx) => baseUri
                      ));
                    containerBuilder.RegisterType<ServiceHost>().As<IServiceHost>()
                      .WithParameter(new ResolvedParameter(
                        (pi, ctx) => pi.Position == 1 && pi.ParameterType == typeof(Uri),
                        (pi, ctx) => baseUri
                      )).SingleInstance();
                    containerBuilder.RegisterType<UsageMonitor>().As<IUsageMonitor>();
                }
        */
    }
}

