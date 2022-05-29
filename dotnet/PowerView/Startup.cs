using PowerView.Model;
using PowerView.Service;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
/*
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
*/
namespace PowerView
{
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
            services.AddOptions<DatabaseOptions>().ValidateDataAnnotations().ValidateOnStart();
            services.Configure<DatabaseOptions>(Configuration.GetSection("Database"));
            services.AddOptions<DatabaseBackupOptions>().ValidateDataAnnotations().ValidateOnStart();
            services.Configure<DatabaseBackupOptions>(Configuration.GetSection("Database:Backup"));
            services.AddOptions<DatabaseCheckOptions>().ValidateDataAnnotations().ValidateOnStart();
            services.Configure<DatabaseCheckOptions>(Configuration.GetSection("Database:Check"));

            // TODO
            services.AddOptions<Database2Options>().ValidateDataAnnotations().ValidateOnStart();
            services.Configure<Database2Options>(Configuration.GetSection("Database"));

            services.AddDatabase();
            services.AddServices();
            services.AddTransient<IDbSetup, DbSetup>();
            services.AddTransient<ILocationSetup, LocationSetup>();
            services.AddTransient<ITestDataSetup, TestDataSetup>();

            // Add services to the container.

            services.AddControllers();
            
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

            //app.UseFileServer(new FileServerOptions{ });
            /*            
                        app.UseDefaultFiles();
                        var cacheMaxAgeOneWeek = (60 * 60 * 24 * 7).ToString();
                        app.UseStaticFiles(new StaticFileOptions
                        {
                            FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "PowerView-Web")),
                            RequestPath = "/web",
                            OnPrepareResponse = ctx =>
                            {
                                ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cacheMaxAgeOneWeek}");
                            }
                        });
            */
            //app.UseAuthorization();

            //          app.MapControllers();

            app.UseRouting();
            app.UseEndpoints(x => x.MapControllers());
        }
    }
}
