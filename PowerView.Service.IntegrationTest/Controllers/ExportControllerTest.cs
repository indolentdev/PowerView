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

public class ExportControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<ISeriesNameRepository> seriesNameRepository;
    private Mock<IExportRepository> exportRepository;

    [SetUp]
    public void Setup()
    {
        seriesNameRepository = new Mock<ISeriesNameRepository>();
        exportRepository = new Mock<IExportRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(seriesNameRepository.Object);
                    sc.AddSingleton(exportRepository.Object);
                    sc.AddSingleton(TimeZoneHelper.GetDenmarkLocationContext());
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
    public async Task GetLabels_CallsSeriesNameRepository()
    {
        // Arrange
        SetupSeriesNameRepositoryGetStoredSeriesNames();

        // Act
        var response = await httpClient.GetAsync($"api/export/labels");

        // Assert
        seriesNameRepository.Verify(x => x.GetSeriesNames());
    }

    [Test]
    public async Task GetLabels()
    {
        // Arrange
        var seriesNames = new[] { new SeriesName("lbl1", ObisCode.ColdWaterVolume1), new SeriesName("lbl2", ObisCode.ElectrActiveEnergyA14) };
        SetupSeriesNameRepositoryGetStoredSeriesNames(seriesNames);

        // Act
        var response = await httpClient.GetAsync($"api/export/labels");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.That(json, Is.EqualTo(seriesNames.Select(x => x.Label).ToArray()));
    }

    [Test]
    public async Task GetLabelsDistinctsLabels()
    {
        // Arrange
        var seriesNames = new[] { new SeriesName("lbl1", ObisCode.ColdWaterVolume1), new SeriesName("lbl1", ObisCode.ElectrActiveEnergyA14) };
        SetupSeriesNameRepositoryGetStoredSeriesNames(seriesNames);

        // Act
        var response = await httpClient.GetAsync($"api/export/labels");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.That(json, Is.EqualTo(new[] { "lbl1" }));
    }

    [Test]
    public async Task GetLabelsIncludesCumulatives()
    {
        // Arrange
        var seriesNames = new[] { new SeriesName("lbl1", ObisCode.ColdWaterFlow1), new SeriesName("lbl2", ObisCode.ElectrActiveEnergyA14) };
        SetupSeriesNameRepositoryGetStoredSeriesNames(seriesNames);

        // Act
        var response = await httpClient.GetAsync($"api/export/labels");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.That(json, Is.EqualTo(new[] { "lbl2" }));
    }

    [Test]
    public async Task GetHourlyDiffExportFromAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffExportToAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffExportFromEqualToTo()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.ToString("o")}&to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffExportFromGreaterThanTo()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.AddSeconds(1).ToString("o")}&to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffExportLabelAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.ToString("o")}&to={today.AddSeconds(1).ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffExportFromBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from=badFormat&to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffExportToBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.ToString("o")}&to=badFormat&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffExportFromNotUtc()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var local = DateTime.SpecifyKind(today, DateTimeKind.Local);

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={local.ToString("o")}&to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffExportToNotUtc()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var local = DateTime.SpecifyKind(today, DateTimeKind.Local);

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.ToString("o")}&to={local.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffExportFromToLabelsPresent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var labels = new[] { "lbl1", "lbl2" };
        var lss = new TimeRegisterValueLabelSeriesSet(today, today, new TimeRegisterValueLabelSeries[0]);
        SetupExportRepositoryGetLiveCumulativeSeries(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.ToString("o")}&to={today.AddDays(1).ToString("o")}&{string.Join("&", labels.Select(l => "label=" + l))}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.Is<DateTime>(dt => dt == today && dt.Kind == today.Kind),
                                                                 It.Is<DateTime>(dt => dt == today.AddDays(1) && dt.Kind == today.Kind),
                                                                 It.Is<IList<string>>(x => x.SequenceEqual(labels))));
    }

    [Test]
    public async Task GetHourlyDiffExportOneSeries()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var skew = TimeSpan.FromMinutes(5); // To verify normalization.
        var t1 = today - TimeSpan.FromHours(3) + skew;
        var t2 = today - TimeSpan.FromHours(2) + skew;
        var t3 = today - TimeSpan.FromHours(1) + skew;
        var obisCode = ObisCode.ColdWaterVolume1;
        var ls = CreateLabelSeries("label1", obisCode, new[]
        {
            new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
            new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre),
            new TimeRegisterValue("S1", t3, 5, 2, Unit.CubicMetre)
        });
        var lss = new TimeRegisterValueLabelSeriesSet(t1, t3, new[] { ls });
        exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.AddHours(-5).ToString("o")}&to={today.ToString("o")}&label={ls.Label}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportDiffsRoot>();
        AssertPeriods(new[] { new ExportPeriod(t1 - skew, t2 - skew), new ExportPeriod(t2 - skew, t3 - skew) }, json.periods);

        Assert.That(json.series, Has.Length.EqualTo(1));
        AssertExportDiffSeries(ls.Label, obisCode.ToDelta(), new dynamic[] 
        {
            new { From = t1, To = t2, Value = (double?)100, Unit = "m3" },
            new { From = t2, To = t3, Value = (double?)200, Unit = "m3" }
        }, json.series[0]);
    }

    [Test]
    public async Task GetHourlyDiffExportTwoSeriesOneLabel()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var skew = TimeSpan.FromMinutes(5); // To verify normalization.
        var t1 = today - TimeSpan.FromHours(3) + skew;
        var t2 = today - TimeSpan.FromHours(2) + skew;
        var t3 = today - TimeSpan.FromHours(1) + skew;
        var a14 = ObisCode.ElectrActiveEnergyA14;
        var a23 = ObisCode.ElectrActiveEnergyA23;
        var ls1 = CreateLabelSeries("label1", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>
        {
            { a14, new[]
                {
                    new TimeRegisterValue("S1", t1, 2, 3, Unit.WattHour),
                    new TimeRegisterValue("S1", t2, 7, 3, Unit.WattHour),
                    new TimeRegisterValue("S1", t3, 10, 3, Unit.WattHour)
                }
            },
            {
            a23, new[]
                {
                    new TimeRegisterValue("S2", t1 + skew, 4, 3, Unit.WattHour),
                    new TimeRegisterValue("S2", t2 + skew, 6, 3, Unit.WattHour),
                    new TimeRegisterValue("S2", t3 + skew, 13, 3, Unit.WattHour)
                }
            }
        });

        var lss = new TimeRegisterValueLabelSeriesSet(t1, t3, new[] { ls1 });
        exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.AddHours(-5).ToString("o")}&to={today.ToString("o")}&label={ls1.Label}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportDiffsRoot>();
        AssertPeriods(new[] { new ExportPeriod(t1 - skew, t2 - skew), new ExportPeriod(t2 - skew, t3 - skew) }, json.periods);

        Assert.That(json.series, Has.Length.EqualTo(4));
        AssertExportDiffSeries(ls1.Label, a14.ToDelta(), new dynamic[] 
        {
            new { From = t1, To = t2, Value = 5, Unit = "kWh" },
            new { From = t2, To = t3, Value = 3, Unit = "kWh" },
        }, json.series[0]);
        AssertExportDiffSeries(ls1.Label, a23.ToDelta(), new dynamic[] 
        {
            new { From = t1 + skew, To = t2 + skew, Value = 2, Unit = "kWh" },
            new { From = t2 + skew, To = t3 + skew, Value = 7, Unit = "kWh" },
        }, json.series[1]);
        AssertExportDiffSeries(ls1.Label, ObisCode.ElectrActiveEnergyA14NetDelta, new dynamic[] 
        {
            new { From = t1, To = t2 + skew, Value = 3, Unit = "kWh" },
            new { From = t2, To = t3 + skew, Value = 0, Unit = "kWh" },
        }, json.series[2]);
        AssertExportDiffSeries(ls1.Label, ObisCode.ElectrActiveEnergyA23NetDelta, new dynamic[] 
        {
            new { From = t1, To = t2 + skew, Value = 0, Unit = "kWh" },
            new { From = t2, To = t3 + skew, Value = 4, Unit = "kWh" },
        }, json.series[3]);
    }

    [Test]
    public async Task GetHourlyDiffExportTwoSeriesTwoLabels()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var skew = TimeSpan.FromMinutes(5); // To verify normalization.
        var t1 = today - TimeSpan.FromHours(2) + skew;
        var t2 = today - TimeSpan.FromHours(1) + skew;
        var obisCode = ObisCode.ElectrActiveEnergyA14;
        var ls1 = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre)
        });
        var ls2 = CreateLabelSeries("label2", obisCode, new[]
        {
            new TimeRegisterValue("S2", t1 + skew, 4, 3, Unit.WattHour),
            new TimeRegisterValue("S2", t2 + skew, 6, 3, Unit.WattHour)
        });
        var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls1, ls2 });
        exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.AddHours(-5).ToString("o")}&to={today.ToString("o")}&label={ls1.Label}&label={ls2.Label}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportDiffsRoot>();
        AssertPeriods(new[] { new ExportPeriod(t1 - skew, t2 - skew) }, json.periods);

        Assert.That(json.series, Has.Length.EqualTo(2));
        AssertExportDiffSeries(ls1.Label, obisCode.ToDelta(), new dynamic[] 
        {
            new { From = t1, To = t2, Value = 100, Unit = "m3" },
        }, json.series[0]);
        AssertExportDiffSeries(ls2.Label, obisCode.ToDelta(), new dynamic[] 
        {
            new { From = t1 + skew, To = t2 + skew, Value = 2, Unit = "kWh" },
        }, json.series[1]);
    }

    [Test]
    public async Task GetHourlyDiffExportDiffValueAbsentWhenDeviceIdChanges()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var skew = TimeSpan.FromMinutes(5); // To verify normalization.
        var t1 = today - TimeSpan.FromHours(3) + skew;
        var t2 = today - TimeSpan.FromHours(2) + skew;
        var t3 = today - TimeSpan.FromHours(1) + skew;
        var obisCode = ObisCode.ElectrActiveEnergyA14;
        var ls = CreateLabelSeries("label1", obisCode, new[]
        {
            new TimeRegisterValue("S1", t1, 2, 3, Unit.WattHour),
            new TimeRegisterValue("Other", t2, 3, 3, Unit.WattHour),
            new TimeRegisterValue("Other", t3, 4, 3, Unit.WattHour)
        }
        );
        var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls });
        exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/diffs/hourly?from={today.AddHours(-5).ToString("o")}&to={today.ToString("o")}&label={ls.Label}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportDiffsRoot>();
        AssertPeriods(new[] { new ExportPeriod(t1 - skew, t2 - skew), new ExportPeriod(t2 - skew, t3 - skew) }, json.periods);

        Assert.That(json.series, Has.Length.EqualTo(1));
        AssertExportDiffSeries(ls.Label, obisCode.ToDelta(), new dynamic[] 
        {
            new { From = t1, To = t2, Value = 0, Unit = "kWh" },
            new { From = t2, To = t3, Value = 1, Unit = "kWh" },
        }, json.series[0]);
    }

    [Test]
    public async Task GetHourlyGaugesExportFromAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyGaugesExportToAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyGaugesExportFromEqualToTo()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.ToString("o")}&to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyDiffGaugesFromGreaterThanTo()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.AddSeconds(1).ToString("o")}&to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyGaugesExportLabelAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.ToString("o")}&to={today.AddSeconds(1).ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyGaugesExportFromBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from=badFormat&to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyGaugesExportToBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.ToString("o")}&to=badFormat&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyGaugesExportFromNotUtc()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var local = DateTime.SpecifyKind(today, DateTimeKind.Local);

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={local.ToString("o")}&to={today.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyGaugesExportToNotUtc()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var local = DateTime.SpecifyKind(today, DateTimeKind.Local);

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.ToString("o")}&to={local.ToString("o")}&label=lbl1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task GetHourlyGaugesExportFromToLabelsPresent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var labels = new[] { "lbl1", "lbl2" };
        var lss = new TimeRegisterValueLabelSeriesSet(today, today, new TimeRegisterValueLabelSeries[0]);
        SetupExportRepositoryGetLiveCumulativeSeries(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.ToString("o")}&to={today.AddDays(1).ToString("o")}&{string.Join("&", labels.Select(l => "label=" + l))}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        exportRepository.Verify(pr => pr.GetLiveCumulativeSeries(It.Is<DateTime>(dt => dt == today && dt.Kind == today.Kind),
                                                                 It.Is<DateTime>(dt => dt == today.AddDays(1) && dt.Kind == today.Kind),
                                                                 It.Is<IList<string>>(x => x.SequenceEqual(labels))));
    }

    [Test]
    public async Task GetHourlyGaugesExportOneSeries()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var skew = TimeSpan.FromMinutes(5); // To verify normalization.
        var t1 = today - TimeSpan.FromHours(3) + skew;
        var t2 = today - TimeSpan.FromHours(2) + skew;
        var t3 = today - TimeSpan.FromHours(1) + skew;
        var obisCode = ObisCode.ColdWaterVolume1;
        var ls = CreateLabelSeries("label1", obisCode, new[]
        {
            new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
            new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre),
            new TimeRegisterValue("S1", t3, 5, 2, Unit.CubicMetre)
        });
        var lss = new TimeRegisterValueLabelSeriesSet(t1, t3, new[] { ls });
        exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.AddHours(-5).ToString("o")}&to={today.ToString("o")}&label={ls.Label}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportGaugesRoot>();
        AssertTimestamps(new[] { t1, t2, t3 }.Select(x => x - skew), json.timestamps);

        Assert.That(json.series, Has.Length.EqualTo(1));
        AssertExportGaugeSeries(ls.Label, obisCode, new dynamic[] 
        {
            new { Timestamp = t1, Value = 200, Unit = "m3", DeviceId = "S1" },
            new { Timestamp = t2, Value = 300, Unit = "m3", DeviceId = "S1" },
            new { Timestamp = t3, Value = 500, Unit = "m3", DeviceId = "S1" }
        }, json.series[0]);
    }

    [Test]
    public async Task GetHourlyGaugesExportTwoSeriesOneLabel()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var skew = TimeSpan.FromMinutes(5); // To verify normalization.
        var t1 = today - TimeSpan.FromHours(2) + skew;
        var t2 = today - TimeSpan.FromHours(1) + skew;
        var obisCode1 = ObisCode.ElectrActiveEnergyA14;
        var obisCode2 = ObisCode.ElectrActiveEnergyA23;
        var ls1 = CreateLabelSeries("label1", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>
        {
            { obisCode1, new[]
                {
                    new TimeRegisterValue("S1", t1, 2, 3, Unit.WattHour),
                    new TimeRegisterValue("S1", t2, 3, 3, Unit.WattHour)
                }
            },
            {
            obisCode2, new[]
                {
                    new TimeRegisterValue("S2", t1 + skew, 4, 3, Unit.WattHour),
                    new TimeRegisterValue("S2", t2 + skew, 6, 3, Unit.WattHour)
                }
            }
        });

        var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls1 });
        exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.AddHours(-5).ToString("o")}&to={today.ToString("o")}&label={ls1.Label}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportGaugesRoot>();

        Assert.That(json.series, Has.Length.EqualTo(2));
        AssertExportGaugeSeries(ls1.Label, obisCode1, new dynamic[] 
        {
        new { Timestamp = t1, Value = 2, Unit = "kWh", DeviceId = "S1" },
        new { Timestamp = t2, Value = 3, Unit = "kWh", DeviceId = "S1" },
        }, json.series[0]);
        AssertExportGaugeSeries(ls1.Label, obisCode2, new dynamic[] 
        {
        new { Timestamp = t1 + skew, Value = 4, Unit = "kWh", DeviceId = "S2" },
        new { Timestamp = t2 + skew, Value = 6, Unit = "kWh", DeviceId = "S2" },
        }, json.series[1]);
    }

    [Test]
    public async Task GetHourlyGaugesExportTwoSeriesTwoLabels()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var skew = TimeSpan.FromMinutes(5); // To verify normalization.
        var t1 = today - TimeSpan.FromHours(2) + skew;
        var t2 = today - TimeSpan.FromHours(1) + skew;
        var obisCode = ObisCode.ElectrActiveEnergyA14;
        var ls1 = CreateLabelSeries("label1", obisCode, new[]
        {
            new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
            new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre)
        });
        var ls2 = CreateLabelSeries("label2", obisCode, new[]
        {
            new TimeRegisterValue("S2", t1 + skew, 4, 3, Unit.WattHour),
            new TimeRegisterValue("S2", t2 + skew, 6, 3, Unit.WattHour)
        }
        );
        var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls1, ls2 });
        exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.AddHours(-5).ToString("o")}&to={today.ToString("o")}&label={ls1.Label}&label={ls2.Label}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportGaugesRoot>();

        Assert.That(json.series, Has.Length.EqualTo(2));
        AssertExportGaugeSeries(ls1.Label, obisCode, new dynamic[] 
        {
            new { Timestamp = t1, Value = 200, Unit = "m3", DeviceId = "S1" },
            new { Timestamp = t2, Value = 300, Unit = "m3", DeviceId = "S1" },
        }, json.series[0]);
        AssertExportGaugeSeries(ls2.Label, obisCode, new dynamic[] 
        {
            new { Timestamp = t1 + skew, Value = 4, Unit = "kWh", DeviceId = "S2" },
            new { Timestamp = t2 + skew, Value = 6, Unit = "kWh", DeviceId = "S2" },
        }, json.series[1]);
    }

    [Test]
    public async Task GetHourlyGaugesExportValueAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var skew = TimeSpan.FromMinutes(5); // To verify normalization.
        var t1 = today - TimeSpan.FromHours(3) + skew;
        var t2 = today - TimeSpan.FromHours(2) + skew;
        var t3 = today - TimeSpan.FromHours(1) + skew;
        var obisCode = ObisCode.ElectrActiveEnergyA14;
        var ls1 = CreateLabelSeries("label1", obisCode, new[]
        {
          new TimeRegisterValue("S1", t1, 2, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t2, 3, 2, Unit.CubicMetre),
          new TimeRegisterValue("S1", t3, 3, 2, Unit.CubicMetre),
        }
        );
        var ls2 = CreateLabelSeries("label2", obisCode, new[]
        {
            new TimeRegisterValue("S2", t1, 4, 3, Unit.WattHour),
            // t2 missing
            new TimeRegisterValue("S2", t3, 6, 3, Unit.WattHour)
        }
        );
        var lss = new TimeRegisterValueLabelSeriesSet(t1, t2, new[] { ls1, ls2 });
        exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>())).Returns(lss);

        // Act
        var response = await httpClient.GetAsync($"api/export/gauges/hourly?from={today.AddHours(-5).ToString("o")}&to={today.ToString("o")}&label={ls1.Label}&label={ls2.Label}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ExportGaugesRoot>();

        Assert.That(json.series, Has.Length.EqualTo(2));
        AssertExportGaugeSeries(ls2.Label, obisCode, new dynamic[] 
        {
            new { Timestamp = t1, Value = 4, Unit = "kWh", DeviceId = "S2" },
            new { Timestamp = (DateTime?)null, Value = (double?)null, Unit = (string)null, DeviceId = (string)null },
            new { Timestamp = t3, Value = 6, Unit = "kWh", DeviceId = "S2" },
        }, json.series[1]);
    }

    private void SetupSeriesNameRepositoryGetStoredSeriesNames(params SeriesName[] seriesNames)
    {
        seriesNameRepository.Setup(x => x.GetSeriesNames()).Returns(seriesNames);
    }

    private void SetupExportRepositoryGetLiveCumulativeSeries(TimeRegisterValueLabelSeriesSet lss)
    {
        exportRepository.Setup(er => er.GetLiveCumulativeSeries(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IList<string>>()))
            .Returns(lss);
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

    private static void AssertPeriods(IEnumerable<ExportPeriod> expected, ExportDiffPeriod[] actual)
    {
        Assert.That(
            actual.Select(x => new ExportPeriod(
                DateTime.Parse(x.from, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), 
                DateTime.Parse(x.to, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            )).ToArray(), 
            Is.EqualTo(expected.ToArray())
        );
    }

    private static void AssertExportDiffSeries(string label, ObisCode obisCode, dynamic[] registers, ExportDiffSeries actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.label, Is.EqualTo(label));
        Assert.That(actual.obisCode, Is.EqualTo(obisCode.ToString()));
        Assert.That(actual.values, Has.Length.EqualTo(registers.Length));
        for (var ix = 0; ix < registers.Length; ix++)
        {
            var actualRegister = actual.values[ix];
            dynamic expectedRegister = registers[ix];
            Assert.That(DateTime.Parse(actualRegister.from, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), Is.EqualTo(expectedRegister.From));
            Assert.That(DateTime.Parse(actualRegister.to, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), Is.EqualTo(expectedRegister.To));
            Assert.That(actualRegister.value, Is.EqualTo(expectedRegister.Value));
            Assert.That(actualRegister.unit, Is.EqualTo(expectedRegister.Unit));
        }
    }

    private static void AssertExportGaugeSeries(string label, ObisCode obisCode, dynamic[] registers, ExportGaugeSeries actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.label, Is.EqualTo(label));
        Assert.That(actual.obisCode, Is.EqualTo(obisCode.ToString()));
        Assert.That(actual.values, Has.Length.EqualTo(registers.Length));
        for (var ix = 0; ix < registers.Length; ix++)
        {
            var actualRegister = actual.values[ix];
            dynamic expectedRegister = registers[ix];
            Assert.That(actualRegister.timestamp != null, Is.EqualTo(expectedRegister.Timestamp != null));
            if (expectedRegister.Timestamp != null)
            {
                Assert.That(DateTime.Parse(actualRegister.timestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), Is.EqualTo(expectedRegister.Timestamp));
            }
            Assert.That(actualRegister.value, Is.EqualTo(expectedRegister.Value));
            Assert.That(actualRegister.unit, Is.EqualTo(expectedRegister.Unit));
            Assert.That(actualRegister.deviceId, Is.EqualTo(expectedRegister.DeviceId));
        }
    }

    internal class ExportDiffsRoot
    {
        public ExportDiffPeriod[] periods { get; set; }
        public ExportDiffSeries[] series { get; set; }
    }

    internal class ExportDiffPeriod
    {
        public string from { get; set; }
        public string to { get; set; }
    }

    internal class ExportDiffSeries
    {
        public string label { get; set; }
        public string obisCode { get; set; }
        public ExportDiffRegister[] values { get; set; }
    }

    internal class ExportDiffRegister
    {
        public string from { get; set; }
        public string to { get; set; }
        public double? value { get; set; }
        public string unit { get; set; }
    }

    internal class ExportGaugesRoot
    {
        public string[] timestamps { get; set; }
        public ExportGaugeSeries[] series { get; set; }
    }

    internal class ExportGaugeSeries
    {
        public string label { get; set; }
        public string obisCode { get; set; }
        public ExportGaugeRegister[] values { get; set; }
    }

    internal class ExportGaugeRegister
    {
        public string timestamp { get; set; }
        public double? value { get; set; }
        public string unit { get; set; }
        public string deviceId { get; set; }
    }

}
