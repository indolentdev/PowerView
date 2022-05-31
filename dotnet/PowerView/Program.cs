AppDomain.CurrentDomain.UnhandledException += PowerView.UnhandledExceptionLogger.UnhandledException;

var options = new WebApplicationOptions { Args = args, WebRootPath = "PowerView-Web" };
var builder = WebApplication.CreateBuilder(options);

var startup = new PowerView.Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

PowerView.UnhandledExceptionLogger.SetApplicationLogger(app.Logger);

startup.Configure(app, app.Environment);

// Some setup before launching the app fully
app.Services.GetRequiredService<PowerView.IDbSetup>().SetupDatabase();
app.Services.GetRequiredService<PowerView.ILocationSetup>().SetupLocation();
app.Services.GetRequiredService<PowerView.ITestDataSetup>().SetupTestData(); // Will only run conditionally..

// Launch the app
var serviceOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<PowerView.Service.ServiceOptions>>();
app.Run(serviceOptions.Value.BaseUrl);
