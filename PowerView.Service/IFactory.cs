using Autofac.Features.OwnedInstances;

namespace PowerView.Service
{
  public interface IFactory
  {
    Owned<T> Create<T>();
  }
}

