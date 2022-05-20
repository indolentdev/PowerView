/*
using System.Collections.Generic;
using PowerView.Model;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using PowerView.Service.DisconnectControl;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class DeviceOnDemandModuleTest
  {
    private Mock<IDisconnectControlCache> disconnectControlCache;

    private Browser browser;

    [SetUp]
    public void SetUp()
    {
      disconnectControlCache = new Mock<IDisconnectControlCache>();

      browser = new Browser(cfg =>
      {
        cfg.Module<DeviceOnDemandModule>();
        cfg.Dependency<IDisconnectControlCache>(disconnectControlCache.Object);
      });
    }

    [Test]
    public void GetOnDemand()
    {
      // Arrange
      var outputStatuses = new Dictionary<ISeriesName, bool> { 
        { new SeriesName("lbl", "0.1.96.3.10.255"), true }, { new SeriesName("lbl", "0.2.96.3.10.255"), false } };
      disconnectControlCache.Setup(dcc => dcc.GetOutputStatus(It.IsAny<string>())).Returns(outputStatuses);

      // Act
      var response = browser.Get("/api/devices/ondemand", 
                                with => { with.HttpRequest(); with.Query("label", "lbl"); } );

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      disconnectControlCache.Verify(dcc => dcc.GetOutputStatus("lbl"));
      var json = response.Body.DeserializeJson<TestOnDemandSetDto>();
      Assert.That(json.items.Length, Is.EqualTo(2));
      Assert.That(json.items[0].label, Is.EqualTo("lbl"));
      Assert.That(json.items[0].obisCode, Is.EqualTo("0.1.96.3.10.255"));
      Assert.That(json.items[0].kind, Is.EqualTo("Method"));
      Assert.That(json.items[0].index, Is.EqualTo(2));
      Assert.That(json.items[1].label, Is.EqualTo("lbl"));
      Assert.That(json.items[1].obisCode, Is.EqualTo("0.2.96.3.10.255"));
      Assert.That(json.items[1].kind, Is.EqualTo("Method"));
      Assert.That(json.items[1].index, Is.EqualTo(1));
    }

    [Test]
    public void GetOnDemandQueryParamLabelAbsent()
    {
      // Arrange

      // Act
      var response = browser.Get("/api/devices/ondemand", with => { with.HttpRequest(); });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      disconnectControlCache.Verify(dcc => dcc.GetOutputStatus(It.IsAny<string>()), Times.Never);
    }

    internal class TestOnDemandSetDto
    {
      public TestOnDemandDto[] items { get; set; }
    }

    internal class TestOnDemandDto
    {
      public string label { get; set; }
      public string obisCode { get; set; }
      public string kind { get; set; }
      public int index { get; set; }
    }


  }
}

*/