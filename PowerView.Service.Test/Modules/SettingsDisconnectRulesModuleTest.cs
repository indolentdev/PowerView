using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class SettingsDisconnectRulesModuleTest
  {
    private Mock<IDisconnectRuleRepository> disconnectRuleRepository;
    private Mock<IDisconnectRuleMapper> disconnectRuleMapper;

    private Browser browser;

    private const string DisconnectRulesRoute = "/api/settings/disconnectrules/";
    private const string DisconnectRulesOptionsRoute = DisconnectRulesRoute + "options";
    private const string DisconnectRulesDeleteTemplateRoute = DisconnectRulesRoute + "names/{0}/{1}";

    [SetUp]
    public void SetUp()
    {
      disconnectRuleRepository = new Mock<IDisconnectRuleRepository>();
      disconnectRuleMapper = new Mock<IDisconnectRuleMapper>();

      browser = new Browser(cfg =>
      {
        cfg.Module<SettingsDisconnectRulesModule>();
        cfg.Dependency<IDisconnectRuleRepository>(disconnectRuleRepository.Object);
        cfg.Dependency<IDisconnectRuleMapper>(disconnectRuleMapper.Object);
      });
    }

    [Test]
    public void GetDisconnectRules()
    {
      // Arrange
      var dr = new Mock<IDisconnectRule>();
      var disconnectRules = new[] { dr.Object };
      disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(disconnectRules);
      var disconnectRuleDto = new DisconnectRuleDto { NameLabel = "l", NameObisCode = "OC1", EvaluationLabel = "e", EvaluationObisCode = "OC2",
        DurationMinutes = 30, DisconnectToConnectValue = 1500, ConnectToDisconnectValue = 200, Unit = "W" };
      disconnectRuleMapper.Setup(drm => drm.MapToDto(It.IsAny<IDisconnectRule>())).Returns(disconnectRuleDto);

      // Act
      var response = browser.Get(DisconnectRulesRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      disconnectRuleRepository.Verify(drr => drr.GetDisconnectRules());
      disconnectRuleMapper.Verify(drm => drm.MapToDto(dr.Object));
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = response.Body.DeserializeJson<DisconnectRuleSetDto>();
      Assert.That(json.Items, Is.Not.Null);
      Assert.That(json.Items.Length, Is.EqualTo(1));
      AssertDisconnectRuleDto(disconnectRuleDto, json.Items.First());
    }

    private static void AssertDisconnectRuleDto(DisconnectRuleDto expected, DisconnectRuleDto actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.NameLabel, Is.EqualTo(expected.NameLabel));
      Assert.That(actual.NameObisCode, Is.EqualTo(expected.NameObisCode));
      Assert.That(actual.EvaluationLabel, Is.EqualTo(expected.EvaluationLabel));
      Assert.That(actual.EvaluationObisCode, Is.EqualTo(expected.EvaluationObisCode));
      Assert.That(actual.DurationMinutes, Is.EqualTo(expected.DurationMinutes));
      Assert.That(actual.DisconnectToConnectValue, Is.EqualTo(expected.DisconnectToConnectValue));
      Assert.That(actual.ConnectToDisconnectValue, Is.EqualTo(expected.ConnectToDisconnectValue));
      Assert.That(actual.Unit, Is.EqualTo(expected.Unit));
    }

    [Test]
    public void GetDisconnectControlOptionsEmpty()
    {
      // Arrange
      disconnectRuleRepository.Setup(drr => drr.GetLatestSerieNames(It.IsAny<DateTime>())).Returns(new Dictionary<ISerieName, Unit>());
      disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(new IDisconnectRule[0]);

      // Act
      var response = browser.Get(DisconnectRulesOptionsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      var oneSecond = TimeSpan.FromSeconds(1);
      disconnectRuleRepository.Verify(drr => drr.GetLatestSerieNames(It.IsInRange(DateTime.UtcNow - oneSecond, DateTime.UtcNow + oneSecond, Range.Inclusive)));
      disconnectRuleRepository.Verify(drr => drr.GetLatestSerieNames(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc)));
      disconnectRuleRepository.Verify(drr => drr.GetDisconnectRules());
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = JObject.Parse(response.Body.AsString());
      Assert.That(json.SelectTokens("$.disconnectControlItems[*]").Count(), Is.EqualTo(0));
      Assert.That(json.SelectTokens("$.evaluationItems[*]").Count(), Is.EqualTo(0));
    }

    [Test]
    public void GetDisconnectControlOptionsEvaluationItems()
    {
      // Arrange
      disconnectRuleRepository.Setup(drr => drr.GetLatestSerieNames(It.IsAny<DateTime>())).Returns(new Dictionary<ISerieName, Unit> 
      {
        { new SerieName("label1", ObisCode.ActualPowerP14L1), Unit.Watt },
        { new SerieName("label1", ObisCode.ActualPowerP23L1), Unit.Watt },
        { new SerieName("label2", ObisCode.ActualPowerP23L2), Unit.Percentage },
        { new SerieName("label3", ObisCode.ActualPowerP23L3), Unit.DegreeCelsius },
        { new SerieName("label3", ObisCode.ActualPowerP14L1), Unit.Watt }
      });
      disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(new IDisconnectRule[0]);

      // Act
      var response = browser.Get(DisconnectRulesOptionsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = JObject.Parse(response.Body.AsString());
      Assert.That(json.SelectTokens("$.evaluationItems[*]").Count(), Is.EqualTo(3));
      Assert.That(json.SelectToken("$.evaluationItems[?(@.label == 'label1')]")["obisCode"].ToString(), Is.EqualTo(ObisCode.ActualPowerP23L1.ToString()));
      Assert.That(json.SelectToken("$.evaluationItems[?(@.label == 'label1')]")["unit"].ToString(), Is.EqualTo("W"));
      Assert.That(json.SelectToken("$.evaluationItems[?(@.label == 'label2')]")["obisCode"].ToString(), Is.EqualTo(ObisCode.ActualPowerP23L2.ToString()));
      Assert.That(json.SelectToken("$.evaluationItems[?(@.label == 'label2')]")["unit"].ToString(), Is.EqualTo("%"));
      Assert.That(json.SelectToken("$.evaluationItems[?(@.label == 'label3')]")["obisCode"].ToString(), Is.EqualTo(ObisCode.ActualPowerP23L3.ToString()));
      Assert.That(json.SelectToken("$.evaluationItems[?(@.label == 'label3')]")["unit"].ToString(), Is.EqualTo("C"));
    }

    [Test]
    public void GetDisconnectControlOptionsDisconnectControlItems()
    {
      // Arrange
      disconnectRuleRepository.Setup(drr => drr.GetLatestSerieNames(It.IsAny<DateTime>())).Returns(new Dictionary<ISerieName, Unit>
      {
        { new SerieName("label1", ObisCode.ActualPowerP14L1), Unit.Watt },
        { new SerieName("label2", "0.1.96.3.10.255"), Unit.NoUnit },
        { new SerieName("label3", "0.2.96.3.10.255"), Unit.NoUnit },
        { new SerieName("label4", ObisCode.ActualPowerP14L1), Unit.Watt }
      });
      disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(new IDisconnectRule[0]);

      // Act
      var response = browser.Get(DisconnectRulesOptionsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = JObject.Parse(response.Body.AsString());
      Assert.That(json.SelectTokens("$.disconnectControlItems[*]").Count(), Is.EqualTo(2));
      Assert.That(json.SelectToken("$.disconnectControlItems[?(@.label == 'label2')]")["obisCode"].ToString(), Is.EqualTo("0.1.96.3.10.255"));
      Assert.That(json.SelectToken("$.disconnectControlItems[?(@.label == 'label3')]")["obisCode"].ToString(), Is.EqualTo("0.2.96.3.10.255"));
    }

    [Test]
    public void GetDisconnectControlOptionsDisconnectControlItemsDisconnectRulePresent()
    {
      // Arrange
      disconnectRuleRepository.Setup(drr => drr.GetLatestSerieNames(It.IsAny<DateTime>())).Returns(new Dictionary<ISerieName, Unit>
      {
        { new SerieName("label1", ObisCode.ActualPowerP14L1), Unit.Watt },
        { new SerieName("label2", "0.1.96.3.10.255"), Unit.NoUnit },
        { new SerieName("label3", "0.2.96.3.10.255"), Unit.NoUnit },
        { new SerieName("label4", ObisCode.ActualPowerP14L1), Unit.Watt }
      });
      disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(new [] { 
        new DisconnectRule(new SerieName("label2", "0.1.96.3.10.255"), new SerieName("l", "1.2.3.4.5.6"), new TimeSpan(0, 30, 0), 2, 1, Unit.Watt) });

      // Act
      var response = browser.Get(DisconnectRulesOptionsRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      var json = JObject.Parse(response.Body.AsString());
      Assert.That(json.SelectTokens("$.disconnectControlItems[*]").Count(), Is.EqualTo(1));
      Assert.That(json.SelectToken("$.disconnectControlItems[?(@.label == 'label3')]")["obisCode"].ToString(), Is.EqualTo("0.2.96.3.10.255"));
    }

    [Test]
    public void AddDisconnectRule()
    {
      // Arrange
      var disconnectRuleDto = new DisconnectRuleDto { NameLabel = "NameLabel", NameObisCode = "1.2.3.4.5.6", EvaluationLabel = "EvalLabel",
        EvaluationObisCode = "6.5.4.3.2.1", DurationMinutes = 30, DisconnectToConnectValue = 1550, ConnectToDisconnectValue = 350, Unit = "W" };
      var disconnectRule = new DisconnectRule(new SerieName("NameLabel", "1.2.3.4.5.6"), new SerieName("EvalLabel", "6.5.4.3.2.1"), TimeSpan.FromMinutes(30),
                                              1550, 350, Unit.Watt);
      disconnectRuleMapper.Setup(drm => drm.MapFromDto(It.IsAny<DisconnectRuleDto>())).Returns(disconnectRule);

      // Act
      var response = browser.Post(DisconnectRulesRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.JsonBody(disconnectRuleDto);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      disconnectRuleMapper.Verify(drm => drm.MapFromDto(It.Is<DisconnectRuleDto>(x => x.NameLabel == disconnectRuleDto.NameLabel)));
      disconnectRuleRepository.Verify(err => err.AddDisconnectRule(disconnectRule));
    }

    [Test]
    public void AddDisconnectRuleNoContent()
    {
      // Arrange
      disconnectRuleMapper.Setup(drm => drm.MapFromDto(It.IsAny<DisconnectRuleDto>())).Throws(new ArgumentException());

      // Act
      var response = browser.Post(DisconnectRulesRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
      Assert.That(response.Body.AsString(), Contains.Substring("Disconnect rule content invalid"));
    }

    [Test]
    public void AddDisconnectRuleNameAlreadyExists()
    {
      // Arrange
      var disconnectRule = new DisconnectRule(new SerieName("NameLabel", "1.2.3.4.5.6"), new SerieName("EvalLabel", "6.5.4.3.2.1"), TimeSpan.FromMinutes(30),
                                              1550, 350, Unit.Watt);
      disconnectRuleMapper.Setup(drm => drm.MapFromDto(It.IsAny<DisconnectRuleDto>())).Returns(disconnectRule);
      disconnectRuleRepository.Setup(err => err.AddDisconnectRule(It.IsAny<DisconnectRule>())).Throws(new DataStoreUniqueConstraintException());

      // Act
      var response = browser.Post(DisconnectRulesRoute, with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
      Assert.That(response.Body.AsString(), Contains.Substring("Disconnect rule unique constraint violation"));
    }

    [Test]
    public void DeleteDisconnectRuleAbsentPath()
    {
      // Arrange

      // Act
      var response = browser.Delete(DisconnectRulesRoute + "/names", with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
      disconnectRuleRepository.Verify(drr => drr.DeleteDisconnectRule(It.IsAny<ISerieName>()), Times.Never);
    }

    [Test]
    public void DeleteDisconnectRuleBadObisCode()
    {
      // Arrange
      const string label = "label";
      const string obisCode = "bad obis code";

      // Act
      var response = browser.Delete(string.Format(CultureInfo.InvariantCulture, DisconnectRulesDeleteTemplateRoute, label, obisCode), with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
      disconnectRuleRepository.Verify(drr => drr.DeleteDisconnectRule(It.IsAny<ISerieName>()), Times.Never);
    }

    [Test]
    public void DeleteDisconnectRule()
    {
      // Arrange
      const string label = "label";
      const string obisCode = "0.1.96.3.10.255";

      // Act
      var response = browser.Delete(string.Format(CultureInfo.InvariantCulture, DisconnectRulesDeleteTemplateRoute, label, obisCode), with =>
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      disconnectRuleRepository.Verify(drr => drr.DeleteDisconnectRule(new SerieName(label, obisCode)));
    }

  }
}

