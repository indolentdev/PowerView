using System;
using Microsoft.Extensions.DependencyInjection;
using PowerView.Service.DisconnectControl;
using PowerView.Service.EnergiDataService;
using PowerView.Service.EventHub;
using PowerView.Service.Mailer;
using PowerView.Service.Mappers;
using PowerView.Service.Controllers;
using PowerView.Service.Mqtt;
using PowerView.Service.Translations;

namespace PowerView.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IEventQueue, EventQueue>();
        serviceCollection.AddTransient<IReadingAccepter, ReadingAccepter>();
        serviceCollection.AddTransient<IUrlProvider, UrlProvider>();

        // Translations
        serviceCollection.AddTransient<ITranslation, Translation>();

        // DisconnectControl
        serviceCollection.AddSingleton<DisconnectWarden>();
        serviceCollection.AddTransient<IDisconnectWarden>(sp => sp.GetRequiredService<DisconnectWarden>());
        serviceCollection.AddTransient<IDisconnectControlCache>(sp => sp.GetRequiredService<DisconnectWarden>());
        serviceCollection.AddTransient<IDisconnectCalculator, DisconnectCalculator>();

        // EnergiDataService
        serviceCollection.AddTransient<IEnergiDataServiceClient, EnergiDataServiceClient>();

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

        return serviceCollection;
    }
}
