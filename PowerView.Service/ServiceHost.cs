using System;
using System.Reflection;
using Autofac;  
using log4net;
using Nancy.Hosting.Self;

namespace PowerView.Service
{
  internal class ServiceHost : IServiceHost
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly NancyHost host;
    private readonly Uri uri;

    public ServiceHost(ILifetimeScope container, Uri baseUri)
    {
      if (container == null) throw new ArgumentNullException("container");
      if (baseUri == null) throw new ArgumentNullException("baseUri");

      uri = new UriBuilder(baseUri).Uri;

      var bs = new PowerViewNancyBootstrapper(container);
      host = new NancyHost(bs, uri);
    }

    public void Start()
    {
      host.Start();
      log.InfoFormat("Service host started @ {0}", uri);
    }

    public void Dispose() 
    {
      Dispose(true);
      GC.SuppressFinalize(this);      
    }

    ~ServiceHost()
    {
      // Finalizer calls Dispose(false)
      Dispose(false);
    }

    private bool disposed;
    protected virtual void Dispose(bool disposing)
    {
      if (disposed)
      {
        return;
      }

      if (disposing) 
      {
        host.Stop();
        host.Dispose();
      }
      disposed = true;   
    }
  }
}

