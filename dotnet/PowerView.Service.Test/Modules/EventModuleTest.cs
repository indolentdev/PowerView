/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using Newtonsoft.Json;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class EventModuleTest
  {
    private Mock<IMeterEventRepository> meterEventRepository;

    private Browser browser;

    [SetUp]
    public void SetUp()
    {
      meterEventRepository = new Mock<IMeterEventRepository>();


      browser = new Browser(cfg =>
      {
        cfg.Module<EventModule>();
        cfg.Dependency(meterEventRepository.Object);
      });
    }

    [Test]
    public void GetEvents()
    {
      // Arrange
      var dt = DateTime.UtcNow;
      var meterEvent = new MeterEvent("lbl", dt, true,
                            new LeakMeterEventAmplification(dt.Subtract(TimeSpan.FromHours(20)), dt.Subtract(TimeSpan.FromHours(10)), new UnitValue(44.55d, Unit.DegreeCelsius)));
      var meterEvents = new WithCount<ICollection<MeterEvent>>(10, new[] { meterEvent });
      meterEventRepository.Setup(mer => mer.GetMeterEvents(It.IsAny<int>(), It.IsAny<int>())).Returns(meterEvents);

      // Act
      var response = browser.Get("/api/events", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestEventsDto>();
      Assert.That(json.totalCount, Is.EqualTo(meterEvents.TotalCount));
      Assert.That(json.items, Has.Length.EqualTo(1));
      AssertMeterEvent(meterEvent, json.items.First());
    }

    private static void AssertMeterEvent(MeterEvent meterEvent, TestEventItemDto eventItem)
    {
      Assert.That(eventItem.type, Is.EqualTo(meterEvent.Amplification.GetMeterEventType()));
      Assert.That(eventItem.label, Is.EqualTo(meterEvent.Label));
      Assert.That(eventItem.detectTimestamp, Is.EqualTo(meterEvent.DetectTimestamp.ToString("o")));
      Assert.That(eventItem.status, Is.EqualTo(meterEvent.Flag.ToString()));
      var leak = meterEvent.Amplification as LeakMeterEventAmplification;
      AssertDictionaryEntry(eventItem.amplification, "StartTimestamp", leak.Start.ToString("o"));
      AssertDictionaryEntry(eventItem.amplification, "EndTimestamp", leak.End.ToString("o"));
      AssertDictionaryEntry(eventItem.amplification, "Value", leak.UnitValue.Value);
      AssertDictionaryEntry(eventItem.amplification, "Unit", "C");
    }

    private static void AssertDictionaryEntry(IDictionary<string, object> d, string key, object value)
    {
      Assert.That(d.ContainsKey(key), Is.True);
      Assert.That(d[key], Is.EqualTo(value));
    }

    private string ToJson(object obj)
    {
      using (var writer = new StringWriter())
      {
        new JsonSerializer().Serialize(writer, obj);
        return writer.ToString();
      }
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
      public string status { get; set; }
      public IDictionary<string, object> amplification { get; set; }
    }

  }
}

*/