using System;
using Microsoft.Extensions.DependencyInjection;

namespace PowerView.Service.EventHub
{
  public interface IMeterEventCoordinator
  {
    void DetectAndNotify(IServiceScope serviceScope, DateTime dateTime);
  }
}

