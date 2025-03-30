using System;
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

[TestFixture]
public class ObisControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<ILiveReadingRepository> liveReadingRepository;

    [SetUp]
    public void Setup()
    {
        liveReadingRepository = new Mock<ILiveReadingRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(liveReadingRepository.Object);
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
    public async Task GetObisCodes_CallsRepository()
    {
        // Arrange
        SetupLiveReadingRepositoryGetObisCodes();
        var label = "theLabel";

        // Act
        var response = await httpClient.GetAsync($"api/obis/codes?label={label}");

        // Assert
        liveReadingRepository.Verify(x => x.GetObisCodes(It.Is<string>(a => a == label), It.Is<DateTime>(a => (DateTime.UtcNow - a) - TimeSpan.FromDays(365) < TimeSpan.FromSeconds(2))));
    }

    [Test]
    public async Task GetObisCodes()
    {
        // Arrange
        SetupLiveReadingRepositoryGetObisCodes("1.2.3.4.5.6", "6.5.4.3.2.1");

        // Act
        var response = await httpClient.GetAsync($"api/obis/codes?label=theLabel");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<ObisCodesDto>();
        Assert.That(json.ObisCodes, Is.EqualTo(new[] { "1.2.3.4.5.6", "6.5.4.3.2.1" }));
    }

    private void SetupLiveReadingRepositoryGetObisCodes(params ObisCode[] obisCodes)
    {
        liveReadingRepository.Setup(x => x.GetObisCodes(It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(obisCodes);
    }

    public class ObisCodesDto
    {
        public string[] ObisCodes { get; set; }
    }

}
