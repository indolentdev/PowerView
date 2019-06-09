using Nancy;
using Nancy.Responses;

namespace PowerView.Service.Modules
{
  public class RootModule : CommonNancyModule
  {
    public RootModule()
      :base("/")
    {
      Get[""] = RedirectToIndexHtml;
      Get["/index.html"] = RedirectToIndexHtml;
    }

    private dynamic RedirectToIndexHtml(dynamic ctx)
    {
      return Response.AsRedirect("web/index.html", RedirectResponse.RedirectType.SeeOther);
    }
  }
}

