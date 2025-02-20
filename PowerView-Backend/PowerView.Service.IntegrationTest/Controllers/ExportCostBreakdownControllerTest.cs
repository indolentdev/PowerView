using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Controllers;

namespace PowerView.Service.IntegrationTest;

public class ExportCostBreakdownControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<ICostBreakdownRepository> costBreakdownRepository;

    [SetUp]
    public void Setup()
    {
        costBreakdownRepository = new Mock<ICostBreakdownRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(costBreakdownRepository.Object);
                    sc.AddSingleton(TimeZoneHelper.GetDenmarkLocationContext());
                });
            });

        httpClient = application.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        httpClient?.Dispose();
        application?.Dispose();
    }

    [Test]
    public async Task GetTitles_CallsRepository()
    {
        // Arrange
        SetupCostBreakdownRepositoryGetTitles();

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdowntitles");

        // Assert
        costBreakdownRepository.Verify(x => x.GetCostBreakdownTitles());
    }

    [Test]
    public async Task GetTitles()
    {
        // Arrange
        var titles = new[] { "Title1", "Title2" };
        SetupCostBreakdownRepositoryGetTitles(titles);

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdowntitles");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.That(json, Is.EqualTo(titles));
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportFromAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?to={today.ToString("o")}&title=t1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportToAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={today.ToString("o")}&title=t1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportFromEqualToTo()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={today.ToString("o")}&to={today.ToString("o")}&title=t1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportFromGreaterThanTo()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={today.AddSeconds(1).ToString("o")}&to={today.ToString("o")}&title=t1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.IsAny<string>()), Times.Never);
    }


    [Test]
    public async Task GetHourlyCostBreakdownExportTitleAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={today.ToString("o")}&to={today.AddSeconds(1).ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportFromBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from=badFormat&to={today.ToString("o")}&title=t1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportToBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={today.ToString("o")}&to=badFormat&title=t1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportFromNotUtc()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var local = DateTime.SpecifyKind(today, DateTimeKind.Local);

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={local.ToString("o")}&to={today.ToString("o")}&title=t1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportToNotUtc()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var local = DateTime.SpecifyKind(today, DateTimeKind.Local);

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={today.ToString("o")}&to={local.ToString("o")}&title=t1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportFromToLabelsPresent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var title = "title1";
        var costBreakdown = new CostBreakdown(title, Unit.Dkk, 10, Array.Empty<CostBreakdownEntry>());
        SetupCostBreakdownRepositoryGetCostBreakdown(costBreakdown);

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={today.ToString("o")}&to={today.AddDays(1).ToString("o")}&title={title}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdown(It.Is<string>(x => x == title)));
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportAbsent()
    {
        // Arrange
        var today = new DateTime(2023, 9, 16, 22, 0, 0, DateTimeKind.Utc);
        var from = today;
        var to = from.AddHours(8);

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={from.ToString("o")}&to={to.ToString("o")}&title=dummy");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportOneEntry()
    {
        // Arrange
        var today = new DateTime(2023, 9, 16, 22, 0, 0, DateTimeKind.Utc);
        var from = today;
        var to = from.AddHours(8);
        var costBreakdownEntry = new CostBreakdownEntry(from.AddHours(3), to.AddHours(3), "entry", 4, 23, 12.34);
        var costBreakdown = new CostBreakdown("Title1", Unit.Eur, 11, new[] { costBreakdownEntry });
        SetupCostBreakdownRepositoryGetCostBreakdown(costBreakdown);

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={from.ToString("o")}&to={to.ToString("o")}&title=dummy");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportCostBreakdownRoot>();
        AssertPeriods(new[] { new ExportPeriod(from, from.AddHours(1)), new ExportPeriod(from.AddHours(1), from.AddHours(2)),
          new ExportPeriod(from.AddHours(2), from.AddHours(3)), new ExportPeriod(from.AddHours(3), from.AddHours(4)),
          new ExportPeriod(from.AddHours(4), from.AddHours(5)), new ExportPeriod(from.AddHours(5), from.AddHours(6)),
          new ExportPeriod(from.AddHours(6), from.AddHours(7)), new ExportPeriod(from.AddHours(7), from.AddHours(8)) }, json.periods);

        Assert.That(json.title, Is.EqualTo(costBreakdown.Title));
        Assert.That(json.currency, Is.EqualTo(costBreakdown.Currency.ToString().ToUpperInvariant()));
        Assert.That(json.vat, Is.EqualTo(costBreakdown.Vat));
        Assert.That(json.entries, Has.Length.EqualTo(1));
        AssertExportCostBreakdownEntryValues(costBreakdownEntry.Name, new dynamic[]
        {
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
            new { From = (DateTime?)from.AddHours(4), To = (DateTime?)from.AddHours(5), Value = (double?)costBreakdownEntry.Amount },
            new { From = (DateTime?)from.AddHours(5), To = (DateTime?)from.AddHours(6), Value = (double?)costBreakdownEntry.Amount },
            new { From = (DateTime?)from.AddHours(6), To = (DateTime?)from.AddHours(7), Value = (double?)costBreakdownEntry.Amount },
            new { From = (DateTime?)from.AddHours(7), To = (DateTime?)from.AddHours(8), Value = (double?)costBreakdownEntry.Amount },
        }, json.entries[0]);
    }

    [Test]
    public async Task GetHourlyCostBreakdownExportTwoEntries()
    {
        // Arrange
        var today = new DateTime(2023, 9, 16, 22, 0, 0, DateTimeKind.Utc);
        var from = today;
        var to = from.AddHours(8);
        var costBreakdownEntry1 = new CostBreakdownEntry(from.AddDays(-33), to.AddDays(33), "entry", 0, 23, 23.45);
        var costBreakdownEntry2 = new CostBreakdownEntry(from.AddHours(-3), to.AddHours(-3), "entry", 0, 1, 12.34);
        var costBreakdown = new CostBreakdown("Title1", Unit.Eur, 11, new[] { costBreakdownEntry1, costBreakdownEntry2 });
        SetupCostBreakdownRepositoryGetCostBreakdown(costBreakdown);

        // Act
        var response = await httpClient.GetAsync($"api/export/costbreakdown/hourly?from={from.ToString("o")}&to={to.ToString("o")}&title=dummy");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportCostBreakdownRoot>();
        AssertPeriods(new[] { new ExportPeriod(from, from.AddHours(1)), new ExportPeriod(from.AddHours(1), from.AddHours(2)),
          new ExportPeriod(from.AddHours(2), from.AddHours(3)), new ExportPeriod(from.AddHours(3), from.AddHours(4)),
          new ExportPeriod(from.AddHours(4), from.AddHours(5)), new ExportPeriod(from.AddHours(5), from.AddHours(6)),
          new ExportPeriod(from.AddHours(6), from.AddHours(7)), new ExportPeriod(from.AddHours(7), from.AddHours(8)) }, json.periods);

        Assert.That(json.title, Is.EqualTo(costBreakdown.Title));
        Assert.That(json.currency, Is.EqualTo(costBreakdown.Currency.ToString().ToUpperInvariant()));
        Assert.That(json.vat, Is.EqualTo(costBreakdown.Vat));
        Assert.That(json.entries, Has.Length.EqualTo(2));
        AssertExportCostBreakdownEntryValues(costBreakdownEntry1.Name, new dynamic[]
        {
            new { From = (DateTime?)from, To = (DateTime?)from.AddHours(1), Value = (double?)costBreakdownEntry1.Amount },
            new { From = (DateTime?)from.AddHours(1), To = (DateTime?)from.AddHours(2), Value = (double?)costBreakdownEntry1.Amount },
            new { From = (DateTime?)from.AddHours(2), To = (DateTime?)from.AddHours(3), Value = (double?)costBreakdownEntry1.Amount },
            new { From = (DateTime?)from.AddHours(3), To = (DateTime?)from.AddHours(4), Value = (double?)costBreakdownEntry1.Amount },
            new { From = (DateTime?)from.AddHours(4), To = (DateTime?)from.AddHours(5), Value = (double?)costBreakdownEntry1.Amount },
            new { From = (DateTime?)from.AddHours(5), To = (DateTime?)from.AddHours(6), Value = (double?)costBreakdownEntry1.Amount },
            new { From = (DateTime?)from.AddHours(6), To = (DateTime?)from.AddHours(7), Value = (double?)costBreakdownEntry1.Amount },
            new { From = (DateTime?)from.AddHours(7), To = (DateTime?)from.AddHours(8), Value = (double?)costBreakdownEntry1.Amount },
        }, json.entries[0]);
        AssertExportCostBreakdownEntryValues(costBreakdownEntry2.Name, new dynamic[]
        {
            new { From = (DateTime?)from, To = (DateTime?)from.AddHours(1), Value = (double?)costBreakdownEntry2.Amount },
            new { From = (DateTime?)from.AddHours(1), To = (DateTime?)from.AddHours(2), Value = (double?)costBreakdownEntry2.Amount },
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
            new { From = (DateTime?)null, To = (DateTime?)null, Value = (double?)null },
        }, json.entries[1]);
    }

    private void SetupCostBreakdownRepositoryGetTitles(params string[] titles)
    {
        costBreakdownRepository.Setup(x => x.GetCostBreakdownTitles()).Returns(titles);
    }

    private void SetupCostBreakdownRepositoryGetCostBreakdown(CostBreakdown costBreakdown)
    {
        costBreakdownRepository.Setup(cbr => cbr.GetCostBreakdown(It.IsAny<string>())).Returns(costBreakdown);
    }

    private static TimeRegisterValueLabelSeries CreateLabelSeries(string label, ObisCode obisCode, params TimeRegisterValue[] values)
    {
        return CreateLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { obisCode, values }
      });
    }

    private static TimeRegisterValueLabelSeries CreateLabelSeries(string label, IDictionary<ObisCode, IEnumerable<TimeRegisterValue>> values)
    {
        return new TimeRegisterValueLabelSeries(label, values);
    }

    private static void AssertTimestamps(IEnumerable<DateTime> expected, string[] actual)
    {
        Assert.That(actual.Select(dt => DateTime.Parse(dt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)).ToArray(), Is.EqualTo(expected.ToArray()));
    }

    private static void AssertPeriods(IEnumerable<ExportPeriod> expected, ExportCostBreakdownPeriod[] actual)
    {
        Assert.That(
            actual.Select(x => new ExportPeriod(
                DateTime.Parse(x.from, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTime.Parse(x.to, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            )).ToArray(),
            Is.EqualTo(expected.ToArray())
        );
    }

    private static void AssertExportCostBreakdownEntryValues(string name, dynamic[] values, ExportCostBreakdownEntry actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.name, Is.EqualTo(name));
        Assert.That(actual.values, Has.Length.EqualTo(values.Length));
        for (var ix = 0; ix < values.Length; ix++)
        {
            var actualValue = actual.values[ix];
            dynamic expectedValue = values[ix];
            Assert.That(actualValue.from != null ? DateTime.Parse(actualValue.from, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null, Is.EqualTo(expectedValue.From));
            Assert.That(actualValue.to != null ? DateTime.Parse(actualValue.to, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null, Is.EqualTo(expectedValue.To));
            Assert.That(actualValue.value, Is.EqualTo(expectedValue.Value));
        }
    }

    internal class ExportCostBreakdownRoot
    {
        public string title { get; set; }
        public string currency { get; set; }
        public int? vat { get; set; }
        public ExportCostBreakdownPeriod[] periods { get; set; }
        public ExportCostBreakdownEntry[] entries { get; set; }
    }

    internal class ExportCostBreakdownPeriod
    {
        public string from { get; set; }
        public string to { get; set; }
    }

    internal class ExportCostBreakdownEntry
    {
        public string name { get; set; }
        public ExportCostBreakdownValue[] values { get; set; }
    }

    internal class ExportCostBreakdownValue
    {
        public string from { get; set; }
        public string to { get; set; }
        public double? value { get; set; }
    }

}
