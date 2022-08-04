using System;
using System.Collections.Generic;
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

namespace PowerView.Service.IntegrationTest;

public class DiffControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<IProfileRepository> profileRepository;

    [SetUp]
    public void Setup()
    {
        profileRepository = new Mock<IProfileRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(profileRepository.Object);
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
    public async Task GetDiff_CallsProfileRepository()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var utcOneDay = today.AddDays(1);
        SetupProfileRepositoryGetMonthProfileSet();

        // Act
        var response = await httpClient.GetAsync($"api/diff?from={today.ToString("o")}&to={utcOneDay.ToString("o")}");

        // Assert
        profileRepository.Verify(x => x.GetMonthProfileSet(
            It.Is<DateTime>(x => x == today - TimeSpan.FromHours(12) && x.Kind == today.Kind),
            It.Is<DateTime>(x => x == today && x.Kind == today.Kind),
            It.Is<DateTime>(x => x == utcOneDay && x.Kind == utcOneDay.Kind)
        ));
    }

    [Test]
    public async Task GetDiff()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var t1 = today - TimeSpan.FromDays(2);
        var t2 = today - TimeSpan.FromDays(1);
        var label1Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>
        {
            {"8.0.1.0.0.255", new [] { new TimeRegisterValue("1", t1, 2, 2, Unit.CubicMetre), new TimeRegisterValue("1", t2, 3, 2, Unit.CubicMetre) } }
        };
        var label2Values = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>
        {
            {"1.0.1.8.0.255", new [] { new TimeRegisterValue("2", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("2", t2, 3, 6, Unit.WattHour) } },
            {"1.0.2.8.0.255", new [] { new TimeRegisterValue("2", t1, 4, 6, Unit.WattHour) } }
        };
        SetupProfileRepositoryGetMonthProfileSet(new TimeRegisterValueLabelSeries("Label1", label1Values), new TimeRegisterValueLabelSeries("Label2", label2Values));

        // Act
        var response = await httpClient.GetAsync($"api/diff?from={t1.ToString("o")}&to={today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<DiffRoot>();
        Assert.That(json.from, Is.EqualTo(t1.ToString("o")));
        Assert.That(json.to, Is.EqualTo(today.ToString("o")));

        Assert.That(json.registers, Has.Length.EqualTo(2));
        AssertDiffRegister("Label1", ObisCode.ColdWaterVolume1Period, t1, t2, 100, "m3", json.registers.First());
        AssertDiffRegister("Label2", ObisCode.ElectrActiveEnergyA14Period, t1, t2, 1000, "kWh", json.registers.Last());
    }

    private static void AssertDiffRegister(string label, ObisCode obisCode, DateTime from, DateTime to, double value, string unit, DiffRegister actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.label, Is.EqualTo(label));
        Assert.That(actual.obisCode, Is.EqualTo(obisCode.ToString()));
        Assert.That(actual.from, Is.EqualTo(from.ToString("o")));
        Assert.That(actual.to, Is.EqualTo(to.ToString("o")));
        Assert.That(actual.value, Is.EqualTo(value));
        Assert.That(actual.unit, Is.EqualTo(unit));
    }

    internal class DiffRoot
    {
        public string from { get; set; }
        public string to { get; set; }
        public DiffRegister[] registers { get; set; }
    }

    internal class DiffRegister
    {
        public string label { get; set; }
        public string obisCode { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public double value { get; set; }
        public string unit { get; set; }
    }

    [Test]
    public async Task GetDiffFromAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/diff?to={today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileRepository.Verify(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetDiffToAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/diff?from={today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileRepository.Verify(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetDiffFromBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/diff?from=BadFormat&to={today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileRepository.Verify(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetDiffToBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/diff?from={today.ToString("o")}&to=BadFormat");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileRepository.Verify(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetDiffFromNotUtc()
    {
        // Arrange
        var today = DateTime.SpecifyKind(TimeZoneHelper.GetDenmarkTodayAsUtc(), DateTimeKind.Local);

        // Act
        var response = await httpClient.GetAsync($"api/diff?from=BadFormat&to={today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileRepository.Verify(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetDiffToNotUtc()
    {
        // Arrange
        var today = DateTime.SpecifyKind(TimeZoneHelper.GetDenmarkTodayAsUtc(), DateTimeKind.Local);

        // Act
        var response = await httpClient.GetAsync($"api/diff?from={today.ToString("o")}&to=BadFormat");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        profileRepository.Verify(pr => pr.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    private void SetupProfileRepositoryGetMonthProfileSet(params TimeRegisterValueLabelSeries[] series)
    {
        profileRepository.Setup(x => x.GetMonthProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns<DateTime, DateTime, DateTime>((preStart, start, end) => new TimeRegisterValueLabelSeriesSet(start, end, series));
    }
}
