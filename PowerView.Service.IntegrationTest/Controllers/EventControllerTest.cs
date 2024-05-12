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

public class EventControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<IMeterEventRepository> meterEventRepository;

    [SetUp]
    public void Setup()
    {
        meterEventRepository = new Mock<IMeterEventRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(meterEventRepository.Object);
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
    public async Task GetMeterEvents_CallsMeterEventRepository()
    {
        // Arrange
        SetupMeterEventRepositoryGetMeterEvents();

        // Act
        var response = await httpClient.GetAsync($"api/events");

        // Assert
        meterEventRepository.Verify(x => x.GetMeterEvents(
            It.Is<int>(x => x == 0),
            It.Is<int>(x => x == 50)
        ));
    }

    [Test]
    public async Task GetMeterEvents_Leak()
    {
        // Arrange
        var dt = new DateTime(637950601049991234, DateTimeKind.Utc);
        var meterEvent = new MeterEvent("lbl", dt, true,
                              new LeakMeterEventAmplification(dt.Subtract(TimeSpan.FromHours(20)), dt.Subtract(TimeSpan.FromHours(10)), new UnitValue(44.55d, Unit.DegreeCelsius)));
        SetupMeterEventRepositoryGetMeterEvents(meterEvent);

        // Act
        var response = await httpClient.GetAsync($"api/events");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestEventsDto>();
        Assert.That(json.totalCount, Is.EqualTo(1));
        Assert.That(json.items, Has.Length.EqualTo(1));
        AssertMeterEvent(meterEvent, json.items.First());
    }

    private static void AssertMeterEvent(MeterEvent meterEvent, TestEventItemDto eventItem)
    {
        Assert.That(eventItem.type, Is.EqualTo(meterEvent.Amplification.GetMeterEventType()));
        Assert.That(eventItem.label, Is.EqualTo(meterEvent.Label));
        Assert.That(eventItem.detectTimestamp, Is.EqualTo(meterEvent.DetectTimestamp.ToString("o")));
        Assert.That(eventItem.status, Is.EqualTo(meterEvent.Flag));
        var leak = meterEvent.Amplification as LeakMeterEventAmplification;
        AssertDictionaryEntry(eventItem.amplification, "startTimestamp", leak.Start.ToString("o"));
        AssertDictionaryEntry(eventItem.amplification, "endTimestamp", leak.End.ToString("o"));
        AssertDictionaryEntry(eventItem.amplification, "value", leak.UnitValue.Value);
        AssertDictionaryEntry(eventItem.amplification, "unit", "C");
    }

    private static void AssertDictionaryEntry(IDictionary<string, object> d, string key, object value)
    {
        Assert.That(d.ContainsKey(key), Is.True);
        Assert.That(d[key].ToString(), Is.EqualTo(value.ToString()));
    }

    internal class TestEventsDto
    {
        public int totalCount { get; set; }
        public TestEventItemDto[] items { get; set; }

    }

    internal class TestEventItemDto
    {
        public string type { get; set; }
        public string label { get; set; }
        public string eventName { get; set; }
        public string detectTimestamp { get; set; }
        public bool status { get; set; }
        public IDictionary<string, object> amplification { get; set; }
    }

    private void SetupMeterEventRepositoryGetMeterEvents(params MeterEvent[] events)
    {
        meterEventRepository.Setup(x => x.GetMeterEvents(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new WithCount<ICollection<MeterEvent>>(events.Length, events));
    }

}
