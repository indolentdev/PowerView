using PowerView.Model;
using PowerView.Service.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace PowerView.Service.IntegrationTest;

/// <summary>
/// Enough of the real Startup class to get the tests going.
/// </summary>
public class TestStartup
{
    public TestStartup(IConfigurationRoot configuration)
    {
        Configuration = configuration;
    }

    public IConfigurationRoot Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers(x => x.ModelBinderProviders.Insert(0, new PowerView.Service.Controllers.EmptyQueryStringBinderProvider()))
            .AddApplicationPart(typeof(RootController).Assembly); // Ensure the test setup detects PowerView Controllers.
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
