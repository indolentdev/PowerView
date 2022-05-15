/*
using System;
using Autofac;
using Autofac.Core;
using PowerView.Service.DisconnectControl;
using PowerView.Service.EventHub;
using PowerView.Service.Mailer;
using PowerView.Service.Mappers;
using PowerView.Service.Modules;
using PowerView.Service.Mqtt;
using PowerView.Service.Translations;

namespace PowerView.Service
{
  public static class ContainerConfiguration
  {
    public static void Register(ContainerBuilder containerBuilder, Uri baseUri, Uri pvOutputAddStatusUri, string pvDeviceLabel, string pvDeviceId, string pvDeviceIdParam,
                                string actualPowerP23L1Param, string actualPowerP23L2Param, string actualPowerP23L3Param)
    {
      containerBuilder.RegisterType<ExitSignal>().As<IExitSignal>().As<IExitSignalProvider>().SingleInstance();

      // DisconnectControl
      containerBuilder.RegisterType<DisconnectWarden>().As<IDisconnectWarden>().As<IDisconnectControlCache>().SingleInstance();
      containerBuilder.RegisterType<DisconnectCalculator>().As<IDisconnectCalculator>();

      // EventHub
      containerBuilder.RegisterType<Hub>().As<IHub>().SingleInstance();
      containerBuilder.RegisterType<IntervalTrigger>().As<IIntervalTrigger>();
      containerBuilder.RegisterType<ReadingPiper>().As<IReadingPiper>();
      containerBuilder.RegisterType<LocationResolver>().As<ILocationResolver>();  
      containerBuilder.RegisterType<Factory>().As<IFactory>();
      containerBuilder.RegisterType<MeterEventCoordinator>().As<IMeterEventCoordinator>();
      containerBuilder.RegisterType<MeterEventDetector>().As<IMeterEventDetector>();
      containerBuilder.RegisterType<MeterEventNotifier>().As<IMeterEventNotifier>();
      containerBuilder.RegisterType<Tracker>().As<ITracker>();
      containerBuilder.RegisterType<MqttPublisherFactory>().As<IMqttPublisherFactory>();
      containerBuilder.RegisterType<MqttPublisher>().As<IMqttPublisher>();
      containerBuilder.RegisterType<MqttMapper>().As<IMqttMapper>();
      containerBuilder.RegisterType<DisconnectControlFactory>().As<IDisconnectControlFactory>();
      containerBuilder.RegisterType<HealthCheck>().As<IHealthCheck>();

      // Mailer
      containerBuilder.RegisterType<SmtpMailer>().As<IMailer>();
      containerBuilder.RegisterType<MailMediator>().As<IMailMediator>();

      // Mappers
      containerBuilder.RegisterType<LiveReadingMapper>().As<ILiveReadingMapper>();
      containerBuilder.RegisterType<SerieMapper>().As<ISerieMapper>();
      containerBuilder.RegisterType<DisconnectRuleMapper>().As<IDisconnectRuleMapper>();

      // Modules
      containerBuilder.RegisterType<ReadingAccepter>().As<IReadingAccepter>();
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

      // Translations
      containerBuilder.RegisterType<Translation>().As<ITranslation>();

      // "Root"
      containerBuilder.RegisterType<UrlProvider>().As<IUrlProvider>()
        .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.Position == 0 && pi.ParameterType == typeof(Uri),
          (pi, ctx) => baseUri
        ));
      containerBuilder.RegisterType<HttpWebRequestFactory>().As<IHttpWebRequestFactory>();
      containerBuilder.RegisterType<ServiceHost>().As<IServiceHost>()
        .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.Position == 1 && pi.ParameterType == typeof(Uri),
          (pi, ctx) => baseUri
        )).SingleInstance();
      containerBuilder.RegisterType<UsageMonitor>().As<IUsageMonitor>();
    }
  }
}

*/