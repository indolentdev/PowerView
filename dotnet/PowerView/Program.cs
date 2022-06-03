using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using PowerView;

LogManager.ThrowConfigExceptions = true;
var logger = LogManager.Setup().LoadConfigurationFromFile().GetCurrentClassLogger();
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
app.Services.GetRequiredService<PowerView.IDbSetup>().SetupDatabase();
app.Services.GetRequiredService<PowerView.ILocationSetup>().SetupLocation();
app.Services.GetRequiredService<PowerView.ITestDataSetup>().SetupTestData(); // Will only run conditionally..

// Launch the app
var serviceOptions = app.Services.GetRequiredService<IOptions<PowerView.Service.ServiceOptions>>();
app.Run(serviceOptions.Value.BaseUrl);

LogManager.Flush();
LogManager.Shutdown();
