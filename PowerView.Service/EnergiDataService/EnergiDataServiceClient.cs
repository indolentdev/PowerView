using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace PowerView.Service.EnergiDataService;

public class EnergiDataServiceClient : IEnergiDataServiceClient
{
    private readonly EnergiDataServiceClientOptions options;
    private readonly IHttpClientFactory httpClientFactory;

    public EnergiDataServiceClient(IOptions<EnergiDataServiceClientOptions> options, IHttpClientFactory httpClientFactory)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        if (options.Value == null) throw new ArgumentException("Options value must not be null", nameof(options));
        this.options = options.Value;
    }

    public async Task<IList<KwhAmount>> GetElectricityAmounts(DateTime start, TimeSpan timeSpan, string priceArea)
    {
        if (start.Kind != DateTimeKind.Utc) throw new ArgumentOutOfRangeException(nameof(start), "Must be UTC");
        if (timeSpan <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeSpan), "Must not be negative or zero");
        if (timeSpan > TimeSpan.FromDays(7)) throw new ArgumentOutOfRangeException(nameof(timeSpan), "Must not be greater than 7 days");
        if (priceArea == null) throw new ArgumentNullException(nameof(priceArea));
        if (string.IsNullOrEmpty(priceArea)) throw new ArgumentException(nameof(priceArea), "Must not be empty string");

        var dto = await GetElSpotPrices(start, timeSpan, priceArea);
        Validate(dto);
        var result = dto.Records.Select(x => x.GetKwhAmount()).ToList();
        return result;
    }

    private async Task<ElSpotPricesDto> GetElSpotPrices(DateTime start, TimeSpan timeSpan, string priceArea)
    {
        var format = "yyyy-MM-ddTHH:mm";
        var periodStart = start.ToString(format, CultureInfo.InvariantCulture);
        var periodEnd = (start + timeSpan).ToString(format, CultureInfo.InvariantCulture);
        var filter = "{\"PriceArea\":[\"%PriceArea%\"]}".Replace("%PriceArea%", priceArea);
        const string timezone = "UTC";
        const string sort = "HourUTC";
        var url = UrlHelper.EncodeUrlParameters(options.BaseUrl, $"elspotprices?start={periodStart}&end={periodEnd}&filter={filter}&timezone={timezone}&sort={sort}");
        using (var httpClient = httpClientFactory.CreateClient())
        {
            httpClient.Timeout = options.RequestTimeout;
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.StatusCode == HttpStatusCode.TooManyRequests) // 429
                {
                    return new ElSpotPricesDto { Records = new List<ElSpotPriceRecordDto>() }; // Backoff if the server doesn't handle the request right now.
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ElSpotPricesDto>();
            }
            catch (TaskCanceledException e)
            {
                throw new EnergiDataServiceClientException($"Energi Data Service http request timed out. {request}", e);
            }
            catch (HttpRequestException e)
            {
                throw new EnergiDataServiceClientException($"Energi Data Service http request failed. {request}", e);
            }
            catch (JsonException e)
            {
                throw new EnergiDataServiceClientException($"Energi Data Service response deserialization failed. {request}", e);
            }
       }
    }

    private void Validate(ElSpotPricesDto dto)
    {
        try
        {
            Validator.ValidateObject(dto, new ValidationContext(dto), true);
            foreach (var record in dto.Records)
            {
                Validator.ValidateObject(record, new ValidationContext(record), true);
            }
        }
        catch (ValidationException e)
        {
            throw new EnergiDataServiceClientException("Energi Data Service response validation failed.", e);
        }
    }

}