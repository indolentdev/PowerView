using System;
using System.Globalization;
using System.IO;
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
  public class SettingsApplicationModuleTest
  {
    private Mock<ILocationProvider> locationProvider;

    private Browser browser;

    private const string ApplicationRoute = "/api/settings/application";

    [SetUp]
    public void SetUp()
    {
      locationProvider = new Mock<ILocationProvider>();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsApplicationModule>();
        cfg.Dependency<ILocationProvider>(locationProvider.Object);
      });
    }

    [Test]
    public void Get()
    {
      // Arrange
      var timeZoneInfo = TimeZoneInfo.Local;
      locationProvider.Setup(lp => lp.GetTimeZone()).Returns(timeZoneInfo);
      var cultureInfo = new CultureInfo("de-DE");
      locationProvider.Setup(lp => lp.GetCultureInfo()).Returns(cultureInfo);

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

