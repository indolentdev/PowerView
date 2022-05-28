AppDomain.CurrentDomain.UnhandledException += PowerView.UnhandledExceptionLogger.UnhandledException;

var builder = WebApplication.CreateBuilder(args);

var startup = new PowerView.Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

PowerView.UnhandledExceptionLogger.SetApplicationLogger(app.Logger);

startup.Configure(app, app.Environment);

// Setup before launching the app fully
app.Services.GetRequiredService<PowerView.IDbSetup>().SetupDatabase();
app.Services.GetRequiredService<PowerView.ILocationSetup>().SetupLocation();

// Launch the app
var serviceOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<PowerView.Service.ServiceOptions>>();
app.Run(serviceOptions.Value.BaseUrl);

/*
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//app.UseFileServer(new FileServerOptions{ });
app.UseDefaultFiles();
var cacheMaxAgeOneWeek = (60 * 60 * 24 * 7).ToString();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "PowerView-Web")),
    RequestPath = "/web",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append(
             "Cache-Control", $"public, max-age={cacheMaxAgeOneWeek}");
    }
});

//app.UseAuthorization();

app.MapControllers();

app.Run();
*/