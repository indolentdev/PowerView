using System.Collections.Generic;
using System.Linq;
using System.Text;
using PowerView.Model;
using PowerView.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace PowerView.Service.IntegrationTest;

internal class TestProgram
{
    static void Main(string[] args)
    {
        // Do enough to make WebApplicationFactory happy.
        var builder = WebApplication.CreateBuilder();
        var startup = new TestStartup(builder.Configuration);
        startup.ConfigureServices(builder.Services);
        var app = builder.Build();
        /*
                app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
                {
                    var sb = new StringBuilder();
                    var endpoints = endpointSources.SelectMany(es => es.Endpoints);
                    foreach (var endpoint in endpoints)
                    {
                        if (endpoint is RouteEndpoint routeEndpoint)
                        {
                            _ = routeEndpoint.RoutePattern.RawText;
                            _ = routeEndpoint.RoutePattern.PathSegments;
                            _ = routeEndpoint.RoutePattern.Parameters;
                            _ = routeEndpoint.RoutePattern.InboundPrecedence;
                            _ = routeEndpoint.RoutePattern.OutboundPrecedence;
                        }

                        var routeNameMetadata = endpoint.Metadata.OfType<Microsoft.AspNetCore.Routing.RouteNameMetadata>().FirstOrDefault();
                        _ = routeNameMetadata?.RouteName;

                        var httpMethodsMetadata = endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();
                        _ = httpMethodsMetadata?.HttpMethods; // [GET, POST, ...]
                    }
                });
        */
        startup.Configure(app, app.Environment);
        app.Run();
    }
}
