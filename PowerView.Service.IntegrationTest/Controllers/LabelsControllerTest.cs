using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class LabelsControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<ILabelRepository> labelRepository;

    [SetUp]
    public void Setup()
    {
        labelRepository = new Mock<ILabelRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(labelRepository.Object);
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
    public async Task GetLabelNames_CallsLabelRepository()
    {
        // Arrange
        SetupLabelRepositoryGetLabelsByTimestamp();

        // Act
        var response = await httpClient.GetAsync($"api/labels/names");

        // Assert
        labelRepository.Verify(x => x.GetLabelsByTimestamp());
    }

    [Test]
    public async Task GetLabelnames()
    {
        // Arrange
        SetupLabelRepositoryGetLabelsByTimestamp("lbl1", "lbl2");

        // Act
        var response = await httpClient.GetAsync($"api/labels/names");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.That(json, Is.EqualTo(new [] { "lbl1", "lbl2" }));
    }

    private void SetupLabelRepositoryGetLabelsByTimestamp(params string[] labels)
    {
        labelRepository.Setup(x => x.GetLabelsByTimestamp())
            .Returns(labels);
    }

}
