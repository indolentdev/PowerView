using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
  public class LocationResolver : ILocationResolver
  {
    private static readonly Uri uri = new Uri("http://ip-api.com/json/");
    private readonly ILogger logger;
    private readonly IHttpWebRequestFactory webRequestFactory;
    private readonly ISettingRepository settingRepository;

    public LocationResolver(ILogger<LocationResolver> logger, IHttpWebRequestFactory webRequestFactory, ISettingRepository settingRepository)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.webRequestFactory = webRequestFactory ?? throw new ArgumentNullException(nameof(webRequestFactory));
      this.settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
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

      if (!string.IsNullOrEmpty(timeZoneId))
      {
        settingRepository.Upsert(Settings.TimeZoneId, timeZoneId);
      }
      if (!string.IsNullOrEmpty(cultureInfoName))
      {
        settingRepository.Upsert(Settings.CultureInfoName, cultureInfoName);
      }

      logger.LogDebug("Resolved time zone:{0} and culture info:{1}", timeZoneId, cultureInfoName);
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

      if (cultureInfos.Length == 0)
      {
        return null;
      }

      return cultureInfos.First().Name; // Hmm.. Here we assume the first CultureInfo is the best one .. but that may not be the case....
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
        logger.LogInformation(e, msg);
        return null;
      }

      LocationDto dto;
      try
      {
        dto = JsonConvert.DeserializeObject<LocationDto>(content);
      }
      catch (JsonException e)
      {
        logger.LogWarning(e, "Location resolve failed. Respnose decoding error. Response:" + content);
        return null;
      }

      if (dto == null || !string.Equals(dto.status, "success", StringComparison.OrdinalIgnoreCase))
      {
        logger.LogWarning("Location resolve failed. Respnose error. Response:" + content);
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

