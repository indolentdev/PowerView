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
using PowerView.Service.Mappers;

namespace PowerView.Service.IntegrationTest;

public class DataCrudeControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<ICrudeDataRepository> crudeDataRepository;
    private Mock<IReadingHistoryRepository> readingHistoryRepository;

    [SetUp]
    public void Setup()
    {
        crudeDataRepository = new Mock<ICrudeDataRepository>();
        readingHistoryRepository = new Mock<IReadingHistoryRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(crudeDataRepository.Object);
                    sc.AddSingleton(readingHistoryRepository.Object);
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

    [Test]
    public async Task GetCrudeDataByLabelAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/by/");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeDataBy(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataByLabelEmpty()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/by//{today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeDataBy(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataByTimestampAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/by/lbl/");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeDataBy(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataByTimestampBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/by/lbl/bad");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeDataBy(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataByTimestampNotUtc()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var local = DateTime.SpecifyKind(today, DateTimeKind.Unspecified);

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/by/lbl/{local.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeDataBy(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public async Task GetCrudeDataByCallsRepository()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        SetupCrudeDataRepositoryGetCrudeDataBy();

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/by/lbl/{today.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        crudeDataRepository.Verify(cdr => cdr.GetCrudeDataBy(It.Is<string>(p => p == "lbl"), It.Is<DateTime>(p => p == today && p.Kind == today.Kind)));
    }

    [Test]
    public async Task GetCrudeDataBy()
    {
        // Arrange
        var baseTime = new DateTime(2022, 10, 18, 19, 39, 12, DateTimeKind.Utc);
        var value1 = new CrudeDataValue(baseTime, ObisCode.ElectrActiveEnergyA14, 123, 1, Unit.WattHour, "SN1");
        var value2 = new CrudeDataValue(baseTime, ObisCode.ElectrActiveEnergyA23, 321, 1, Unit.WattHour, "SN1");
        SetupCrudeDataRepositoryGetCrudeDataBy(value1, value2);

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/by/lbl/{baseTime.ToString("o")}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestCrudeDataValueDto[]>();
        Assert.That(json.Length, Is.EqualTo(2));
        AssertCrudeDataVaue(value1, json[0]);
        AssertCrudeDataVaue(value2, json[1]);
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

    [Test]
    public async Task DeleteCrudeDataLabelAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/data/crude/values");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        crudeDataRepository.Verify(cdr => cdr.DeleteCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<ObisCode>()), Times.Never);
        readingHistoryRepository.Verify(rhr => rhr.ClearDayMonthYearHistory(), Times.Never);
    }

    [Test]
    public async Task DeleteCrudeDataLabelEmpty()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.DeleteAsync($"api/data/crude/values//{today.ToString("o")}/1.2.3.4.5.6");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        crudeDataRepository.Verify(cdr => cdr.DeleteCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<ObisCode>()), Times.Never);
        readingHistoryRepository.Verify(rhr => rhr.ClearDayMonthYearHistory(), Times.Never);
    }

    [Test]
    public async Task DeleteCrudeDataTimestampAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.DeleteAsync($"api/data/crude/values/lbl/");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        crudeDataRepository.Verify(cdr => cdr.DeleteCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<ObisCode>()), Times.Never);
        readingHistoryRepository.Verify(rhr => rhr.ClearDayMonthYearHistory(), Times.Never);
    }

    [Test]
    public async Task DeleteCrudeDataTimestampBadFormat()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/data/crude/values/lbl/bad/1.2.3.4.5.6");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.DeleteCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<ObisCode>()), Times.Never);
        readingHistoryRepository.Verify(rhr => rhr.ClearDayMonthYearHistory(), Times.Never);
    }

    [Test]
    public async Task DeleteCrudeDataTimestampNotUtc()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        var local = DateTime.SpecifyKind(today, DateTimeKind.Unspecified);

        // Act
        var response = await httpClient.DeleteAsync($"api/data/crude/values/lbl/{local.ToString("o")}/1.2.3.4.5.6");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.DeleteCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<ObisCode>()), Times.Never);
        readingHistoryRepository.Verify(rhr => rhr.ClearDayMonthYearHistory(), Times.Never);
    }

    [Test]
    public async Task DeleteCrudeDataObisCodeAbsent()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.DeleteAsync($"api/data/crude/values/lbl/{today.ToString("o")}/");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        crudeDataRepository.Verify(cdr => cdr.DeleteCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<ObisCode>()), Times.Never);
        readingHistoryRepository.Verify(rhr => rhr.ClearDayMonthYearHistory(), Times.Never);
    }

    [Test]
    public async Task DeleteCrudeDataObisCodeBadFormat()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.DeleteAsync($"api/data/crude/values/lbl/{today.ToString("o")}/1.2.bad.4.5.6");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.DeleteCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<ObisCode>()), Times.Never);
        readingHistoryRepository.Verify(rhr => rhr.ClearDayMonthYearHistory(), Times.Never);
    }

    [Test]
    public async Task DeleteCrudeDataCallsRepository()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();

        // Act
        var response = await httpClient.DeleteAsync($"api/data/crude/values/lbl/{today.ToString("o")}/1.2.3.4.5.6");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        crudeDataRepository.Verify(cdr => cdr.DeleteCrudeData(It.Is<string>(p => p == "lbl"), 
          It.Is<DateTime>(p => p == today && p.Kind == today.Kind), It.Is<ObisCode>(p => p == (ObisCode)"1.2.3.4.5.6") ));
        readingHistoryRepository.Verify(rhr => rhr.ClearDayMonthYearHistory());
    }

    [Test]
    public async Task DeleteCrudeData()
    {
        // Arrange
        var baseTime = new DateTime(2022, 10, 18, 19, 39, 12, DateTimeKind.Utc);

        // Act
        var response = await httpClient.DeleteAsync($"api/data/crude/values/lbl/{baseTime.ToString("o")}/1.2.3.4.5.6");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task GetMissingDaysLabelAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/missing-days");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.GetMissingDays(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetMissingDaysLabelEmpty()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/missing-days?label=");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        crudeDataRepository.Verify(cdr => cdr.GetMissingDays(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetMissingDaysCallsRepository()
    {
        // Arrange
        SetupCrudeDataRepositoryGetMissingDays();

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/missing-days?label=lbl");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        crudeDataRepository.Verify(cdr => cdr.GetMissingDays(It.Is<string>(p => p == "lbl")));
    }

    [Test]
    public async Task GetMissingDays()
    {
        // Arrange
        var baseTime = new DateTime(2022, 10, 18, 19, 39, 12, DateTimeKind.Utc);
        var previousDateTime = new DateTime(2022, 10, 17, 19, 39, 12, DateTimeKind.Utc);
        var nextDateTime = new DateTime(2022, 10, 20, 19, 39, 12, DateTimeKind.Utc);
        var missingDay1 = new MissingDate(baseTime, previousDateTime, nextDateTime);
        var missingDay2 = new MissingDate(baseTime.AddDays(1), previousDateTime, nextDateTime);
        SetupCrudeDataRepositoryGetMissingDays(missingDay1, missingDay2);

        // Act
        var response = await httpClient.GetAsync($"api/data/crude/missing-days?label=lbl");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestMissingDateDto[]>();
        AssertMissingDate(missingDay1, json.First());
        AssertMissingDate(missingDay2, json.Last());
    }

    private static void AssertMissingDate(MissingDate expected, TestMissingDateDto actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.timestamp, Is.EqualTo(expected.Timestamp.ToString("o").Substring(0, 19) + "Z"));
        Assert.That(actual.previousTimestamp, Is.EqualTo(expected.PreviousTimestamp.ToString("o").Substring(0, 19) + "Z"));
        Assert.That(actual.nextTimestamp, Is.EqualTo(expected.NextTimestamp.ToString("o").Substring(0, 19) + "Z"));
    }

    private void SetupCrudeDataRepositoryGetCrudeData(int totalCount, params CrudeDataValue[] values)
    {
        crudeDataRepository.Setup(x => x.GetCrudeData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new WithCount<ICollection<CrudeDataValue>>(totalCount, values));
    }

    private void SetupCrudeDataRepositoryGetCrudeDataBy(params CrudeDataValue[] values)
    {
        crudeDataRepository.Setup(x => x.GetCrudeDataBy(It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(values);
    }

    private void SetupCrudeDataRepositoryGetMissingDays(params MissingDate[] missingDays)
    {
        crudeDataRepository.Setup(x => x.GetMissingDays(It.IsAny<string>()))
            .Returns(missingDays);
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

    internal class TestMissingDateDto
    {
        public string timestamp { get; set; }
        public string previousTimestamp { get; set; }
        public string nextTimestamp { get; set; }
    }

}