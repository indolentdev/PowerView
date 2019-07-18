﻿using System;
using System.Collections.Generic;
using PowerView.Model;
using PowerView.Model.Expression;
using PowerView.Model.Repository;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using System.Linq;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class DiffModuleTest
  {
    private Mock<IProfileRepository> profileRepository;
    private Mock<ITemplateConfigProvider> templateConfigProvider;

    private Browser browser;

    private const string DiffRoute = "/api/diff";

    [SetUp]
    public void SetUp()
    {
      profileRepository = new Mock<IProfileRepository>();
      templateConfigProvider = new Mock<ITemplateConfigProvider>();

      templateConfigProvider.Setup(mcp => mcp.LabelObisCodeTemplates).Returns(new LabelObisCodeTemplate[0]);

      browser = new Browser(cfg =>
      {
        cfg.Module<DiffModule>();
        cfg.Dependency<IProfileRepository>(profileRepository.Object);
        cfg.Dependency<ITemplateConfigProvider>(templateConfigProvider.Object);
      });
    }

    [Test]
    public void GetDiffFromAbsent()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("to", utcNow.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileRepository.Verify(pr => pr.GetCustomProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDiffToAbsent()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", utcNow.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileRepository.Verify(pr => pr.GetCustomProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDiffToBadFormat()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", utcNow.ToString("o"));
        with.Query("to", "BadFormat");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      profileRepository.Verify(pr => pr.GetCustomProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDiffFromAndToPresent()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      profileRepository.Setup(dpr => dpr.GetCustomProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelProfileSet(utcNow, new LabelProfile[0]));

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", utcNow.ToString("o"));
        with.Query("to", (utcNow+TimeSpan.FromDays(1)).ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      profileRepository.Verify(pr => pr.GetCustomProfileSet(It.Is<DateTime>(dt => dt == utcNow && dt.Kind == utcNow.Kind), 
                                                            It.Is<DateTime>(dt => dt == utcNow+TimeSpan.FromDays(1) && dt.Kind == utcNow.Kind)));
    }

    [Test]
    public void Get()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      var t1 = utcNow-TimeSpan.FromDays(2);
      var t2 = utcNow-TimeSpan.FromDays(1);
      var label1Values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        {"8.0.1.0.0.255", new [] { new TimeRegisterValue("1", t1, 2, 2, Unit.CubicMetre), new TimeRegisterValue("1", t2, 3, 2, Unit.CubicMetre) } }
      };
      var label2Values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        {"1.0.1.8.0.255", new [] { new TimeRegisterValue("1", t1, 2, 6, Unit.WattHour), new TimeRegisterValue("1", t2, 3, 6, Unit.WattHour) } },
        {"1.0.2.8.0.255", new [] { new TimeRegisterValue("1", t1, 4, 6, Unit.WattHour) } }
      };
      var dps = new LabelProfileSet(t1, new[] { new LabelProfile("Label1", t1, label1Values), new LabelProfile("Label2", t1, label2Values) });
      profileRepository.Setup(pr => pr.GetCustomProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(dps);

      // Act
      var response = browser.Get(DiffRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Query("from", (utcNow-TimeSpan.FromDays(10)).ToString("o"));
        with.Query("to", utcNow.ToString("o"));
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<DiffRoot>();
      Assert.That(json.from, Is.EqualTo((utcNow-TimeSpan.FromDays(10)).ToString("o")));
      Assert.That(json.to, Is.EqualTo(utcNow.ToString("o")));

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

  }
}
