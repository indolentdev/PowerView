using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Nancy;
using log4net;

namespace PowerView.Service.Modules
{
  public abstract class CommonNancyModule : NancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    protected CommonNancyModule(string modulePath) : base(modulePath)
    {
      Before.AddItemToStartOfPipeline(LogDebugRequest);
      After.AddItemToEndOfPipeline(LogDebugResponse);
      OnError.AddItemToStartOfPipeline(LogError);
    }

    private static Response LogDebugRequest(NancyContext ctx)
    {
      const string format = "Request {0}";
      log.DebugFormat(CultureInfo.InvariantCulture, format, Rend(ctx.Request)); 
      return null;
    }

    private static void LogDebugResponse(NancyContext ctx)
    {
      const string format = "Response {0}";
      log.DebugFormat(CultureInfo.InvariantCulture, format, Rend(ctx.Response)); 
    }

    private static Response LogError(NancyContext ctx, Exception ex)
    {
      const string format = "Request fail {0}. Candidate response {1}";
      log.Error(string.Format(CultureInfo.InvariantCulture, format, Rend(ctx.Request), Rend(ctx.Response)), ex); 
      return null;
    }

    private static string Rend(Request request)
    { 
      const string format = "{0} {1} {2} {3}";
      return string.Format(CultureInfo.InvariantCulture, format, 
        request.UserHostAddress, request.Url, request.Method, request.Headers.ContentType);
    }

    private static string Rend(Response response)
    {
      if ( response == null ) return "Nothing (Internal Server Error [500]?)";

      const string format = "{0} {1}";
      return string.Format(CultureInfo.InvariantCulture, format, 
        response.StatusCode, response.ContentType);
    }

  }
}