using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using PowerView.Model.Repository;

namespace PowerView.Service.EventHub
{
    public class LocationResolver : ILocationResolver
    {
        private static readonly Uri uri = new Uri("http://ip-api.com/json/");
        private readonly ILogger logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ISettingRepository settingRepository;

        public LocationResolver(ILogger<LocationResolver> logger, IHttpClientFactory httpClientFactory, ISettingRepository settingRepository)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
        }

        public void ResolveToDatabase()
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

            logger.LogInformation($"Fetched regional information from {uri} (time zone:{timeZoneId} and culture info:{cultureInfoName}). Saving to database.");

            if (!string.IsNullOrEmpty(timeZoneId))
            {
                settingRepository.Upsert(Settings.TimeZoneId, timeZoneId);
            }
            if (!string.IsNullOrEmpty(cultureInfoName))
            {
                settingRepository.Upsert(Settings.CultureInfoName, cultureInfoName);
            }
        }

        private static string GetCultureInfoName(LocationDto locationDto)
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

            return cultureInfos.First().Name;
        }

        private LocationDto GetLocationContentFromIpAddress()
        {
            LocationDto dto;

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            using var httpClient = httpClientFactory.CreateClient(nameof(LocationResolver));
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                using var response = httpClient.Send(request, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();
                dto = response.Content.ReadFromJsonAsync<LocationDto>().GetAwaiter().GetResult();
            }
            catch (TaskCanceledException e)
            {
                logger.LogInformation(e, $"Location resolve failed. Timeout. Request error. {request}");
                return null;
            }
            catch (HttpRequestException e)
            {
                logger.LogInformation(e, $"Location resolve failed. Request error. {request}");
                return null;
            }
            catch (JsonException e)
            {
                logger.LogWarning(e, "Location resolve failed. Respnose decoding error.");
                return null;
            }

            if (dto == null || !string.Equals(dto.status, "success", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning($"Location resolve failed. Respnose error. {dto?.status}");
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

