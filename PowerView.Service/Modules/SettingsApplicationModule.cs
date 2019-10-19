using System;
using Nancy;
using PowerView.Model;

namespace PowerView.Service.Modules
{
  public class SettingsApplicationModule : CommonNancyModule
  {
    private readonly ILocationContext locationContext;

    public SettingsApplicationModule(ILocationContext locationContext)
      : base("/api/settings/application")
    {
      if (locationContext == null) throw new ArgumentNullException("locationContext");

      this.locationContext = locationContext;

      Get[""] = GetProps;
    }

    private dynamic GetProps(dynamic param)
    {
      var version = GetVersion();
      var cultureInfo = locationContext.CultureInfo;
      var timeZone = locationContext.TimeZoneInfo;

      var r = new { Version = version, Culture = cultureInfo.NativeName, TimeZone = timeZone.DisplayName };

      return Response.AsJson(r);
    }

    private string GetVersion()
    {
      return GetType().Assembly.GetName().Version.ToString(3);
    }

  }
}
