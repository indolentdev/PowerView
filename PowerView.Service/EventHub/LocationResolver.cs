using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
  public class LocationResolver : ILocationResolver
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly Uri uri = new Uri("http://ip-api.com/json/");

    private readonly IHttpWebRequestFactory webRequestFactory;
    private readonly IFactory repositoryFactory;

    public LocationResolver(IHttpWebRequestFactory webRequestFactory, IFactory repositoryFactory)
    {
      if (webRequestFactory == null) throw new ArgumentNullException("webRequestFactory");
      if (repositoryFactory == null) throw new ArgumentNullException("repositoryFactory");

      this.webRequestFactory = webRequestFactory;
      this.repositoryFactory = repositoryFactory;
    }

    public void Resolve()
    {
      var locationDto = GetLocationContentFromIpAddress();

      if (locationDto == null)
      {
        return;
      }
      
      var timeZoneId = locationDto.timezone;
      var cultureInfoName = GetCultureInfoName(locationDto);

      if (string.IsNullOrEmpty(timeZoneId) && string.IsNullOrEmpty(cultureInfoName))
      {
        return;
      }

      using (var ownedRepo = repositoryFactory.Create<ISettingRepository>())
      {
        if (!string.IsNullOrEmpty(timeZoneId))
        {
          ownedRepo.Value.Upsert(Settings.TimeZoneId, timeZoneId);
        }
        if (!string.IsNullOrEmpty(cultureInfoName))
        {
          ownedRepo.Value.Upsert(Settings.CultureInfoName, cultureInfoName);
        }
      }

      log.InfoFormat("Resolved time zone:{0} and culture info:{1}", timeZoneId, cultureInfoName);
    }

    private string GetCultureInfoName(LocationDto locationDto)
    {
      var country = locationDto.country;
      if (string.IsNullOrEmpty(country))
      {
        return null;
      }

      var cultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                        .Where(ci => ci.EnglishName.Contains(country)).ToArray();

      if (cultureInfos.Length != 1)
      {
        return null;
      }

      return cultureInfos[0].Name;
    }

    private LocationDto GetLocationContentFromIpAddress()
    {
      string content;
      
      var request = webRequestFactory.Create(uri);
      request.Method = "GET";
      request.Timeout = 10 * 1000; // 10 sec.
      try
      {
        var response = request.GetResponse();
        using (var sr = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
        {
          content = sr.ReadToEnd();
        }
      }
      catch (HttpWebException e)
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Location resolve failed. Request error. {0}", uri);
        log.Info(msg, e);
        return null;
      }

      LocationDto dto;
      try
      {
        dto = JsonConvert.DeserializeObject<LocationDto>(content);
      }
      catch (JsonException e)
      {
        log.Warn("Location resolve failed. Respnose decoding error. Response:" + content, e);
        return null;
      }

      if (dto == null || !string.Equals(dto.status, "success", StringComparison.OrdinalIgnoreCase))
      {
        log.Warn("Location resolve failed. Respnose error. Response:" + content);
        return null;
      }

      return dto;
    }

    private class LocationDto
    {
      public string status { get; set; }
      public string timezone { get; set; }
      public string country { get; set; }
    }

  }
}

