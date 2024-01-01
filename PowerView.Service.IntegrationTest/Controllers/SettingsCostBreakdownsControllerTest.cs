using System;
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
using PowerView.Service.Dtos;

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class SettingsCostBreakdownsControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<ICostBreakdownRepository> costBreakdownRepository;

    [SetUp]
    public void SetUp()
    {
        costBreakdownRepository = new Mock<ICostBreakdownRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(costBreakdownRepository.Object);
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
    public async Task GetCostBreakdowns()
    {
        // Arrange
        DateTime dateTime = new DateTime(2023, 9, 27, 19, 0, 3, DateTimeKind.Utc);
        var costBreakdown1 = new CostBreakdown("Title1", Unit.Eur, 10, new [] { 
            new CostBreakdownEntry(dateTime.AddDays(1), dateTime.AddDays(2), "N1-1", 2, 21, 1.2345678), 
            new CostBreakdownEntry(dateTime.AddDays(1), dateTime.AddDays(2), "N1-2", 0, 23, 2.222222),
            new CostBreakdownEntry(dateTime, dateTime.AddDays(1), "N1-3", 0, 19, 3),
            new CostBreakdownEntry(dateTime.AddDays(2), dateTime.AddDays(3), "N1-4", 0, 18, 4.444)
        });
        var costBreakdown2 = new CostBreakdown("Title2", Unit.Dkk, 25, new[] { 
            new CostBreakdownEntry(dateTime, dateTime.AddDays(2), "N2-1", 3, 20, 2.345678) 
        });
        costBreakdownRepository.Setup(cbr => cbr.GetCostBreakdowns()).Returns(new[] { costBreakdown1, costBreakdown2 });

        // Act
        var response = await httpClient.GetAsync($"api/settings/costbreakdowns");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestCostBreakdownSetDto>();
        Assert.That(json.costBreakdowns.Length, Is.EqualTo(2));
        AssertCostBreakdown(costBreakdown1, new [] { new[] { costBreakdown1.Entries[3] }, new [] { costBreakdown1.Entries[0], costBreakdown1.Entries[1] }, new [] { costBreakdown1.Entries[2] } }, json.costBreakdowns[0]);
        AssertCostBreakdown(costBreakdown2, new [] { new [] { costBreakdown2.Entries[0] } }, json.costBreakdowns[1]);
        costBreakdownRepository.Verify(cbr => cbr.GetCostBreakdowns());
    }

    [Test]
    public async Task PostCostBreakdown()
    {
        // Arrange
        var costBreakdown = new
        {
            Title = "The Title",
            Currency = "DKK",
            Vat = 22
        };
        var content = JsonContent.Create(costBreakdown);

        // Act
        var response = await httpClient.PostAsync($"api/settings/costbreakdowns", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        costBreakdownRepository.Verify(cbr => cbr.AddCostBreakdown(It.Is<CostBreakdown>(cb =>
          cb.Title == costBreakdown.Title && cb.Currency.ToString().ToUpperInvariant() == costBreakdown.Currency &&
          cb.Vat == costBreakdown.Vat && cb.Entries.Count == 0)));
    }

    [Test]
    public async Task PostCostBreakdownNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync($"api/settings/costbreakdowns", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostCostBreakdownBad()
    {
        // Arrange
        var costBreakdown = new { };
        var content = JsonContent.Create(costBreakdown);

        // Act
        var response = await httpClient.PostAsync($"api/settings/costbreakdowns", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostCostBreakdownAlreadyExists()
    {
        // Arrange
        costBreakdownRepository.Setup(cbr => cbr.AddCostBreakdown(It.IsAny<CostBreakdown>())).Throws(new DataStoreUniqueConstraintException());
        var costBreakdown = new
        {
            Title = "The Title",
            Currency = "DKK",
            Vat = 22
        };
        var content = JsonContent.Create(costBreakdown);

        // Act
        var response = await httpClient.PostAsync($"api/settings/costbreakdowns", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task DeleteCostBreakdown()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/costbreakdowns/TheTitle");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        costBreakdownRepository.Verify(err => err.DeleteCostBreakdown("TheTitle"));
    }

    [Test]
    public async Task DeleteCostBreakdownTitleParameterBad()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/costbreakdowns/12345678901234567890123456");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(err => err.DeleteCostBreakdown(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task PostCostBreakdownEntry()
    {
        // Arrange
        const string title = "theTitle";
        var costBreakdownEntry = new CostBreakdownEntryDto
        {
            FromDate = new DateTime(2023, 10, 31, 23, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2023, 11, 30, 23, 0, 0, DateTimeKind.Utc),
            Name = "TheName", StartTime = 0, EndTime = 23, Amount = 12.345678
        };
        var content = JsonContent.Create(costBreakdownEntry);

        // Act
        var response = await httpClient.PostAsync($"api/settings/costbreakdowns/{title}/entries", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        costBreakdownRepository.Verify(cbr => cbr.AddCostBreakdownEntry(It.Is<string>(t => t == title), It.Is<CostBreakdownEntry>(cbe =>
          cbe.FromDate == costBreakdownEntry.FromDate && cbe.ToDate == costBreakdownEntry.ToDate &&
          cbe.Name == costBreakdownEntry.Name && cbe.StartTime == costBreakdownEntry.StartTime &&
          cbe.EndTime == costBreakdownEntry.EndTime && cbe.Amount == costBreakdownEntry.Amount)));
    }

    [Test]
    public async Task PostCostBreakdownEntryTitleParameterBad()
    {
        // Arrange
        const string title = "12345678901234567890123456";
        var costBreakdownEntry = new CostBreakdownEntryDto
        {
            FromDate = new DateTime(2023, 10, 31, 23, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2023, 11, 30, 23, 0, 0, DateTimeKind.Utc),
            Name = "TheName", StartTime = 0, EndTime = 23, Amount = 12.345678
        };
        var content = JsonContent.Create(costBreakdownEntry);

        // Act
        var response = await httpClient.PostAsync($"api/settings/costbreakdowns/{title}/entries", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(err => err.AddCostBreakdownEntry(It.IsAny<string>(), It.IsAny<CostBreakdownEntry>()), Times.Never);
    }

    [Test]
    public async Task PostCostBreakdownEntryContentBad()
    {
        // Arrange
        const string title = "theTitle";
        var costBreakdownEntry = new CostBreakdownEntryDto { };
        var content = JsonContent.Create(costBreakdownEntry);

        // Act
        var response = await httpClient.PostAsync($"api/settings/costbreakdowns/{title}/entries", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(err => err.AddCostBreakdownEntry(It.IsAny<string>(), It.IsAny<CostBreakdownEntry>()), Times.Never);
    }

    [Test]
    public async Task PostCostBreakdownEntryCostBreakdownAbsent()
    {
        // Arrange
        const string title = "theTitle";
        var costBreakdownEntry = new CostBreakdownEntryDto
        {
            FromDate = new DateTime(2023, 10, 31, 23, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2023, 11, 30, 23, 0, 0, DateTimeKind.Utc),
            Name = "TheName", StartTime = 0, EndTime = 23, Amount = 12.345678
        };
        var content = JsonContent.Create(costBreakdownEntry);
        costBreakdownRepository.Setup(x => x.AddCostBreakdownEntry(It.IsAny<string>(), It.IsAny<CostBreakdownEntry>()))
            .Throws(new DataStoreException());

        // Act
        var response = await httpClient.PostAsync($"api/settings/costbreakdowns/{title}/entries", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostCostBreakdownEntryCostBreakdownDuplicate()
    {
        // Arrange
        const string title = "theTitle";
        var costBreakdownEntry = new CostBreakdownEntryDto
        {
            FromDate = new DateTime(2023, 10, 31, 23, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2023, 11, 30, 23, 0, 0, DateTimeKind.Utc),
            Name = "TheName", StartTime = 0, EndTime = 23, Amount = 12.345678
        };
        var content = JsonContent.Create(costBreakdownEntry);
        costBreakdownRepository.Setup(x => x.AddCostBreakdownEntry(It.IsAny<string>(), It.IsAny<CostBreakdownEntry>()))
            .Throws(new DataStoreUniqueConstraintException());

        // Act
        var response = await httpClient.PostAsync($"api/settings/costbreakdowns/{title}/entries", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task PutCostBreakdownEntry()
    {
        // Arrange
        const string title = "theTitle";
        var fromDate = new DateTime(2022, 10, 31, 23, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2022, 11, 30, 23, 0, 0, DateTimeKind.Utc);
        const string name = "oldName";
        var costBreakdownEntry = new CostBreakdownEntryDto
        {
            FromDate = new DateTime(2023, 10, 31, 23, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2023, 11, 30, 23, 0, 0, DateTimeKind.Utc),
            Name = "TheName", StartTime = 0, EndTime = 23, Amount = 12.345678
        };
        var content = JsonContent.Create(costBreakdownEntry);

        // Act
        var response = await httpClient.PutAsync($"api/settings/costbreakdowns/{title}/entries/{fromDate.ToString("O")}/{toDate.ToString("O")}/{name}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        costBreakdownRepository.Verify(cbr => cbr.UpdateCostBreakdownEntry(It.Is<string>(t => t == title), It.Is<DateTime>(p => p == fromDate),
          It.Is<DateTime>(p => p == toDate), It.Is<string>(p => p == name), It.Is<CostBreakdownEntry>(cbe =>
            cbe.FromDate == costBreakdownEntry.FromDate && cbe.ToDate == costBreakdownEntry.ToDate &&
            cbe.Name == costBreakdownEntry.Name && cbe.StartTime == costBreakdownEntry.StartTime &&
            cbe.EndTime == costBreakdownEntry.EndTime && cbe.Amount == costBreakdownEntry.Amount)));
    }

    [Test]
    [TestCase("", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.NotFound)]
    [TestCase("12345678901234567890123456", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.NotFound)]
    [TestCase("theTitle", "2022-10-31T23:00:00", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "BAD", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "", "oldName", HttpStatusCode.NotFound)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "BAD", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00Z", "", HttpStatusCode.NotFound)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00Z", "12345678901234567890123456", HttpStatusCode.BadRequest)]
    public async Task PutCostBreakdownEntryRouteParameterBad(string title, string fromDate, string toDate, string name, HttpStatusCode statusCode)
    {
        // Arrange
        var costBreakdownEntry = new CostBreakdownEntryDto
        {
            FromDate = new DateTime(2023, 10, 31, 23, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2023, 11, 30, 23, 0, 0, DateTimeKind.Utc),
            Name = "TheName", StartTime = 0, EndTime = 23, Amount = 12.345678
        };
        var content = JsonContent.Create(costBreakdownEntry);

        // Act
        var response = await httpClient.PutAsync($"api/settings/costbreakdowns/{title}/entries/{fromDate}/{toDate}/{name}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(statusCode));
        costBreakdownRepository.Verify(cbr => cbr.UpdateCostBreakdownEntry(It.IsAny<string>(), It.IsAny<DateTime>(),
          It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<CostBreakdownEntry>()), Times.Never);
    }

    [Test]
    public async Task PutCostBreakdownEntryContentBad()
    {
        // Arrange
        const string title = "theTitle";
        var fromDate = new DateTime(2022, 10, 31, 23, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2022, 11, 30, 23, 0, 0, DateTimeKind.Utc);
        const string name = "oldName";
        var costBreakdownEntry = new CostBreakdownEntryDto { };
        var content = JsonContent.Create(costBreakdownEntry);

        // Act
        var response = await httpClient.PutAsync($"api/settings/costbreakdowns/{title}/entries/{fromDate.ToString("O")}/{toDate.ToString("O")}/{name}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        costBreakdownRepository.Verify(cbr => cbr.UpdateCostBreakdownEntry(It.IsAny<string>(), It.IsAny<DateTime>(),
          It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<CostBreakdownEntry>()), Times.Never);
    }

    [Test]
    public async Task PutCostBreakdownEntryCostBreakdownEntryAbsent()
    {
        // Arrange
        const string title = "theTitle";
        var fromDate = new DateTime(2022, 10, 31, 23, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2022, 11, 30, 23, 0, 0, DateTimeKind.Utc);
        const string name = "oldName";
        var costBreakdownEntry = new CostBreakdownEntryDto
        {
            FromDate = new DateTime(2023, 10, 31, 23, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2023, 11, 30, 23, 0, 0, DateTimeKind.Utc),
            Name = "TheName", StartTime = 0, EndTime = 23, Amount = 12.345678
        };
        var content = JsonContent.Create(costBreakdownEntry);
        costBreakdownRepository.Setup(cbr => cbr.UpdateCostBreakdownEntry(It.IsAny<string>(), It.IsAny<DateTime>(),
          It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<CostBreakdownEntry>())).Throws(new DataStoreException());

        // Act
        var response = await httpClient.PutAsync($"api/settings/costbreakdowns/{title}/entries/{fromDate.ToString("O")}/{toDate.ToString("O")}/{name}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutCostBreakdownEntryCostBreakdownEntryDuplicate()
    {
        // Arrange
        const string title = "theTitle";
        var fromDate = new DateTime(2022, 10, 31, 23, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2022, 11, 30, 23, 0, 0, DateTimeKind.Utc);
        const string name = "oldName";
        var costBreakdownEntry = new CostBreakdownEntryDto
        {
            FromDate = new DateTime(2023, 10, 31, 23, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2023, 11, 30, 23, 0, 0, DateTimeKind.Utc),
            Name = "TheName", StartTime = 0, EndTime = 23, Amount = 12.345678
        };
        var content = JsonContent.Create(costBreakdownEntry);
        costBreakdownRepository.Setup(cbr => cbr.UpdateCostBreakdownEntry(It.IsAny<string>(), It.IsAny<DateTime>(),
          It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<CostBreakdownEntry>())).Throws(new DataStoreUniqueConstraintException());

        // Act
        var response = await httpClient.PutAsync($"api/settings/costbreakdowns/{title}/entries/{fromDate.ToString("O")}/{toDate.ToString("O")}/{name}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task DeleteCostBreakdownEntry()
    {
        // Arrange
        const string title = "theTitle";
        var fromDate = new DateTime(2022, 10, 31, 23, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2022, 11, 30, 23, 0, 0, DateTimeKind.Utc);
        const string name = "theName";

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/costbreakdowns/{title}/entries/{fromDate.ToString("O")}/{toDate.ToString("O")}/{name}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        costBreakdownRepository.Verify(cbr => cbr.DeleteCostBreakdownEntry(It.Is<string>(t => t == title), It.Is<DateTime>(p => p == fromDate),
          It.Is<DateTime>(p => p == toDate), It.Is<string>(p => p == name)));
    }

    [Test]
    [TestCase("", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.NotFound)]
    [TestCase("12345678901234567890123456", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.NotFound)]
    [TestCase("theTitle", "2022-10-31T23:00:00", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "BAD", "2022-11-30T23:00:00Z", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "", "oldName", HttpStatusCode.NotFound)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "BAD", "oldName", HttpStatusCode.BadRequest)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00Z", "", HttpStatusCode.NotFound)]
    [TestCase("theTitle", "2022-10-31T23:00:00Z", "2022-11-30T23:00:00Z", "12345678901234567890123456", HttpStatusCode.BadRequest)]
    public async Task DeleteCostBreakdownEntryRouteParameterBad(string title, string fromDate, string toDate, string name, HttpStatusCode statusCode)
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/costbreakdowns/{title}/entries/{fromDate}/{toDate}/{name}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(statusCode));
        costBreakdownRepository.Verify(cbr => cbr.DeleteCostBreakdownEntry(It.IsAny<string>(), It.IsAny<DateTime>(),
          It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task DeleteCostBreakdownEntryCostBreakdownAbsent()
    {
        // Arrange
        const string title = "theTitle";
        var fromDate = new DateTime(2022, 10, 31, 23, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2022, 11, 30, 23, 0, 0, DateTimeKind.Utc);
        const string name = "theName";
        costBreakdownRepository.Setup(cbr => cbr.DeleteCostBreakdownEntry(It.IsAny<string>(), It.IsAny<DateTime>(),
          It.IsAny<DateTime>(), It.IsAny<string>())).Throws(new DataStoreException());

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/costbreakdowns/{title}/entries/{fromDate.ToString("O")}/{toDate.ToString("O")}/{name}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private void AssertCostBreakdown(CostBreakdown costBreakdown, CostBreakdownEntry[][] entryPeriods, TestCostBreakdownDto dto)
    {
        Assert.That(dto.title, Is.EqualTo(costBreakdown.Title));
        Assert.That(dto.currency, Is.EqualTo(costBreakdown.Currency.ToString().ToUpperInvariant()));
        Assert.That(dto.vat, Is.EqualTo(costBreakdown.Vat));
        Assert.That(dto.entryPeriods.Length, Is.EqualTo(entryPeriods.Length));
        for (var ix = 0; ix < entryPeriods.Length; ix++)
        {
            var expectedEntryPeriods = entryPeriods[ix];
            var actualEntryPeriod = dto.entryPeriods[ix];
            Assert.That(actualEntryPeriod.entries.Length, Is.EqualTo(expectedEntryPeriods.Length));
            for ( var ix2 = 0; ix2 < expectedEntryPeriods.Length; ix2++)
            {
                AssertCostBreakdownEntry(expectedEntryPeriods[ix2], actualEntryPeriod.entries[ix2]);
            }
        }
    }

    private void AssertCostBreakdownEntry(CostBreakdownEntry costBreakdownEntry, TestCostBreakdownEntryDto dto)
    {
        Assert.That(DateTime.Parse(dto.fromDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal), Is.EqualTo(costBreakdownEntry.FromDate));
        Assert.That(DateTime.Parse(dto.toDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal), Is.EqualTo(costBreakdownEntry.ToDate));
        Assert.That(dto.name, Is.EqualTo(costBreakdownEntry.Name));
        Assert.That(dto.startTime, Is.EqualTo(costBreakdownEntry.StartTime));
        Assert.That(dto.endTime, Is.EqualTo(costBreakdownEntry.EndTime));
        Assert.That(dto.amount, Is.EqualTo(costBreakdownEntry.Amount));
    }

    internal class TestCostBreakdownSetDto
    {
        public TestCostBreakdownDto[] costBreakdowns { get; set; }
    }

    public class TestCostBreakdownDto
    {
        public string title { get; set; }

        public string currency { get; set; }

        public int? vat { get; set; }

        public TestCostBreakdownEntryPeriodDto[] entryPeriods { get; set; }
    }

    public class TestCostBreakdownEntryPeriodDto
    {
        public TestCostBreakdownPeriodDto period { get; set; }

        public TestCostBreakdownEntryDto[] entries { get; set; }
    }

    public class TestCostBreakdownPeriodDto
    {
        public string fromDate { get; set; }

        public string toDate { get; set; }
    }

    public class TestCostBreakdownEntryDto
    {
        public string fromDate { get; set; }

        public string toDate { get; set; }

        public string name { get; set; }

        public int? startTime { get; set; }

        public int? endTime { get; set; }

        public double? amount { get; set; }
    }

}
