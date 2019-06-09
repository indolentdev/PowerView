using System;
using Nancy;
using PowerView.Model.Repository;

namespace PowerView.Service.Modules
{
  public class SettingsApplicationModule : CommonNancyModule
  {
    private readonly ILocationProvider locationProvider;

    public SettingsApplicationModule(ILocationProvider locationProvider)
      : base("/api/settings/application")
    {
      if (locationProvider == null) throw new ArgumentNullException("locationProvider");

      this.locationProvider = locationProvider;

      Get[""] = GetProps;
    }

    private dynamic GetProps(dynamic param)
    {
      var version = GetVersion();
      var cultureInfo = locationProvider.GetCultureInfo();
      var timeZone = locationProvider.GetTimeZone();

      var r = new { Version = version, Culture = cultureInfo.NativeName, TimeZone = timeZone.DisplayName };

      return Response.AsJson(r);
    }

    private string GetVersion()
    {
      return GetType().Assembly.GetName().Version.ToString(3);
    }

  }
}
