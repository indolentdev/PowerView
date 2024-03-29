using PowerView.Model;
using PowerView.Service;
using PowerView.Service.EnergiDataService;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace PowerView;

public class Startup
{
    public Startup(IConfigurationRoot configuration)
    {
        Configuration = configuration;
    }

    public IConfigurationRoot Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<ServiceOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.Configure<ServiceOptions>(Configuration.GetSection("Service"));
        services.AddOptions<PvOutputOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.Configure<PvOutputOptions>(Configuration.GetSection("Service:PvOutputFacade"));
        services.AddOptions<EnergiDataServiceClientOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.Configure<EnergiDataServiceClientOptions>(Configuration.GetSection("Service:EnergiDataService"));
        services.AddOptions<DatabaseOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.Configure<DatabaseOptions>(Configuration.GetSection("Database"));
        services.AddOptions<DatabaseBackupOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.Configure<DatabaseBackupOptions>(Configuration.GetSection("Database:Backup"));
        services.AddOptions<DatabaseCheckOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.Configure<DatabaseCheckOptions>(Configuration.GetSection("Database:Check"));
        services.AddOptions<DatabaseRegionOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.Configure<DatabaseRegionOptions>(Configuration.GetSection("Database:Region"));

        services.AddDatabase();
        services.AddServices();
        services.AddTransient<IDbSetup, DbSetup>();
        services.AddTransient<ILocationSetup, LocationSetup>();
        services.AddTransient<ITestDataSetup, TestDataSetup>();

        services.AddHttpClient();

        services.AddControllers(x => x.ModelBinderProviders.Insert(0, new PowerView.Service.Controllers.EmptyQueryStringBinderProvider()));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();

        //app.UseAuthorization();

        //          app.MapControllers();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UsePowerViewAngularStaticFiles();
    }
}
