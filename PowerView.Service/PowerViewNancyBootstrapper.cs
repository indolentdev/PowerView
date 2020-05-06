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
      nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("web", WebApplicationDirectory));
      nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("assets", Path.Combine(WebApplicationDirectory, "assets")));

      base.ConfigureConventions(nancyConventions);
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

