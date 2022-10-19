using System;
using System.Collections.Generic;
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

public class DataCrudeControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<ICrudeDataRepository> crudeDataRepository;

    [SetUp]
    public void Setup()
    {
        crudeDataRepository = new Mock<ICrudeDataRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(crudeDataRepository.Object);
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
    public async Task GetCrudeDataLabelAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/data/crude?from={today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataLabelEmpty()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/data/crude?label=&from={today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataFromAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/data/crude?label=lbl");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataFromBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/data/crude?label=lbl&from=bad");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataFromNotUtc()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var local = DateTime.SpecifyKind(today, DateTimeKind.Local);

        // Act
        var response = await httpClient.GetAsync($"api/data/crude?label=lbl&from={local.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataCallsRepository()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        SetupCrudeDataRepositoryGetCrudeData(0);

        // Act
        var response = await httpClient.GetAsync($"api/data/crude?label=lbl&from={today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeData(It.Is<string>(p => p == "lbl"), It.Is<DateTime>(p => p == today && p.Kind == today.Kind), It.IsAny<int>(), It.IsAny<int>()));
    }

    [Test]
    public async Task GetCrudeData()
    {
        // Arrange
        var baseTime = new DateTime(2022, 10, 18, 19, 39, 12, DateTimeKind.Utc);
        var value1 = new CrudeDataValue(baseTime, ObisCode.ElectrActiveEnergyA14, 123, 1, Unit.WattHour, "SN1");
        var value2 = new CrudeDataValue(baseTime, ObisCode.ElectrActiveEnergyA23, 321, 1, Unit.WattHour, "SN1");
        var value3 = new CrudeDataValue(baseTime.AddHours(1), ObisCode.ElectrActiveEnergyA14, 456, 1, Unit.WattHour, "SN1");
        SetupCrudeDataRepositoryGetCrudeData(44, value1, value2, value3);

        // Act
        var response = await httpClient.GetAsync($"api/data/crude?label=lbl&from={baseTime.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestCrudeDataSetDto>();
        Assert.That(json.label, Is.EqualTo("lbl"));
        Assert.That(json.totalCount, Is.EqualTo(44));
        AssertCrudeDataVaue(value1, json.values[0]);
        AssertCrudeDataVaue(value2, json.values[1]);
        AssertCrudeDataVaue(value3, json.values[2]);
    }

    private void SetupCrudeDataRepositoryGetCrudeData(int totalCount, params CrudeDataValue[] values)
    {
        crudeDataRepository.Setup(x => x.GetCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new WithCount<ICollection<CrudeDataValue>>(totalCount, values));
    }

    private static void AssertCrudeDataVaue(CrudeDataValue expected, TestCrudeDataValueDto actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.timestamp, Is.EqualTo(expected.DateTime.ToString("o").Substring(0, 19) + "Z"));
        Assert.That(actual.obiscode, Is.EqualTo(expected.ObisCode.ToString()));
        Assert.That(actual.value, Is.EqualTo(expected.Value));
        Assert.That(actual.scale, Is.EqualTo(expected.Scale));
        Assert.That(actual.unit, Is.EqualTo(UnitMapper.Map(expected.Unit)));
        Assert.That(actual.deviceId, Is.EqualTo(expected.DeviceId));
    }

    internal class TestCrudeDataSetDto
    {
        public string label { get; set; }
        public int totalCount { get; set; }
        public TestCrudeDataValueDto[] values { get; set; }
    }

    internal class TestCrudeDataValueDto
    {
        public string timestamp { get; set; }
        public string obiscode { get; set; }
        public int value { get; set; }
        public short scale { get; set; }
        public string unit { get; set; }
        public string deviceId { get; set; }
    }

}