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
using PowerView.Service.Mappers;

namespace PowerView.Service.IntegrationTest;

public class DataControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<IReadingHistoryRepository> readingHistoryRepository;

    [SetUp]
    public void Setup()
    {
        readingHistoryRepository = new Mock<IReadingHistoryRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(readingHistoryRepository.Object);
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
    public async Task GetHistoryStatusCallsRepository()
    {
        // Arrange
        var today = TimeZoneHelper.GetDenmarkTodayAsUtc();
        SetupReadingHistoryRepositoryGetHistoryStatus();

        // Act
        var response = await httpClient.GetAsync("api/data/history/status");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        readingHistoryRepository.Verify(x => x.GetReadingHistoryStatus());
    }

    [Test]
    public async Task GetHistoryStatus()
    {
        // Arrange
        var baseTime = new DateTime(2022, 10, 18, 19, 39, 12, DateTimeKind.Utc);
        var label1 = "label1";
        var label2 = "label2";

        var dayLabel1Ts = (Label: label1, LatestTimestamp: baseTime);
        var dayLabel2Ts = (Label: label2, LatestTimestamp: baseTime.AddHours(1));
        var monthLabel1Ts = (Label: label1, LatestTimestamp: baseTime.AddDays(-4));
        SetupReadingHistoryRepositoryGetHistoryStatus(("Day", new [] { dayLabel1Ts, dayLabel2Ts }), ("Month", new [] { monthLabel1Ts }));

        // Act
        var response = await httpClient.GetAsync("api/data/history/status");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestHistoryStatusSetDto>();
        Assert.That(json.items.Length, Is.EqualTo(2));

        var item = json.items.FirstOrDefault(x => x.interval == "Day");
        Assert.That(item, Is.Not.Null);
        Assert.That(item.labelTimestamps.Select(x => (Label:x.label, LatestTimestamp:DateTime.Parse(x.latestTimestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind))).ToList(), 
          Is.EquivalentTo(new [] { dayLabel1Ts, dayLabel2Ts }));

        item = json.items.FirstOrDefault(x => x.interval == "Month");
        Assert.That(item, Is.Not.Null);
        Assert.That(item.labelTimestamps.Select(x => (Label: x.label, LatestTimestamp: DateTime.Parse(x.latestTimestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind))).ToList(), 
          Is.EquivalentTo(new[] { monthLabel1Ts }));
    }


    private void SetupReadingHistoryRepositoryGetHistoryStatus(params (string Interval, IList<(string Label, DateTime LatestTimestamp)> Status)[] values)
    {
        readingHistoryRepository.Setup(x => x.GetReadingHistoryStatus())
            .Returns(values);
    }

    internal class TestHistoryStatusSetDto
    {
        public TestHistoryStatusDto[] items { get; set; }
    }

    internal class TestHistoryStatusDto
    {
        public string interval { get; set; }
        public TestLabelTimestampDto[] labelTimestamps { get; set; }
    }

    internal class TestLabelTimestampDto
    {
        public string label { get; set; }
        public string latestTimestamp { get; set; }
    }

}