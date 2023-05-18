using System.Reflection;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using PowerView;
using PowerView.Service;

System.Diagnostics.Debugger.Break();

string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
Directory.SetCurrentDirectory(exeDir);

LogManager.ThrowConfigExceptions = true;
var logSetup = LogManager.Setup().LoadConfigurationFromFile();
var logger = logSetup.GetLogger("PowerView.Program");

var logConfigWatcher = new FileSystemWatcher { Filter = "NLog.config", Path = exeDir, IncludeSubdirectories = false, EnableRaisingEvents = true };
logConfigWatcher.Changed += (sender, args) =>
{
    LogManager.Configuration = LogManager.Configuration.Reload();
    LogManager.ReconfigExistingLoggers();
};

AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    var exception = e.ExceptionObject as Exception;
    logger.Fatal(exception, $"Unhandled exception occurred. IsTerminating:{e.IsTerminating}");

    // Ensure we write this exception to disk as the process may terminate after the event handler.
    LogManager.Flush();
};

var startupType = typeof(Startup);
logger.Info("============================================================================================================================");
logger.Info($"Starting {startupType.Namespace}. Version:{startupType.Assembly.GetName().Version}, CurrentDirectory:{Environment.CurrentDirectory}, ProcessPath:{Environment.ProcessPath}");
logger.Info($"Environment. CLR:{Environment.Version}, OSVersion:{Environment.OSVersion}, ProcessorCount:{Environment.ProcessorCount}, IsLittleEndian:{BitConverter.IsLittleEndian}");
logger.Info($"Environment. MachineName:{Environment.MachineName}, 64BitOS:{Environment.Is64BitOperatingSystem}, 64BitProcess:{Environment.Is64BitProcess}");
logger.Info($"Environment. Culture:{Thread.CurrentThread.CurrentCulture}, UICulture:{Thread.CurrentThread.CurrentUICulture}, TimeZone:{TimeZoneInfo.Local}");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, WebRootPath = "PowerView-Web" });

builder.Logging.ClearProviders().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Host.UseNLog();

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

// Some setup before launching the app fully
app.Services.GetRequiredService<IDbSetup>().SetupDatabase();
app.Services.GetRequiredService<ILocationSetup>().SetupLocation();
app.Services.GetRequiredService<ITestDataSetup>().SetupTestData(); // Will only run conditionally..

app.Services.GetService<PowerView.Service.EventHub.IHub>(); // Init the hub at startup

// Launch the app
var serviceOptions = app.Services.GetRequiredService<IOptions<ServiceOptions>>();
app.Run(serviceOptions.Value.BaseUrl);

// Cleanup
logConfigWatcher.Dispose();
LogManager.Flush();
LogManager.Shutdown();
