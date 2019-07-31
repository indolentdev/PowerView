using System;
using Autofac;
using Autofac.Core;
using PowerView.Model.Repository;

namespace PowerView.Model
{
  public static class ContainerConfiguration
  {
    public static void Register(ContainerBuilder containerBuilder, string dbName, TimeSpan minimumTimeSpan, int maxBackupCount, int integrityCheckCommandTimeout, string configuredTimeZoneId, string configuredCultureInfoName)
    {
      containerBuilder.RegisterInstance<IDbContextFactory>(new DbContextFactory(dbName));

      containerBuilder.RegisterType<LiveReadingRepository>().As<ILiveReadingRepository>();
      containerBuilder.RegisterType<ProfileRepository>().As<IProfileRepository>();
      containerBuilder.RegisterType<LabelSeriesRepository>().As<ILabelSeriesRepository>();
      containerBuilder.RegisterType<ReadingPipeRepository>().As<IReadingPipeRepository>();
      containerBuilder.RegisterType<SettingRepository>().As<ISettingRepository>();
      containerBuilder.RegisterType<SeriesNameRepository>().As<ISeriesNameRepository>();
      containerBuilder.RegisterType<SeriesColorRepository>().As<ISeriesColorRepository>();
      containerBuilder.RegisterType<ProfileGraphRepository>().As<IProfileGraphRepository>();
      containerBuilder.RegisterType<EmailRecipientRepository>().As<IEmailRecipientRepository>();
      containerBuilder.RegisterType<EmailMessageRepository>().As<IEmailMessageRepository>();
      containerBuilder.RegisterType<MeterEventRepository>().As<IMeterEventRepository>();
      containerBuilder.RegisterType<GaugeRepository>().As<IGaugeRepository>();
      containerBuilder.RegisterType<DisconnectRuleRepository>().As<IDisconnectRuleRepository>();

      containerBuilder.RegisterType<ObisColorProvider>().As<IObisColorProvider>();
      containerBuilder.RegisterType<DbCheck>().As<IDbCheck>()
        .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.Position == 1 && pi.ParameterType == typeof(int),
          (pi, ctx) => integrityCheckCommandTimeout
        ));
      containerBuilder.RegisterType<DbUpgrade>().As<IDbUpgrade>();
      containerBuilder.RegisterType<DbMigrate>().As<IDbMigrate>();
      containerBuilder.RegisterType<DbBackup>().As<IDbBackup>()
        .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.Position == 0 && pi.ParameterType == typeof(string),
          (pi, ctx) => dbName
        ))
        .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.Position == 1 && pi.ParameterType == typeof(TimeSpan),
          (pi, ctx) => minimumTimeSpan
        ))
        .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.Position == 2 && pi.ParameterType == typeof(int),
          (pi, ctx) => maxBackupCount
        ));
      containerBuilder.RegisterType<LocationProvider>().As<ILocationProvider>()
        .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.Position == 1 && pi.ParameterType == typeof(string),
          (pi, ctx) => configuredTimeZoneId
        ))
        .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.Position == 2 && pi.ParameterType == typeof(string),
          (pi, ctx) => configuredCultureInfoName
        ));
      containerBuilder.RegisterType<TimeConverter>().As<ITimeConverter>();
    }

    public static void RegisterIDbContextServiceSingleInstance(ContainerBuilder builder)
    {
      builder.Register(ctx =>
      {
        var dbContextFactory = ctx.Resolve<IDbContextFactory>();
        return dbContextFactory.CreateContext();
      }).As<IDbContext>().SingleInstance();
    }

    public static void RegisterIDbContextServiceInstancePerRequest(ContainerBuilder builder)
    {
      builder.Register(ctx =>
      {
        var dbContextFactory = ctx.Resolve<IDbContextFactory>();
        return dbContextFactory.CreateContext();
      }).As<IDbContext>().InstancePerRequest();
    }

  }
}