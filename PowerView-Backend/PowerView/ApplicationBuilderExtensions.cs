using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace PowerView
{
    public static class ApplicationBuilderExtensions
    {

        internal static void UsePowerViewAngularStaticFiles(this IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ApplicationBuilderExtensions));

            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            var cacheMaxAgeOneDay = (int)TimeSpan.FromDays(1).TotalSeconds;

            if (Directory.Exists(env.WebRootPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = env.WebRootFileProvider,
                    RequestPath = "/web",
                    OnPrepareResponse = ctx =>
                    {
                        if (ctx.File.Name.Equals("index.html", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
                        }
                        else
                        {
                            ctx.Context.Response.Headers.Append("Cache-Control", $"private, max-age={cacheMaxAgeOneDay}");
                        }
                    }
                });
            }
            else
            {
                if ( string.Equals(env.EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ApplicationException($"WebRootPath does not exist. {env.WebRootPath}");
                }
                logger.LogWarning($"WebRootPath does not exist. Skipping spa main static files. {env.WebRootPath}");
            }

            var assets = Path.Combine(env.WebRootPath, "assets");
            if (Directory.Exists(assets))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(assets),
                    RequestPath = "/assets",
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", $"private, max-age={cacheMaxAgeOneDay}");
                    }
                });
            }
            else
            {
                if (string.Equals(env.EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ApplicationException($"Web assets path does not exist. {assets}");
                }
                logger.LogWarning($"Web assets path does not exist. Skipping spa assets static files. {assets}");
            }


            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new FaviconPhysicalFileProvider(env.WebRootFileProvider, logger),
                RequestPath = "/favicon.ico",
                ServeUnknownFileTypes = true,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"private, max-age={cacheMaxAgeOneDay}");
                }
            });

        }

        private class FaviconPhysicalFileProvider : IFileProvider
        {
            private const string FileName = "favicon.ico";
            private readonly IFileProvider backing;
            private readonly ILogger logger;

            public FaviconPhysicalFileProvider(IFileProvider fileProvider, ILogger logger)
            {
                backing = fileProvider;
                this.logger = logger;
            }

            public IDirectoryContents GetDirectoryContents(string subpath)
            {
                return NotFoundDirectoryContents.Singleton;
            }

            public IFileInfo GetFileInfo(string subpath)
            {
                if (!string.IsNullOrEmpty(subpath))
                {
                    return new NotFoundFileInfo(subpath);
                }

                return backing.GetFileInfo(FileName);
            }

            public IChangeToken Watch(string filter)
            {
                throw new NotImplementedException();
            }
        }
    }
}
