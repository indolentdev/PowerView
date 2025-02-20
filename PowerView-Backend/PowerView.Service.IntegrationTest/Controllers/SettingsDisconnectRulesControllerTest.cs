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
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class SettingsDisconnectRulesControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<IDisconnectRuleRepository> disconnectRuleRepository;

    [SetUp]
    public void SetUp()
    {
        disconnectRuleRepository = new Mock<IDisconnectRuleRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(disconnectRuleRepository.Object);
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
    public async Task GetDisconnectRules()
    {
        // Arrange
        var disconnectRule = new DisconnectRule(new SeriesName("l", "1.2.3.4.5.6"), new SeriesName("e", "6.5.4.3.2.1"), TimeSpan.FromMinutes(30), 1500, 200, Unit.Watt);
        var disconnectRules = new[] { disconnectRule };
        disconnectRuleRepository.Setup(x => x.GetDisconnectRules()).Returns(disconnectRules);

        // Act
        var response = await httpClient.GetAsync($"api/settings/disconnectrules");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<DisconnectRuleSetDto>();
        Assert.That(json.Items, Is.Not.Null);
        Assert.That(json.Items.Length, Is.EqualTo(1));
        AssertDisconnectRuleDto(disconnectRule, json.Items.First());
    }

    private static void AssertDisconnectRuleDto(DisconnectRule expected, DisconnectRuleDto actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.NameLabel, Is.EqualTo(expected.Name.Label));
        Assert.That(actual.NameObisCode, Is.EqualTo(expected.Name.ObisCode.ToString()));
        Assert.That(actual.EvaluationLabel, Is.EqualTo(expected.EvaluationName.Label));
        Assert.That(actual.EvaluationObisCode, Is.EqualTo(expected.EvaluationName.ObisCode.ToString()));
        Assert.That(actual.DurationMinutes, Is.EqualTo((int)expected.Duration.TotalMinutes));
        Assert.That(actual.DisconnectToConnectValue, Is.EqualTo(expected.DisconnectToConnectValue));
        Assert.That(actual.ConnectToDisconnectValue, Is.EqualTo(expected.ConnectToDisconnectValue));
        Assert.That(actual.Unit?.ToString(), Is.EqualTo(ValueAndUnitConverter.Convert(expected.Unit)));
    }

    [Test]
    public async Task GetDisconnectRulesCallsRepository()
    {
        // Arrange
        var disconnectRule = new DisconnectRule(new SeriesName("l", "1.2.3.4.5.6"), new SeriesName("e", "6.5.4.3.2.1"), TimeSpan.FromMinutes(30), 1500, 200, Unit.Watt);
        var disconnectRules = new[] { disconnectRule };
        disconnectRuleRepository.Setup(x => x.GetDisconnectRules()).Returns(disconnectRules);

        // Act
        var response = await httpClient.GetAsync($"api/settings/disconnectrules");

        // Assert
        disconnectRuleRepository.Verify(drr => drr.GetDisconnectRules());
    }

    [Test]
    public async Task GetDisconnectControlOptionsEmpty()
    {
        // Arrange
        disconnectRuleRepository.Setup(drr => drr.GetLatestSerieNames(It.IsAny<DateTime>())).Returns(new Dictionary<ISeriesName, Unit>());
        disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(new IDisconnectRule[0]);

        // Act
        var response = await httpClient.GetAsync($"api/settings/disconnectrules/options");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestDisconnectControlOptionDto>();
        Assert.That(json, Is.Not.Null);
        Assert.That(json.disconnectControlItems, Is.Empty);
        Assert.That(json.evaluationItems, Is.Empty);
    }

    [Test]
    public async Task GetDisconnectControlOptionsCallsRepository()
    {
        // Arrange
        disconnectRuleRepository.Setup(drr => drr.GetLatestSerieNames(It.IsAny<DateTime>())).Returns(new Dictionary<ISeriesName, Unit>());
        disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(new IDisconnectRule[0]);

        // Act
        var response = await httpClient.GetAsync($"api/settings/disconnectrules/options");

        // Assert
        var oneSecond = TimeSpan.FromSeconds(1);
        disconnectRuleRepository.Verify(drr => drr.GetLatestSerieNames(It.IsInRange(DateTime.UtcNow - oneSecond, DateTime.UtcNow + oneSecond, Moq.Range.Inclusive)));
        disconnectRuleRepository.Verify(drr => drr.GetLatestSerieNames(It.Is<DateTime>(dt => dt.Kind == DateTimeKind.Utc)));
        disconnectRuleRepository.Verify(drr => drr.GetDisconnectRules());
    }

    [Test]
    public async Task GetDisconnectControlOptionsEvaluationItems()
    {
        // Arrange
        disconnectRuleRepository.Setup(drr => drr.GetLatestSerieNames(It.IsAny<DateTime>())).Returns(new Dictionary<ISeriesName, Unit>
              {
                { new SeriesName("label1", ObisCode.ElectrActualPowerP14L1), Unit.Watt },
                { new SeriesName("label1", ObisCode.ElectrActualPowerP23L1), Unit.Watt },
                { new SeriesName("label2", ObisCode.ElectrActualPowerP23L2), Unit.Percentage },
                { new SeriesName("label3", ObisCode.ElectrActualPowerP23L3), Unit.DegreeCelsius },
                { new SeriesName("label3", ObisCode.ElectrActualPowerP14L1), Unit.Watt }
              });
        disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(new IDisconnectRule[0]);

        // Act
        var response = await httpClient.GetAsync($"api/settings/disconnectrules/options");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestDisconnectControlOptionDto>();
        Assert.That(json.evaluationItems.Length, Is.EqualTo(3));
        var eval1 = json.evaluationItems.FirstOrDefault(x => x.label == "label1");
        Assert.That(eval1, Is.Not.Null);
        Assert.That(eval1.obiscode, Is.EqualTo(ObisCode.ElectrActualPowerP23L1.ToString()));
        Assert.That(eval1.unit, Is.EqualTo("W"));
        var eval2 = json.evaluationItems.FirstOrDefault(x => x.label == "label2");
        Assert.That(eval2, Is.Not.Null);
        Assert.That(eval2.obiscode, Is.EqualTo(ObisCode.ElectrActualPowerP23L2.ToString()));
        Assert.That(eval2.unit, Is.EqualTo("%"));
        var eval3 = json.evaluationItems.FirstOrDefault(x => x.label == "label3");
        Assert.That(eval3, Is.Not.Null);
        Assert.That(eval3.obiscode, Is.EqualTo(ObisCode.ElectrActualPowerP23L3.ToString()));
        Assert.That(eval3.unit, Is.EqualTo("C"));
    }

    internal class TestDisconnectControlOptionDto
    {
        public TestDisconnectControlOptionNamesDto[] disconnectControlItems { get; set; }
        public TestDisconnectControlOptionEvaluationDto[] evaluationItems { get; set; }
    }


    internal class TestDisconnectControlOptionNamesDto
    {
        public string label { get; set; }
        public string obiscode { get; set; }
    }

    internal class TestDisconnectControlOptionEvaluationDto
    {
        public string label { get; set; }
        public string obiscode { get; set; }
        public string unit { get; set; }
    }


    [Test]
    public async Task GetDisconnectControlOptionsDisconnectControlItems()
    {
        // Arrange
        disconnectRuleRepository.Setup(drr => drr.GetLatestSerieNames(It.IsAny<DateTime>())).Returns(new Dictionary<ISeriesName, Unit>
                  {
                    { new SeriesName("label1", ObisCode.ElectrActualPowerP14L1), Unit.Watt },
                    { new SeriesName("label2", "0.1.96.3.10.255"), Unit.NoUnit },
                    { new SeriesName("label3", "0.2.96.3.10.255"), Unit.NoUnit },
                    { new SeriesName("label4", ObisCode.ElectrActualPowerP14L1), Unit.Watt }
                  });
        disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(new IDisconnectRule[0]);

        // Act
        var response = await httpClient.GetAsync($"api/settings/disconnectrules/options");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestDisconnectControlOptionDto>();
        Assert.That(json.disconnectControlItems.Length, Is.EqualTo(2));
        var dis1 = json.disconnectControlItems.FirstOrDefault(x => x.label == "label2");
        Assert.That(dis1, Is.Not.Null);
        Assert.That(dis1.obiscode, Is.EqualTo("0.1.96.3.10.255"));
        var dis2 = json.disconnectControlItems.FirstOrDefault(x => x.label == "label3");
        Assert.That(dis2, Is.Not.Null);
        Assert.That(dis2.obiscode, Is.EqualTo("0.2.96.3.10.255"));
    }


    [Test]
    public async Task GetDisconnectControlOptionsDisconnectControlItemsDisconnectRulePresent()
    {
        // Arrange
        disconnectRuleRepository.Setup(drr => drr.GetLatestSerieNames(It.IsAny<DateTime>())).Returns(new Dictionary<ISeriesName, Unit>
                      {
                        { new SeriesName("label1", ObisCode.ElectrActualPowerP14L1), Unit.Watt },
                        { new SeriesName("label2", "0.1.96.3.10.255"), Unit.NoUnit },
                        { new SeriesName("label3", "0.2.96.3.10.255"), Unit.NoUnit },
                        { new SeriesName("label4", ObisCode.ElectrActualPowerP14L1), Unit.Watt }
                      });
        disconnectRuleRepository.Setup(drr => drr.GetDisconnectRules()).Returns(new[] {
                        new DisconnectRule(new SeriesName("label2", "0.1.96.3.10.255"), new SeriesName("l", "1.2.3.4.5.6"), new TimeSpan(0, 30, 0), 2, 1, Unit.Watt) });

        // Act
        var response = await httpClient.GetAsync($"api/settings/disconnectrules/options");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestDisconnectControlOptionDto>();
        Assert.That(json.disconnectControlItems.Length, Is.EqualTo(1));
        var dis1 = json.disconnectControlItems.FirstOrDefault(x => x.label == "label3");
        Assert.That(dis1, Is.Not.Null);
        Assert.That(dis1.obiscode, Is.EqualTo("0.2.96.3.10.255"));
    }

    [Test]
    public async Task AddDisconnectRule()
    {
        // Arrange
        var disconnectRuleDto = new DisconnectRuleDto
        {
            NameLabel = "NameLabel",
            NameObisCode = "1.2.3.4.5.6",
            EvaluationLabel = "EvalLabel",
            EvaluationObisCode = "6.5.4.3.2.1",
            DurationMinutes = 30,
            DisconnectToConnectValue = 1550,
            ConnectToDisconnectValue = 350,
            Unit = DisconnectRuleUnit.W
        };
        var content = JsonContent.Create(disconnectRuleDto);

        // Act
        var response = await httpClient.PostAsync($"api/settings/disconnectrules", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var disconnectRule = new DisconnectRule(new SeriesName("NameLabel", "1.2.3.4.5.6"), new SeriesName("EvalLabel", "6.5.4.3.2.1"), TimeSpan.FromMinutes(30),
                                                1550, 350, Unit.Watt);
        disconnectRuleRepository.Verify(err => err.AddDisconnectRule(disconnectRule));
    }

    [Test]
    public async Task AddDisconnectRuleNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync($"api/settings/disconnectrules", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AddDisconnectRuleNameAlreadyExists()
    {
        // Arrange
        disconnectRuleRepository.Setup(err => err.AddDisconnectRule(It.IsAny<DisconnectRule>())).Throws(new DataStoreUniqueConstraintException());
        var disconnectRuleDto = new DisconnectRuleDto
        {
            NameLabel = "NameLabel",
            NameObisCode = "1.2.3.4.5.6",
            EvaluationLabel = "EvalLabel",
            EvaluationObisCode = "6.5.4.3.2.1",
            DurationMinutes = 30,
            DisconnectToConnectValue = 1550,
            ConnectToDisconnectValue = 350,
            Unit = DisconnectRuleUnit.W
        };
        var content = JsonContent.Create(disconnectRuleDto);

        // Act
        var response = await httpClient.PostAsync($"api/settings/disconnectrules", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task DeleteDisconnectRuleAbsentPath()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/disconnectrules/names");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        disconnectRuleRepository.Verify(drr => drr.DeleteDisconnectRule(It.IsAny<ISeriesName>()), Times.Never);
    }

    [Test]
    public async Task DeleteDisconnectRuleBadObisCode()
    {
        // Arrange
        const string label = "label";
        const string obisCode = "bad obis code";

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/disconnectrules/names/{label}/{obisCode}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        disconnectRuleRepository.Verify(drr => drr.DeleteDisconnectRule(It.IsAny<ISeriesName>()), Times.Never);
    }

    [Test]
    public async Task DeleteDisconnectRule()
    {
        // Arrange
        const string label = "label";
        const string obisCode = "0.1.96.3.10.255";

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/disconnectrules/names/{label}/{obisCode}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        disconnectRuleRepository.Verify(drr => drr.DeleteDisconnectRule(new SeriesName(label, obisCode)));
    }
}
