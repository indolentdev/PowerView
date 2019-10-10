using System;
using System.Globalization;

namespace PowerView.Service
{
  public class ApplicationConfiguration : IApplicationConfiguraiton
  {
    public void Setup(TimeZoneInfo timeZoneInfo, CultureInfo cultureInfo)
    {
      if (timeZoneInfo == null) throw new ArgumentNullException("timeZoneInfo");
      if (cultureInfo == null) throw new ArgumentNullException("cultureInfo");

      TimeZoneInfo = timeZoneInfo;
      CultureInfo = cultureInfo;
    }

    public TimeZoneInfo TimeZoneInfo { get; private set; }
    public CultureInfo CultureInfo { get; private set; }
  }
}
