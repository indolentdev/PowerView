using Microsoft.Extensions.DependencyInjection;
using PowerView.Model.Repository;

namespace PowerView.Model
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDbContextFactory, DbContextFactory>();
            serviceCollection.AddScoped<IDbContext>(sp => sp.GetRequiredService<IDbContextFactory>().CreateContext());

            serviceCollection.AddTransient<ILeakCharacteristicChecker, LeakCharacteristicChecker>();

            serviceCollection.AddTransient<IObisColorProvider, ObisColorProvider>();
            serviceCollection.AddSingleton<LocationContext>();
            serviceCollection.AddTransient<ILocationContext>(sp => sp.GetRequiredService<LocationContext>());
            serviceCollection.AddTransient<ILocationProvider, LocationProvider>();

            serviceCollection.AddTransient<IDbCheck, DbCheck>();
            serviceCollection.AddTransient<IDbBackup, DbBackup>();
            serviceCollection.AddTransient<IDbUpgrade, DbUpgrade>();
            serviceCollection.AddTransient<IDbMigrate, DbMigrate>();

            serviceCollection.AddTransient<ILiveReadingRepository, LiveReadingRepository>();
            serviceCollection.AddTransient<IProfileRepository, ProfileRepository>();
            serviceCollection.AddTransient<IReadingPipeRepository, ReadingPipeRepository>();
            serviceCollection.AddTransient<IReadingHistoryRepository, ReadingHistoryRepository>();
            serviceCollection.AddTransient<ILabelRepository, LabelRepository>();
            serviceCollection.AddTransient<ISettingRepository, SettingRepository>();
            serviceCollection.AddTransient<ISeriesNameRepository, SeriesNameRepository>();
            serviceCollection.AddTransient<ISeriesColorRepository, SeriesColorRepository>();
            serviceCollection.AddTransient<IProfileGraphRepository, ProfileGraphRepository>();
            serviceCollection.AddTransient<IEmailRecipientRepository, EmailRecipientRepository>();
            serviceCollection.AddTransient<IEmailMessageRepository, EmailMessageRepository>();
            serviceCollection.AddTransient<IMeterEventRepository, MeterEventRepository>();
            serviceCollection.AddTransient<IGaugeRepository, GaugeRepository>();
            serviceCollection.AddTransient<IDisconnectRuleRepository, DisconnectRuleRepository>();
            serviceCollection.AddTransient<IEnvironmentRepository, EnvironmentRepository>();
            serviceCollection.AddTransient<IExportRepository, ExportRepository>();
            serviceCollection.AddTransient<ICrudeDataRepository, CrudeDataRepository>();

            return serviceCollection;
        }

    }
}
