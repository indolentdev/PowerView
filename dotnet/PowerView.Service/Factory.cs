/*
using System;
using Autofac;
using Autofac.Features.OwnedInstances;

namespace PowerView.Service
{
  public class Factory : IFactory
  {
    private readonly ILifetimeScope container;

    public Factory(ILifetimeScope container)
    {
      if (container == null) throw new ArgumentNullException("container");

      this.container = container;
    }

    public Owned<T> Create<T>()
    {
      var scope = container.BeginLifetimeScope(Model.ContainerConfiguration.RegisterIDbContextServiceSingleInstance);
      return new Owned<T>(scope.Resolve<T>(), scope);
    }

  }
}
*/