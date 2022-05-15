/*
using System.IO;
using Nancy;

namespace PowerView.Service.Modules
{
  /// <summary>
  /// Complementary route resolving to 
  /// <see cref="PowerView.Service.PowerViewNancyBootstrapper.ConfigureConventions(Nancy.Conventions.NancyConventions)"/> for 
  /// perma linking in the angular web application
  /// </summary>
  public class WebModule : CommonNancyModule
  {
    public WebModule()
      :base("/web")
    {
      Get["/(.*)"] = _ => { return IndexPage(); };
      Get["/(.*)/(.*)"] = _ => { return IndexPage(); };
      Get["/(.*)/(.*)/(.*)"] = _ => { return IndexPage(); };
    }

    private dynamic IndexPage()
    {
      return Response.AsFile(Path.Combine(PowerViewNancyBootstrapper.WebApplicationDirectory,"index.html"));
    }

  }
}

*/
