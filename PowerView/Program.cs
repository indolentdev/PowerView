using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Xml;
using Autofac;
using log4net;
using log4net.Config;
using PowerView.Configuration;
using PowerView.Configuration.Migration;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service;
using PowerView.Service.EventHub;

namespace PowerView
{
  class MainClass
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public static void Main(string[] args)
    {
      SetCurrentDirectory();
      const string configFileName = "PowerView.exe.config";
      XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(configFileName));
      AppDomain.CurrentDomain.UnhandledException += AppDomainCurrentDomainUnhandledException;

      StartLog(args, configFileName);

      MigrateConfig(configFileName); // The old migration..
      var configMigrater = new ConfigMigrater();
      configMigrater.Migrate(configFileName);

      IContainer container = null;
      var config = GetConfiguration();
      if (config != null)
      {
        container = ConfigureContainer(config);

        Start(container, config);
      }

      WaitForExit(container);

      log.Info("Closing application");
      if ( container != null )
      {
        container.Dispose();
      }
      LogManager.Shutdown();
    }

    private static void SetCurrentDirectory()
    {
      var asmDir = System.IO.Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
      Environment.CurrentDirectory = asmDir;
    }

    private static void StartLog(Array args, string configFileName)
    {
      LogStart(configFileName);
      if (args.Length > 0)
      {
        log.Warn("PowerView does not support command line arguments. The passed command line arguments are ignored.");
      }
    }

    private static void MigrateConfig(string configFileName)
    {
      var config = System.IO.File.ReadAllText(configFileName);
      var modifiedConfig = config.Replace("PvDeviceSerialNumber", "PvDeviceId");
      if (!modifiedConfig.Equals(config, StringComparison.Ordinal))
      {
        log.InfoFormat("Migrating config file");
        System.IO.File.WriteAllText(configFileName, modifiedConfig);
      }

      XmlDocument document = new XmlDocument();
      document.PreserveWhitespace = true;
      document.Load(configFileName);

      const string nodeXPath = "/configuration/Register/Mappings";
      XmlNode node = document.SelectSingleNode(nodeXPath);
      if (node == null)
      {
        return;
      }

      log.InfoFormat("Migrating config file");
      node.RemoveAll();
      XmlNode parentnode = node.ParentNode;
      parentnode.RemoveChild(node);

      document.Save(configFileName);
    }

    private static IPowerViewConfiguration GetConfiguration()
    {
      var config = new PowerViewConfiguration();
      try
      { // Trigger validation
        config.GetServiceSection();
        config.GetDatabaseSection();
      }
      catch (ConfigurationErrorsException e)
      {
        log.Error(string.Format("Configuration error detected: {0}", e.Message), e);
        return null;
      }
      return config;
    }

    private static IContainer ConfigureContainer(IPowerViewConfiguration config)
    {
      var serviceConfig = config.GetServiceSection();
      var dbConfig = config.GetDatabaseSection();

      var containerBuilder = new ContainerBuilder();

      var minBackupInterval = TimeSpan.MinValue;
      var maxBackupCount = int.MaxValue;
      if (dbConfig.HasBackupElement)
      {
        minBackupInterval = TimeSpan.FromDays(dbConfig.Backup.MinimumIntervalDays.GetValueAsInt());
        maxBackupCount = dbConfig.Backup.MaximumCount.GetValueAsInt();
      }
      var integrityCheckCommandTimeout = dbConfig.IntegrityCheckCommandTimeout.GetValueAsInt();
      var configuredTimeZoneId = dbConfig.TimeZone == null ? (string)null : (string.IsNullOrEmpty(dbConfig.TimeZone.Value) ? (string)null : dbConfig.TimeZone.Value);
      var configuredCultureInfoName = dbConfig.CultureInfo == null ? (string)null : (string.IsNullOrEmpty(dbConfig.CultureInfo.Value) ? (string)null : dbConfig.CultureInfo.Value);
      Model.ContainerConfiguration.Register(containerBuilder, dbConfig.Name.Value, minBackupInterval, maxBackupCount, integrityCheckCommandTimeout, configuredTimeZoneId, configuredCultureInfoName);

      Service.ContainerConfiguration.Register(containerBuilder, serviceConfig.BaseUrl.GetValueAsUri(), serviceConfig.PvOutputFacade.PvOutputAddStatusUrl.GetValueAsUri(), 
        serviceConfig.PvOutputFacade.PvDeviceLabel.Value, serviceConfig.PvOutputFacade.PvDeviceId.Value,
        serviceConfig.PvOutputFacade.PvDeviceIdParam.Value, serviceConfig.PvOutputFacade.ActualPowerP23L1Param.Value,
        serviceConfig.PvOutputFacade.ActualPowerP23L1Param.Value, serviceConfig.PvOutputFacade.ActualPowerP23L1Param.Value);

      var container = containerBuilder.Build();
      return container;
    }

    private static void Start(ILifetimeScope container, IPowerViewConfiguration config)
    {
      var dbConfig = config.GetDatabaseSection();

      using (var startUpScope = container.BeginLifetimeScope())
      {
        SetupDatabase(startUpScope, dbConfig);

        LocationSetup(startUpScope);

        GenerateTestDataIfDebug(startUpScope);
      }

      var serviceHost = container.Resolve<IServiceHost>();
      serviceHost.Start();
    }

    private static void LocationSetup(ILifetimeScope startUpScope)
    {
      using (var locationSetupScope = startUpScope.BeginLifetimeScope(Model.ContainerConfiguration.RegisterIDbContextServiceSingleInstance))
      {
        locationSetupScope.Resolve<ILocationResolver>().Resolve();
        var locationProvider = locationSetupScope.Resolve<ILocationProvider>();
        var timeZoneInfo = locationProvider.GetTimeZone();
        var cultureInfo = locationProvider.GetCultureInfo();
        startUpScope.Resolve<LocationContext>().Setup(timeZoneInfo, cultureInfo);
        log.InfoFormat(CultureInfo.InvariantCulture, "Applying time zone {0}:{1} and culture info {2}:{3}",
          timeZoneInfo.Id, timeZoneInfo.DisplayName, cultureInfo.Name, cultureInfo.EnglishName);
      }
    }

    private static void SetupDatabase(ILifetimeScope startUpScope, DatabaseSection dbConfig)
    {
      var dbUpgradeNeeded = false;
      using(var scopeNested = startUpScope.BeginLifetimeScope(Model.ContainerConfiguration.RegisterIDbContextServiceSingleInstance))
      {
        var envRepository = scopeNested.Resolve<IEnvironmentRepository>(); 
        var sqliteVersion = envRepository.GetSqliteVersion();
        log.InfoFormat("SQLite version:{0}", sqliteVersion);

        var dbCheck = scopeNested.Resolve<IDbCheck>();
        dbCheck.CheckDatabase();

        var dbUpgrade = scopeNested.Resolve<IDbUpgrade>();
        if ( dbConfig.HasBackupElement )
        {
          dbUpgradeNeeded = dbUpgrade.IsNeeded();
        }
      }

      var dbBackup = startUpScope.Resolve<IDbBackup>();
      dbBackup.BackupDatabaseAsNeeded(dbUpgradeNeeded);

      using (var scopeNested = startUpScope.BeginLifetimeScope(Model.ContainerConfiguration.RegisterIDbContextServiceSingleInstance))
      {
        var dbUpgrade = scopeNested.Resolve<IDbUpgrade>();
        dbUpgrade.ApplyUpdates();

        var dbMigrate = scopeNested.Resolve<IDbMigrate>();
        dbMigrate.Migrate();
      }
    }

    private static void GenerateTestDataIfDebug(ILifetimeScope startUpScope)
    {
#if DEBUG
      using (var genTestDataScope = startUpScope.BeginLifetimeScope(Model.ContainerConfiguration.RegisterIDbContextServiceSingleInstance))
      {
        var readingPiper = genTestDataScope.Resolve<IReadingPiper>();
        var utcNow = DateTime.UtcNow + TimeSpan.FromDays(1);
        readingPiper.PipeLiveReadings(utcNow);
        readingPiper.PipeDayReadings(utcNow);
        readingPiper.PipeMonthReadings(utcNow);

        new TestDataGenerator(genTestDataScope).Invoke();
      }
#endif
    }

    private static void WaitForExit(IContainer container)
    {
      var exitSignal = container.Resolve<IExitSignal>();
      using (var waitHandle = new AutoResetEvent(false))
      {
        exitSignal.Exit += (sender, e) =>
        {
          log.InfoFormat("Received exit signal.");
          waitHandle.Set();
        };

        var exitSignalProvider = container.Resolve<IExitSignalProvider>();
        ProcessStopper.ProcessStopperFactory.Create(Environment.OSVersion.ToString(), exitSignalProvider).WireUp();

        waitHandle.WaitOne(Timeout.Infinite);
      }
    }

    private static void LogStart(string configFileName)
    {
      var mainClassType = typeof(MainClass);
      log.Info("============================================================================================================================");
      log.InfoFormat("Starting {0}. Version:{1}, CurrentDirectory:{2}, Config:{3}", 
                     mainClassType.Namespace, mainClassType.Assembly.GetName().Version, Environment.CurrentDirectory, configFileName);
      log.InfoFormat(CultureInfo.InvariantCulture, 
                     "Environment. CLR:{0}, HostName:{1}, OSVersion:{2}, ProcessorCount:{3}, IsLittleEndian:{4}", 
                     Environment.Version, Environment.MachineName, Environment.OSVersion, Environment.ProcessorCount, BitConverter.IsLittleEndian);
      log.InfoFormat(CultureInfo.InvariantCulture, 
                     "Environment. 64BitOs:{0}, 64BitProcess:{1}, UserName:{2}", 
                      Environment.Is64BitOperatingSystem, Environment.Is64BitProcess, Environment.UserName);
      log.InfoFormat(CultureInfo.InvariantCulture, "Environment. Culture:{0}, UICulture:{1}, MonoRuntimeVersion:{2}, RaspberryPiRevision:{3}", 
        Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture, EnvironmentHelper.GetMonoRuntimeVersion(), EnvironmentHelper.GetCpuInfoRevision());
    }

    private static void AppDomainCurrentDomainUnhandledException (object sender, UnhandledExceptionEventArgs e)
    {
      var exception = e.ExceptionObject as Exception;
      log.Fatal("Unhandled exception occurred. IsTerminating:" + e.IsTerminating, exception);
    }
  }
}
