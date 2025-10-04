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
public class SettingsGeneratorsSeriesControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<IGeneratorSeriesRepository> generatorSeriesRepository;

    [SetUp]
    public void SetUp()
    {
        generatorSeriesRepository = new Mock<IGeneratorSeriesRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(generatorSeriesRepository.Object);
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
    public async Task GetGeneratorsSeries()
    {
        // Arrange
        var generatorSeries = new GeneratorSeries(new SeriesName("l", "1.2.3.4.5.6"), new SeriesName("b", "6.5.4.3.2.1"), "CBTitle");
        var generatorsSeries = new[] { generatorSeries };
        generatorSeriesRepository.Setup(x => x.GetGeneratorSeries()).Returns(generatorsSeries);

        // Act
        var response = await httpClient.GetAsync($"api/settings/generators/series");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestGeneratorsSeriesSetDto>();
        Assert.That(json.items, Is.Not.Null);
        Assert.That(json.items.Length, Is.EqualTo(1));
        Assert.That(json.items.First().nameLabel, Is.EqualTo("l"));
        Assert.That(json.items.First().nameObisCode, Is.EqualTo("1.2.3.4.5.6"));
        Assert.That(json.items.First().baseLabel, Is.EqualTo("b"));
        Assert.That(json.items.First().baseObisCode, Is.EqualTo("6.5.4.3.2.1"));
        Assert.That(json.items.First().costBreakdownTitle, Is.EqualTo("CBTitle"));
    }

    internal class TestGeneratorsSeriesSetDto
    {
        public TestGeneratorsSeriesDto[] items { get; set; }
    }

    internal class TestGeneratorsSeriesDto
    {
        public string nameLabel { get; set; }
        public string nameObisCode { get; set; }
        public string baseLabel { get; set; }
        public string baseObisCode { get; set; }
        public string costBreakdownTitle { get; set; }
    }

    [Test]
    public async Task GetGeneratorsSeriesCallsRepository()
    {
        // Arrange
        generatorSeriesRepository.Setup(x => x.GetGeneratorSeries()).Returns(Array.Empty<GeneratorSeries>());

        // Act
        var response = await httpClient.GetAsync($"api/settings/generators/series");

        // Assert
        generatorSeriesRepository.Verify(x => x.GetGeneratorSeries());
    }

    [Test]
    public async Task AddGeneratorsSeries()
    {
        // Arrange
        var dto = new GeneratorSeriesDto
        {
            NameLabel = "NameLabel",
            NameObisCode = "1.2.3.4.5.6",
            BaseLabel = "BaseLabel",
            BaseObisCode = "6.5.4.3.2.1",
            CostBreakdownTitle = "CBTitle"
        };
        var content = JsonContent.Create(dto);

        // Act
        var response = await httpClient.PostAsync($"api/settings/generators/series", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var generatorSeries = new GeneratorSeries(new SeriesName("NameLabel", "1.2.3.4.5.6"), new SeriesName("BaseLabel", "6.5.4.3.2.1"), "CBTitle");
        generatorSeriesRepository.Verify(err => err.AddGeneratorSeries(generatorSeries));
    }

    [Test]
    public async Task AddGeneratorsSeriesNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync($"api/settings/generators/series", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AddGeneratorsSeriesAlreadyExists()
    {
        // Arrange
        generatorSeriesRepository.Setup(err => err.AddGeneratorSeries(It.IsAny<GeneratorSeries>())).Throws(new DataStoreUniqueConstraintException());
        var dto = new GeneratorSeriesDto
        {
            NameLabel = "NameLabel",
            NameObisCode = "1.2.3.4.5.6",
            BaseLabel = "BaseLabel",
            BaseObisCode = "6.5.4.3.2.1",
            CostBreakdownTitle = "CBTitle"
        };
        var content = JsonContent.Create(dto);

        // Act
        var response = await httpClient.PostAsync($"api/settings/generators/series", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task DeleteGeneratorsSeriesAbsentPath()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/generators/series");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.MethodNotAllowed));
        generatorSeriesRepository.Verify(drr => drr.DeleteGeneratorSeries(It.IsAny<ISeriesName>()), Times.Never);
    }

    [Test]
    public async Task DeleteGeneratorsSeriesBadObisCode()
    {
        // Arrange
        const string label = "label";
        const string obisCode = "bad obis code";

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/generators/series/{label}/{obisCode}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        generatorSeriesRepository.Verify(drr => drr.DeleteGeneratorSeries(It.IsAny<ISeriesName>()), Times.Never);
    }

    [Test]
    public async Task DeleteGeneratorsSeries()
    {
        // Arrange
        const string label = "label";
        const string obisCode = "1.69.25.67.0.255";

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/generators/series/{label}/{obisCode}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        generatorSeriesRepository.Verify(drr => drr.DeleteGeneratorSeries(new SeriesName(label, obisCode)));
    }

    [Test]
    [TestCase("1.68.25.67.0.255", "1.69.25.67.0.255")]
    [TestCase("1.68.25.68.0.255", "1.69.25.68.0.255")]
    public async Task GetBaseSeries(string obisCode, string genObisCode)
    {
        // Arrange
        var dateTime = DateTime.UtcNow;
        var baseSeries = (new SeriesName("b", obisCode), dateTime);
        generatorSeriesRepository.Setup(x => x.GetBaseSeries()).Returns(new[] { baseSeries });

        // Act
        var response = await httpClient.GetAsync($"api/settings/generators/bases/series");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestBasesSeriesSetDto>();
        Assert.That(json.items, Is.Not.Null);
        Assert.That(json.items.Length, Is.EqualTo(1));
        Assert.That(json.items.First().obisCode, Is.EqualTo(genObisCode));
        Assert.That(json.items.First().baseLabel, Is.EqualTo("b"));
        Assert.That(json.items.First().baseObisCode, Is.EqualTo(obisCode));
        Assert.That(json.items.First().latestTimestamp, Is.EqualTo(dateTime.ToString("o")));
    }

    internal class TestBasesSeriesSetDto
    {
        public TestBasesSeriesDto[] items { get; set; }
    }

    internal class TestBasesSeriesDto
    {
        public string obisCode { get; set; }
        public string baseLabel { get; set; }
        public string baseObisCode { get; set; }
        public string latestTimestamp { get; set; }
    }

    [Test]
    public async Task GetBaseSeriesCallsRepository()
    {
        // Arrange
        generatorSeriesRepository.Setup(x => x.GetBaseSeries()).Returns(Array.Empty<(SeriesName, DateTime)>());

        // Act
        var response = await httpClient.GetAsync($"api/settings/generators/bases/series");

        // Assert
        generatorSeriesRepository.Verify(x => x.GetBaseSeries());
    }

}
