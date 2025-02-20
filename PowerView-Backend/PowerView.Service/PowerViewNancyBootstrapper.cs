/*
using System;
using System.IO;
using Autofac;
using Nancy;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;

namespace PowerView.Service
{
  public class PowerViewNancyBootstrapper : AutofacNancyBootstrapper
  {
    internal const string WebApplicationDirectory = "PowerView-Web";

    private readonly ILifetimeScope applicationContainer;

    public PowerViewNancyBootstrapper(ILifetimeScope container)
    {
      if (container == null) throw new ArgumentNullException("container");

      applicationContainer = container.BeginLifetimeScope(Model.ContainerConfiguration.RegisterIDbContextServiceInstancePerRequest);
    }

    protected override void ApplicationStartup(ILifetimeScope container, Nancy.Bootstrapper.IPipelines pipelines)
    {
      base.ApplicationStartup(container, pipelines);
      StaticConfiguration.DisableErrorTraces = false;
      Nancy.Json.JsonSettings.MaxJsonLength = 1024*1024*2;
    }

    protected override ILifetimeScope GetApplicationContainer()
    {
      return applicationContainer;
    }

    protected override void ConfigureConventions(NancyConventions nancyConventions)
    {
      base.ConfigureConventions(nancyConventions);

      nancyConventions.StaticContentsConventions.Clear();

      var indexResponseBuilder = StaticContentConventionBuilder.AddFile("/web/index.html", Path.Combine(WebApplicationDirectory, "index.html"));
      Func<NancyContext, string, Response> indexResponseBuilderFunc = (context, root) =>
      {
        var response = indexResponseBuilder(context, root);
        if (response != null)
        {
          response.Headers.Add("Cache-control", "no-cache, no-store, must-revalidate");
          response.Headers.Add("Pragma", "no-cache");
        }
        return response;
      };
      nancyConventions.StaticContentsConventions.Add(indexResponseBuilderFunc);
      nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("web", WebApplicationDirectory));
      var assetsResponseBuilder = StaticContentConventionBuilder.AddDirectory("assets", Path.Combine(WebApplicationDirectory, "assets"));
      Func<NancyContext, string, Response> assetsResponseBuilderFunc = (context, root) =>
      {
        var response = assetsResponseBuilder(context, root);
        if (response != null)
        {
          response.Headers.Add("Cache-control", "no-cache, no-store, must-revalidate");
          response.Headers.Add("Pragma", "no-cache");
        }
        return response;
      };

      nancyConventions.StaticContentsConventions.Add(assetsResponseBuilderFunc);
    }

    // https://github.com/NancyFx/Nancy.Bootstrappers.Autofac
    // http://stackoverflow.com/questions/17325840/registering-startup-class-in-nancy-using-autofac-bootstrapper
//    protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
//    {
//      // No registrations should be performed in here, however you may
//      // resolve things that are needed during application startup.
//    }
//
//    protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
//    {
//      // Perform registration that should have an application lifetime
//      //var builder = new ContainerBuilder();
//      //builder.RegisterType<MyDependency>();
//      //builder.Update(container.ComponentRegistry);
//    }
//
//    protected override void ConfigureRequestContainer(ILifetimeScope container, NancyContext context)
//    {
//    }
//
//    protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
//    {
//      // No registrations should be performed in here, however you may
//      // resolve things that are needed during request startup.
//    }
  }
}

*/