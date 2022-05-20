/*
using System;
using System.Globalization;
using System.IO;
using PowerView.Model;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using Newtonsoft.Json;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class SettingsApplicationModuleTest
  {
    private Mock<ILocationContext> locationContext;

    private Browser browser;

    private const string ApplicationRoute = "/api/settings/application";

    [SetUp]
    public void SetUp()
    {
      locationContext = new Mock<ILocationContext>();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsApplicationModule>();
        cfg.Dependency<ILocationContext>(locationContext.Object);
      });
    }

    [Test]
    public void Get()
    {
      // Arrange
      var timeZoneInfo = TimeZoneInfo.Local;
      locationContext.Setup(lc => lc.TimeZoneInfo).Returns(timeZoneInfo);
      var cultureInfo = new CultureInfo("de-DE");
      locationContext.Setup(lc => lc.CultureInfo).Returns(cultureInfo);

      // Act
      var response = browser.Get(ApplicationRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<TestApplicationDto>();
      Assert.That(json, Is.Not.Null);
      Assert.That(json.version, Is.Not.Null);
      Assert.That(json.culture, Is.EqualTo(cultureInfo.NativeName));
      Assert.That(json.timeZone, Is.EqualTo(timeZoneInfo.DisplayName));
    }

    private static string ToJson(object obj)
    {
      using (var writer = new StringWriter())
      {
        new JsonSerializer().Serialize(writer, obj);
        return writer.ToString();
      }
    }

    internal class TestApplicationDto
    {
      public string version { get; set; }
      public string culture { get; set; }
      public string timeZone { get; set; }
    }

  }
}

*/