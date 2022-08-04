using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
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
using PowerView.Service.Mappers;

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class ProfileControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<IProfileRepository> profileRepository;
    private Mock<ISeriesColorRepository> serieRepository;
    private Mock<IProfileGraphRepository> profileGraphRepository;
    private Mock<ISerieMapper> serieMapper;

    [SetUp]
    public void SetUp()
    {
        profileRepository = new Mock<IProfileRepository>();
        serieRepository = new Mock<ISeriesColorRepository>();
        profileGraphRepository = new Mock<IProfileGraphRepository>();
        serieMapper = new Mock<ISerieMapper>();

        serieRepository.Setup(sr => sr.GetColorCached(It.IsAny<string>(), It.IsAny<ObisCode>())).Returns<string, ObisCode>((l, oc) => "SC_" + l + "_" + oc);
        serieMapper.Setup(ocm => ocm.MapToSerieType(It.IsAny<ObisCode>())).Returns<ObisCode>(oc => "ST_" + oc);
        serieMapper.Setup(ocm => ocm.MapToSerieYAxis(It.IsAny<ObisCode>())).Returns<ObisCode>(oc => "YA_" + oc);

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(profileRepository.Object);
                    sc.AddSingleton(serieRepository.Object);
                    sc.AddSingleton(profileGraphRepository.Object);
                    sc.AddSingleton(TimeZoneHelper.GetDenmarkLocationContext());
                    sc.AddSingleton(serieMapper.Object);
                });
            });

        httpClient = application.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        application?.Dispose();
    }

    [Test]
    public async Task GetDayProfilePeriodAndPagePassedToRepository()
    {
        // Arrange
        var profileGraph = new ProfileGraph("day", "ThePage", "title", "5-minutes", 1, new[] { new SeriesName("Label", ObisCode.ElectrActiveEnergyA14Period) });
        StubProfileGraph(profileGraph);
        var midnight = TimeZoneHelper.GetDenmarkTodayAsUtc();
        profileRepository.Setup(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
          .Returns(new TimeRegisterValueLabelSeriesSet(midnight, midnight.AddDays(1), new TimeRegisterValueLabelSeries[0]));

        // Act
        var response = await httpClient.GetAsync($"api/profile/day?page={profileGraph.Page}&start={midnight.ToString("o")}");

        // Assert
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(profileGraph.Period, profileGraph.Page));
    }

    [Test]
    public async Task GetDayProfilePageQueryStringAbsent()
    {
        // Arrange
        var midnight = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/profile/day?start={midnight.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetDayProfileDateTimesPassedToRepository()
    {
        // Arrange
        StubProfileGraph();
        var midnight = TimeZoneHelper.GetDenmarkTodayAsUtc();
        profileRepository.Setup(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
          .Returns(new TimeRegisterValueLabelSeriesSet(midnight, midnight.AddDays(1), new TimeRegisterValueLabelSeries[0]));

        // Act
        var response = await httpClient.GetAsync($"api/profile/day?page=thePage&start={midnight.ToString("o")}");

        // Assert
        var end = midnight.AddDays(1);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.Is<DateTime>(dt => dt == midnight.AddMinutes(-2.5) && dt.Kind == midnight.Kind),
          It.Is<DateTime>(dt => dt == midnight && dt.Kind == midnight.Kind),
          It.Is<DateTime>(dt => ((dt - end) <= TimeSpan.FromHours(1) || (dt - end) >= TimeSpan.FromHours(-1)) && dt.Kind == midnight.Kind)));
    }

    [Test]
    public async Task GetDayProfileStartQueryStringAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/profile/day?page=thePage");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetDayProfileStartQueryStringBad()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/profile/day?page=thePage&start=badDateTime");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetDayProfileStartQueryStringNoyUtc()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/profile/day?page=thePage&start={DateTime.Now.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetDayProfile()
    {
        // Arrange
        var profileGraph1 = new ProfileGraph("day", "thePage", "Import", "5-minutes", 1, new[] {
        new SeriesName("Label1", "6.66.1.0.0.255"), new SeriesName("Label1", "6.67.8.0.0.255"),
        new SeriesName("Label1", "6.66.2.0.0.255"), new SeriesName("Label1", "6.67.9.0.0.255"),
        new SeriesName("Label1", "6.0.9.0.0.255")
      });
        StubProfileGraph(profileGraph1);
        var midnight = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var t0 = midnight - TimeSpan.FromMinutes(5);
        var t1 = midnight;
        var t2 = midnight + TimeSpan.FromMinutes(5);
        var label1Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"6.0.1.0.0.255", new [] { new TimeRegisterValue("1", t0, 1, 6, Unit.WattHour), new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } },
        {"6.0.2.0.0.255", new [] { new TimeRegisterValue("1", t0, 2, 0, Unit.CubicMetre), new TimeRegisterValue("1", t1, 3, 0, Unit.CubicMetre), new TimeRegisterValue("1", t2, 4, 0, Unit.CubicMetre) } },
        {"6.0.9.0.0.255", new [] { new TimeRegisterValue("1", t1, 4, 0, Unit.CubicMetrePrHour), new TimeRegisterValue("1", t2, 5, 0, Unit.CubicMetrePrHour) } }
      };
        var lss = new TimeRegisterValueLabelSeriesSet(midnight, midnight.AddDays(1), new[] { new TimeRegisterValueLabelSeries("Label1", label1Values) });
        profileRepository.Setup(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/profile/day?page=thePage&start={midnight.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ViewModelProfileRoot>();
        Assert.That(json.graphs.Length, Is.EqualTo(1));

        var energyImport = new ViewModelProfileGraph
        {
            title = profileGraph1.Title,
            categories = Enumerable.Range(0, 288).Select(i => midnight.AddMinutes(i * 5)).Select(ToStringMinute).ToArray(),
            series = new[] {
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.0.9.0.0.255", unit = "l/h", serietype = "ST_6.0.9.0.0.255", serieyaxis = "YA_6.0.9.0.0.255", seriecolor = "SC_Label1_6.0.9.0.0.255",
            values = new double?[] { 4000, 5000 }.Concat(Enumerable.Repeat<double?>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.66.1.0.0.255", unit = "kWh", serietype = "ST_6.66.1.0.0.255", serieyaxis = "YA_6.66.1.0.0.255", seriecolor = "SC_Label1_6.66.1.0.0.255",
            values = new double?[] { 1000, 2000 }.Concat(Enumerable.Repeat<double?>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.66.2.0.0.255", unit = "m3", serietype = "ST_6.66.2.0.0.255", serieyaxis = "YA_6.66.2.0.0.255", seriecolor = "SC_Label1_6.66.2.0.0.255",
            values = new double?[] { 1, 2 }.Concat(Enumerable.Repeat<double?>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.67.8.0.0.255", unit = "W", serietype = "ST_6.67.8.0.0.255", serieyaxis = "YA_6.67.8.0.0.255", seriecolor = "SC_Label1_6.67.8.0.0.255",
            values = new double?[] { 12000000, 12000000 }.Concat(Enumerable.Repeat<double?>(null, 286)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "6.67.9.0.0.255", unit = "l/h", serietype = "ST_6.67.9.0.0.255", serieyaxis = "YA_6.67.9.0.0.255", seriecolor = "SC_Label1_6.67.9.0.0.255",
            values = new double?[] { 12000, 12000 }.Concat(Enumerable.Repeat<double?>(null, 286)).ToArray() },
        }
        };
        AssertSerieSet(energyImport, json.graphs.First());

        Assert.That(json.periodtotals.Length, Is.EqualTo(2));
        AssertPeriodTotals("Label1", "6.66.1.0.0.255", "kWh", 2000d, json.periodtotals[0]);
        AssertPeriodTotals("Label1", "6.66.2.0.0.255", "m3", 2d, json.periodtotals[1]);
    }

    [Test]
    public async Task GetMonthProfilePeriodAndPagePassedToRepository()
    {
        // Arrange
        var profileGraph = new ProfileGraph("month", "ThePage", "title", "1-days", 1, new[] { new SeriesName("Label", ObisCode.ElectrActiveEnergyA14Period) });
        StubProfileGraph(profileGraph);
        var midnight = TimeZoneHelper.GetDenmarkTodayAsUtc();
        profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
          .Returns(new TimeRegisterValueLabelSeriesSet(midnight, midnight.AddDays(1), new TimeRegisterValueLabelSeries[0]));

        // Act
        var response = await httpClient.GetAsync($"api/profile/month?page={profileGraph.Page}&start={midnight.ToString("o")}");

        // Assert
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(profileGraph.Period, profileGraph.Page));
    }

    [Test]
    public async Task GetMonthProfilePageQueryStringAbsent()
    {
        // Arrange
        var midnight = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/profile/month?page=thePage");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }


    [Test]
    public async Task GetMonthProfileDateTimesPassedToRepository()
    {
        // Arrange
        var profileGraph = new ProfileGraph("month", "ThePage", "title", "1-days", 1, new[] { new SeriesName("Label", ObisCode.ElectrActiveEnergyA14Period) });
        StubProfileGraph(profileGraph);
        var midnight = new DateTime(2019, 3, 31, 22, 0, 0, DateTimeKind.Utc); // Midnight Denmark time..
        profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
          .Returns(new TimeRegisterValueLabelSeriesSet(midnight, midnight.AddDays(1), new TimeRegisterValueLabelSeries[0]));

        // Act
        var response = await httpClient.GetAsync($"api/profile/month?page=thePage&start={midnight.ToString("o")}");

        // Assert
        profileRepository.Verify(dpr => dpr.GetMonthProfileSet(It.Is<DateTime>(dt => dt == midnight.AddDays(-0.5) && dt.Kind == midnight.Kind),
          It.Is<DateTime>(dt => dt == midnight && dt.Kind == midnight.Kind), It.Is<DateTime>(dt => dt == midnight.AddMonths(1) && dt.Kind == midnight.Kind)));
    }

    [Test]
    public async Task GetMonthProfileStartQueryStringAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/profile/month?page=thePage");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetMonthProfileStartQueryStringBad()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/profile/month?page=thePage&start=badDateTime");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetMonthProfileStartQueryStringNoyUtc()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/profile/month?page=thePage&start={DateTime.Now.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetMonthProfile()
    {
        // Arrange
        var profileGraph1 = new ProfileGraph("month", "thePage", "Import", "1-days", 1,
          new[] { new SeriesName("Label1", "1.66.1.8.0.255"), new SeriesName("Label1", "1.65.1.8.0.255") });
        var profileGraph2 = new ProfileGraph("month", "thePage", "Export", "1-days", 2,
          new[] { new SeriesName("Label0", "1.66.2.8.0.255"), new SeriesName("Label0", "1.65.2.8.0.255") });
        StubProfileGraph(profileGraph1, profileGraph2);
        var midnight = new DateTime(2019, 3, 31, 22, 0, 0, DateTimeKind.Utc); // Midnight Denmark time..
        var t1 = midnight;
        var t2 = midnight.AddDays(1);
        var t3 = midnight.AddDays(2);
        var label1Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"1.0.1.8.0.255", new [] { new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } }
      };
        var label0Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"1.0.2.8.0.255", new [] { new TimeRegisterValue("1", t1, 4, 6, Unit.WattHour), new TimeRegisterValue("1", t3, 6, 6, Unit.WattHour) } }
      };
        var lss = new TimeRegisterValueLabelSeriesSet(midnight, midnight.AddMonths(1), new[] { new TimeRegisterValueLabelSeries("Label1", label1Values), new TimeRegisterValueLabelSeries("Label0", label0Values) });
        profileRepository.Setup(dpr => dpr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/profile/month?page=thePage&start={midnight.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ViewModelProfileRoot>();
        Assert.That(json.graphs.Length, Is.EqualTo(2));

        var electricityImport = new ViewModelProfileGraph
        {
            title = profileGraph1.Title,
            categories = Enumerable.Range(0, 30).Select(i => midnight.AddDays(i)).Select(ToStringDay).ToArray(),
            series = new[] {
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.65.1.8.0.255", unit = "kWh", serietype = "ST_1.65.1.8.0.255", serieyaxis = "YA_1.65.1.8.0.255", seriecolor = "SC_Label1_1.65.1.8.0.255",
            values = new double?[] { 0, 1000 }.Concat(Enumerable.Repeat<double?>(null, 28)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.66.1.8.0.255", unit = "kWh", serietype = "ST_1.66.1.8.0.255", serieyaxis = "YA_1.66.1.8.0.255", seriecolor = "SC_Label1_1.66.1.8.0.255",
            values = new double?[] { 0, 1000 }.Concat(Enumerable.Repeat<double?>(null, 28)).ToArray() }
        }
        };
        AssertSerieSet(electricityImport, json.graphs.First());

        var electricityExport = new ViewModelProfileGraph
        {
            title = profileGraph2.Title,
            categories = Enumerable.Range(0, 30).Select(i => midnight.AddDays(i)).Select(ToStringDay).ToArray(),
            series = new[] {
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.65.2.8.0.255", unit = "kWh", serietype = "ST_1.65.2.8.0.255", serieyaxis = "YA_1.65.2.8.0.255", seriecolor = "SC_Label0_1.65.2.8.0.255",
            values = new double?[] { 0, null, 2000 }.Concat(Enumerable.Repeat<double?>(null, 27)).ToArray() },
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.66.2.8.0.255", unit = "kWh", serietype = "ST_1.66.2.8.0.255", serieyaxis = "YA_1.66.2.8.0.255", seriecolor = "SC_Label0_1.66.2.8.0.255",
            values = new double?[] { 0, null, 2000 }.Concat(Enumerable.Repeat<double?>(null, 27)).ToArray() }
        }
        };
        AssertSerieSet(electricityExport, json.graphs.Last());

        Assert.That(json.periodtotals.Length, Is.EqualTo(2));
        AssertPeriodTotals("Label1", "1.66.1.8.0.255", "kWh", 1000d, json.periodtotals[0]);
        AssertPeriodTotals("Label0", "1.66.2.8.0.255", "kWh", 2000d, json.periodtotals[1]);
    }

    [Test]
    public async Task GetYearProfilePeriodAndPagePassedToRepository()
    {
        // Arrange
        var profileGraph = new ProfileGraph("year", "ThePage", "title", "1-months", 1, new[] { new SeriesName("Label", ObisCode.ElectrActiveEnergyA14Period) });
        StubProfileGraph(profileGraph);
        var midnight = new DateTime(2018, 12, 31, 23, 0, 0, DateTimeKind.Utc); // Midnight Denmark time..
        profileRepository.Setup(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
          .Returns(new TimeRegisterValueLabelSeriesSet(midnight, midnight.AddDays(1), new TimeRegisterValueLabelSeries[0]));

        // Act
        var response = await httpClient.GetAsync($"api/profile/year?page={profileGraph.Page}&start={midnight.ToString("o")}");

        // Assert
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(profileGraph.Period, profileGraph.Page));
    }

    [Test]
    public async Task GetYearProfilePageQueryStringAbsent()
    {
        // Arrange
        var midnight = new DateTime(2018, 12, 31, 23, 0, 0, DateTimeKind.Utc); // Midnight Denmark time..

        // Act
        var response = await httpClient.GetAsync($"api/profile/year?page=thePage");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetYearProfileDateTimesPassedToRepository()
    {
        // Arrange
        var profileGraph = new ProfileGraph("month", "thePage", "Import", "1-months", 1,
          new[] { new SeriesName("Label1", "1.0.1.8.0.255"), new SeriesName("Label1", "1.66.1.8.0.255"), new SeriesName("Label1", "1.65.1.8.0.255") });
        StubProfileGraph(profileGraph);
        var midnight = new DateTime(2018, 12, 31, 23, 0, 0, DateTimeKind.Utc); // Midnight Denmark time..
        profileRepository.Setup(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
          .Returns(new TimeRegisterValueLabelSeriesSet(midnight, midnight.AddMonths(12), new TimeRegisterValueLabelSeries[0]));

        // Act
        var response = await httpClient.GetAsync($"api/profile/year?page={profileGraph.Page}&start={midnight.ToString("o")}");

        // Assert
        profileRepository.Verify(dpr => dpr.GetYearProfileSet(It.Is<DateTime>(dt => dt == midnight.AddDays(-15.5) && dt.Kind == midnight.Kind),
          It.Is<DateTime>(dt => dt == midnight && dt.Kind == midnight.Kind), It.Is<DateTime>(dt => dt == midnight.AddMonths(12) && dt.Kind == midnight.Kind)));
    }

    [Test]
    public async Task GetYearProfileStartQueryStringAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/profile/year?page=thePage");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileRepository.Verify(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetYearProfileStartQueryStringBad()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/profile/year?page=thePage&start=badDateTime");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetYearProfileStartQueryStringNoyUtc()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/profile/year?page=thePage&start={DateTime.Now.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileGraphRepository.Verify(pgr => pgr.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        profileRepository.Verify(dpr => dpr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetYearProfile()
    {
        // Arrange
        var profileGraph1 = new ProfileGraph("month", "thePage", "Import", "1-months", 1,
          new[] { new SeriesName("Label1", "1.66.1.8.0.255"), new SeriesName("Label1", "1.65.1.8.0.255") });
        var profileGraph2 = new ProfileGraph("month", "thePage", "Export", "1-months", 2,
          new[] { new SeriesName("Label0", "1.66.2.8.0.255"), new SeriesName("Label0", "1.65.2.8.0.255") });
        StubProfileGraph(profileGraph1, profileGraph2);
        var midnight = new DateTime(2018, 12, 31, 23, 0, 0, DateTimeKind.Utc); // Midnight Denmark time..
        var t1 = midnight;
        var t2 = midnight.AddMonths(1);
        var t3 = midnight.AddMonths(2);
        var label1Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"1.0.1.8.0.255", new [] { new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } }
      };
        var label2Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        {"1.0.2.8.0.255", new [] { new TimeRegisterValue("1", t1, 4, 6, Unit.WattHour), new TimeRegisterValue("1", t3, 6, 6, Unit.WattHour) } }
      };
        var lss = new TimeRegisterValueLabelSeriesSet(t1, t1.AddMonths(12), new[] { new TimeRegisterValueLabelSeries("Label1", label1Values), new TimeRegisterValueLabelSeries("Label0", label2Values) });
        profileRepository.Setup(dpr => dpr.GetYearProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/profile/year?page=thePage&start={midnight.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ViewModelProfileRoot>();
        Assert.That(json.graphs.Length, Is.EqualTo(2));

        var electricityImport = new ViewModelProfileGraph
        {
            title = profileGraph1.Title,
            categories = GetMonthDateTimes(midnight).Select(ToStringMonth).ToArray(),
            series = new[] {
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.65.1.8.0.255", unit = "kWh", serietype = "ST_1.65.1.8.0.255", serieyaxis = "YA_1.65.1.8.0.255", seriecolor = "SC_Label1_1.65.1.8.0.255",
            values = new double?[] { 0, 1000 }.Concat(Enumerable.Repeat<double?>(null, 10)).ToArray() },
          new ViewModelProfileSerie { label = "Label1", obisCode = "1.66.1.8.0.255", unit = "kWh", serietype = "ST_1.66.1.8.0.255", serieyaxis = "YA_1.66.1.8.0.255", seriecolor = "SC_Label1_1.66.1.8.0.255",
            values = new double?[] { 0, 1000 }.Concat(Enumerable.Repeat<double?>(null, 10)).ToArray() }
        }
        };
        AssertSerieSet(electricityImport, json.graphs.First());

        var electricityExport = new ViewModelProfileGraph
        {
            title = profileGraph2.Title,
            categories = GetMonthDateTimes(midnight).Select(ToStringMonth).ToArray(),
            series = new[] {
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.65.2.8.0.255", unit = "kWh", serietype = "ST_1.65.2.8.0.255", serieyaxis = "YA_1.65.2.8.0.255", seriecolor = "SC_Label0_1.65.2.8.0.255",
            values = new double?[] { 0, null, 2000 }.Concat(Enumerable.Repeat<double?>(null, 9)).ToArray() },
          new ViewModelProfileSerie { label = "Label0", obisCode = "1.66.2.8.0.255", unit = "kWh", serietype = "ST_1.66.2.8.0.255", serieyaxis = "YA_1.66.2.8.0.255", seriecolor = "SC_Label0_1.66.2.8.0.255",
            values = new double?[] { 0, null, 2000 }.Concat(Enumerable.Repeat<double?>(null, 9)).ToArray() }
        }
        };
        AssertSerieSet(electricityExport, json.graphs.Last());

        Assert.That(json.periodtotals.Length, Is.EqualTo(2));
        AssertPeriodTotals("Label1", "1.66.1.8.0.255", "kWh", 1000d, json.periodtotals[0]);
        AssertPeriodTotals("Label0", "1.66.2.8.0.255", "kWh", 2000d, json.periodtotals[1]);
    }

    private static IEnumerable<DateTime> GetMonthDateTimes(DateTime origin)
    {
        var timeZoneInfo = TimeZoneHelper.GetDenmarkTimeZoneInfo();
        return Enumerable.Range(0, 12).Select(i =>
        {
            var monthDate = origin.AddMonths(i);
            monthDate = timeZoneInfo.IsDaylightSavingTime(monthDate) ? monthDate.Subtract(TimeSpan.FromHours(1)) : monthDate;
            return monthDate;
        });
    }

    private void StubProfileGraph(params ProfileGraph[] profileGraphs)
    {
        var profileGraphsLocal = new List<ProfileGraph>(profileGraphs);

        if (profileGraphsLocal.Count == 0)
        {
            profileGraphsLocal.Add(new ProfileGraph("day", "thePage", "theTitle", "5-minutes", 1, new[] { new SeriesName("theLabel", ObisCode.ElectrActiveEnergyA14Period) }));
        }

        profileGraphRepository.Setup(x => x.GetProfileGraphs(It.IsAny<string>(), It.IsAny<string>())).Returns(profileGraphsLocal);
    }

    private static string ToStringMinute(DateTime dt)
    {
        dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0, 0, dt.Kind);
        return dt.ToString("yyyy-MM-ddTHH:mmZ", CultureInfo.InvariantCulture);
    }

    private static string ToStringDay(DateTime dt)
    {
        return dt.ToString("yyyy-MM-ddTHH:mmZ", CultureInfo.InvariantCulture);
    }

    private static string ToStringMonth(DateTime dt)
    {
        return dt.ToString("yyyy-MM-ddTHH:mmZ", CultureInfo.InvariantCulture);
    }

    private static void AssertSerieSet(ViewModelProfileGraph expected, ViewModelProfileGraph actual)
    {
        Assert.That(actual.title, Is.EqualTo(expected.title));
        Assert.That(actual.categories, Is.EqualTo(expected.categories));
        Assert.That(actual.series.Length, Is.EqualTo(expected.series.Length));
        for (var ix = 0; ix < expected.series.Length; ix++)
        {
            AssertSerie(expected.series[ix], actual.series[ix]);
        }
    }

    private static void AssertSerie(ViewModelProfileSerie expected, ViewModelProfileSerie actual)
    {
        Assert.That(actual.label, Is.EqualTo(expected.label));
        Assert.That(actual.obisCode, Is.EqualTo(expected.obisCode));
        Assert.That(actual.unit, Is.EqualTo(expected.unit), actual.obisCode);
        Assert.That(actual.serietype, Is.EqualTo(expected.serietype), actual.obisCode);
        Assert.That(actual.serieyaxis, Is.EqualTo(expected.serieyaxis), actual.obisCode);
        Assert.That(actual.seriecolor, Is.EqualTo(expected.seriecolor), actual.obisCode);
        Assert.That(actual.values, Is.EqualTo(expected.values), actual.obisCode);
    }

    private static void AssertPeriodTotals(string label, string register, string unit, double value, ViewModelProfilePeriodTotal actual)
    {
        Assert.That(actual.label, Is.EqualTo(label));
        Assert.That(actual.obisCode, Is.EqualTo(register));
        Assert.That(actual.unit, Is.EqualTo(unit));
        Assert.That(actual.value, Is.EqualTo(value));
    }

    public static Stream EqualTo(IEnumerable<byte> expectedBytes)
    {
        return Match.Create<Stream>(s =>
        {
            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);
                var sArray = ms.ToArray();
                return sArray.SequenceEqual(expectedBytes);
            }
        });
    }

    internal class ViewModelProfileRoot
    {
        public ViewModelProfileGraph[] graphs { get; set; }
        public ViewModelProfilePeriodTotal[] periodtotals { get; set; }
    }

    internal class ViewModelProfileGraph
    {
        public string title { get; set; }
        public string[] categories { get; set; }
        public ViewModelProfileSerie[] series { get; set; }
    }

    internal class ViewModelProfileSerie
    {
        public string label { get; set; }
        public string obisCode { get; set; }
        public string unit { get; set; }
        public string serietype { get; set; }
        public string serieyaxis { get; set; }
        public string seriecolor { get; set; }
        public double?[] values { get; set; }
    }

    internal class ViewModelProfilePeriodTotal
    {
        public string label { get; set; }
        public string obisCode { get; set; }
        public double value { get; set; }
        public string unit { get; set; }
    }

}
