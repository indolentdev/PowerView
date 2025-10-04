using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using PowerView.Model;

namespace PowerView.Service.EnergiDataService;

public class EnergiDataServiceClient : IEnergiDataServiceClient
{
    private readonly EnergiDataServiceClientOptions options;
    private readonly IHttpClientFactory httpClientFactory;

    public EnergiDataServiceClient(IOptions<EnergiDataServiceClientOptions> options, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        if (options.Value == null) throw new ArgumentException("Options value must not be null", nameof(options));
        this.options = options.Value;
    }

    public async Task<IList<KwhAmount>> GetElectricityAmounts(DateTime start, TimeSpan timeSpan, string priceArea)
    {
        ArgCheck.ThrowIfNotUtc(start);
        if (timeSpan <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeSpan), "Must not be negative or zero");
        if (timeSpan > TimeSpan.FromDays(7)) throw new ArgumentOutOfRangeException(nameof(timeSpan), "Must not be greater than 7 days");
        ArgCheck.ThrowIfNullOrEmpty(priceArea);

        var result = await GetKwhAmounts(start, timeSpan, priceArea);
        return result;
    }

    private async Task<IList<KwhAmount>> GetKwhAmounts(DateTime start, TimeSpan timeSpan, string priceArea)
    {
        var useDayAheadPrices = start >= new DateTime(2025, 9, 30, 22, 0, 0, DateTimeKind.Utc); // 1st of October 2025 00:00 DK (summer) time.
        var dataSet = "dayaheadprices";
        if (!useDayAheadPrices)
        {
            dataSet = "elspotprices";
        }

        var format = "yyyy-MM-ddTHH:mm";
        var periodStart = start.ToString(format, CultureInfo.InvariantCulture);
        var periodEnd = (start + timeSpan).ToString(format, CultureInfo.InvariantCulture);
        var filter = "{\"PriceArea\":[\"%PriceArea%\"]}".Replace("%PriceArea%", priceArea);
        const string timezone = "UTC";
        var url = UrlHelper.EncodeUrlParameters(options.BaseUrl, $"{dataSet}?start={periodStart}&end={periodEnd}&filter={filter}&timezone={timezone}");

        using (var httpClient = httpClientFactory.CreateClient())
        {
            httpClient.Timeout = options.RequestTimeout;
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                if (useDayAheadPrices)
                {
                    var prices = await response.Content.ReadFromJsonAsync<DayAheadPricesDto>();
                    Validate(prices);
                    return prices.Records.OrderBy(x => x.TimeUtc).Select(x => x.GetKwhAmount()).ToList();
                }
                else
                {
                    var prices = await response.Content.ReadFromJsonAsync<ElSpotPricesDto>();
                    Validate(prices);
                    return prices.Records.OrderBy(x => x.HourUtc).Select(x => x.GetKwhAmount()).ToList();
                }
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

    private static void Validate(DayAheadPricesDto dto)
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

    private static void Validate(ElSpotPricesDto dto)
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