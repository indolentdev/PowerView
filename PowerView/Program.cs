using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Xml;
using Autofac;
using log4net;
using log4net.Config;
using PowerView.Configuration;
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

      MigrateConfig(configFileName);

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
        config.GetRegisterSection();
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
      var registerConfig = config.GetRegisterSection();

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
      PowerView.Model.ContainerConfiguration.Register(containerBuilder, dbConfig.Name.Value, minBackupInterval, maxBackupCount, integrityCheckCommandTimeout, configuredTimeZoneId, configuredCultureInfoName);

      ContainerConfiguration.Register(containerBuilder, serviceConfig.BaseUrl.GetValueAsUri(), serviceConfig.PvOutputFacade.PvOutputAddStatusUrl.GetValueAsUri(), 
        serviceConfig.PvOutputFacade.PvDeviceLabel.Value, serviceConfig.PvOutputFacade.PvDeviceSerialNumber.Value,
        serviceConfig.PvOutputFacade.PvDeviceSerialNumberParam.Value, serviceConfig.PvOutputFacade.ActualPowerP23L1Param.Value,
        serviceConfig.PvOutputFacade.ActualPowerP23L1Param.Value, serviceConfig.PvOutputFacade.ActualPowerP23L1Param.Value,
        registerConfig.Calculations.GetLabelObisCodeTemplates());

      var container = containerBuilder.Build();
      return container;
    }

    private static void Start(ILifetimeScope container, IPowerViewConfiguration config)
    {
      var dbConfig = config.GetDatabaseSection();

      using (var scope = container.BeginLifetimeScope(Model.ContainerConfiguration.RegisterIDbContextServiceSingleInstance))
      {
        SetupDatabase(scope, dbConfig);

        var locationResolver = scope.Resolve<ILocationResolver>();
        locationResolver.Resolve();

        GenerateTestDataIfDebug(scope);
      }

      var serviceHost = container.Resolve<IServiceHost>();
      serviceHost.Start();
    }

    private static void SetupDatabase(ILifetimeScope scope, DatabaseSection dbConfig)
    {
      var dbUpgradeNeeded = false;
      using(var scopeNested = scope.BeginLifetimeScope())
      {
        var dbCheck = scopeNested.Resolve<IDbCheck>();
        dbCheck.CheckDatabase();

        var dbUpgrade = scopeNested.Resolve<IDbUpgrade>();
        if ( dbConfig.Backup != null )
        {
          dbUpgradeNeeded = dbUpgrade.IsNeeded();
        }
      }

      var dbBackup = scope.Resolve<IDbBackup>();
      dbBackup.BackupDatabaseAsNeeded(dbUpgradeNeeded);

      using (var scopeNested = scope.BeginLifetimeScope())
      {
        var dbUpgrade = scopeNested.Resolve<IDbUpgrade>();
        dbUpgrade.ApplyUpdates();

        var dbMigrate = scopeNested.Resolve<IDbMigrate>();
        dbMigrate.Migrate();
      }
    }

    private static void GenerateTestDataIfDebug(ILifetimeScope scope)
    {
#if DEBUG
      var readingPipeRepository = scope.Resolve<IReadingPipeRepository>();

      for (var i = 0; i < 3; i++)
      {
        readingPipeRepository.PipeLiveReadingsToDayReadings(DateTime.UtcNow);
        readingPipeRepository.PipeDayReadingsToMonthReadings(DateTime.UtcNow);
      }

      new TestDataGenerator(scope).Invoke();
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
      log.InfoFormat(System.Globalization.CultureInfo.InvariantCulture, 
                     "Environment. CLR:{0}, HostName:{1}, OSVersion:{2}, ProcessorCount:{3}, IsLittleEndian:{4}", 
                     Environment.Version, Environment.MachineName, Environment.OSVersion, Environment.ProcessorCount, 
                     BitConverter.IsLittleEndian);
      log.InfoFormat(System.Globalization.CultureInfo.InvariantCulture, 
                     "Environment. 64BitOs:{0}, 64BitProcess:{1}, UserName:{2}", Environment.Is64BitOperatingSystem, Environment.Is64BitProcess, Environment.UserName);
      log.InfoFormat(System.Globalization.CultureInfo.InvariantCulture, 
                     "Environment. Culture:{0}, UICulture:{1}", System.Threading.Thread.CurrentThread.CurrentCulture, System.Threading.Thread.CurrentThread.CurrentUICulture);
    }

    private static void AppDomainCurrentDomainUnhandledException (object sender, UnhandledExceptionEventArgs e)
    {
      var exception = e.ExceptionObject as Exception;
      log.Fatal("Unhandled exception occurred. IsTerminating:" + e.IsTerminating, exception);
    }
  }
}
