/*
using System;
using System.Linq;
using Nancy;
using Microsoft.Extensions.Logging;
using System.Reflection;
using PowerView.Service.DisconnectControl;

namespace PowerView.Service.Modules
{
  public class DeviceOnDemandModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IDisconnectControlCache disconnectControlCache;

    public DeviceOnDemandModule(IDisconnectControlCache disconnectControlCache)
      :base("/api/devices")
    {
      if (disconnectControlCache == null) throw new ArgumentNullException("disconnectControlCache");

      this.disconnectControlCache = disconnectControlCache;

      Get["/ondemand"] = GetOnDemand;
    }

    private dynamic GetOnDemand(dynamic param)
    {
      if (!Request.Query.label.HasValue)
      {
        return Response.AsJson("Query parameter label is misisng.", HttpStatusCode.BadRequest);
      }
      string label = Request.Query.label;

      // DisconnectControl on demand
      var outputStatus = disconnectControlCache.GetOutputStatus(label);

      var items = outputStatus.Select(x => new
      {
        Label = x.Key.Label,
        ObisCode = x.Key.ObisCode.ToString(),
        Kind = "Method",
        Index = (x.Value ? 1 : 0) + 1 // Cosem DisconnectControl object. Method 1=remote_disconnect, Method 2=remote_reconnect
      }).ToList();
      return Response.AsJson(new { Items = items });
    }

  }
}
*/