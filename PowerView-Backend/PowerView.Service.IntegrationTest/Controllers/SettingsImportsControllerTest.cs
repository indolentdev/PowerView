using System;
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
public class SettingsImportsControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<IImportRepository> importRepository;

    [SetUp]
    public void SetUp()
    {
        importRepository = new Mock<IImportRepository>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(importRepository.Object);
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
    public async Task GetImports()
    {
        // Arrange
        DateTime dateTime = new DateTime(2023, 9, 27, 19, 0, 3, DateTimeKind.Utc);
        var import1 = new Import("lbl1", "DK1", Unit.Eur, dateTime, null, false);
        var import2 = new Import("lbl2", "DK2", Unit.Dkk, dateTime.AddHours(1), dateTime.AddDays(1), false);
        importRepository.Setup(cbr => cbr.GetImports()).Returns(new[] { import1, import2 });

        // Act
        var response = await httpClient.GetAsync($"api/settings/imports");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestImportSetDto>();
        Assert.That(json.imports.Length, Is.EqualTo(2));
        AssertImport(import1, json.imports[0]);
        AssertImport(import2, json.imports[1]);
        importRepository.Verify(cbr => cbr.GetImports());
    }

    [Test]
    public async Task Import()
    {
        // Arrange
        var import = new
        {
            Label = "The Label",
            Channel = "DK1",
            Currency = "DKK",
            FromTimestamp = DateTimeMapper.Map(DateTime.UtcNow)
        };
        var content = JsonContent.Create(import);

        // Act
        var response = await httpClient.PostAsync($"api/settings/imports", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        importRepository.Verify(ir => ir.AddImport(It.Is<Import>(i =>
          i.Label == import.Label && i.Channel == import.Channel && i.Currency.ToString().ToUpperInvariant() == import.Currency &&
          DateTimeMapper.Map(i.FromTimestamp) == import.FromTimestamp && i.CurrentTimestamp == null && i.Enabled)));
    }

    [Test]
    public async Task PostImportNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync($"api/settings/imports", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostImportBad()
    {
        // Arrange
        var import = new { };
        var content = JsonContent.Create(import);

        // Act
        var response = await httpClient.PostAsync($"api/settings/imports", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostImportAlreadyExists()
    {
        // Arrange
        importRepository.Setup(ir => ir.AddImport(It.IsAny<Import>())).Throws(new DataStoreUniqueConstraintException());
        var import = new
        {
            Label = "The Label",
            Channel = "DK1",
            Currency = "DKK",
            FromTimestamp = DateTimeMapper.Map(DateTime.UtcNow)
        };
        var content = JsonContent.Create(import);

        // Act
        var response = await httpClient.PostAsync($"api/settings/imports", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task DeleteImport()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/imports/TheLabel");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        importRepository.Verify(ir => ir.DeleteImport("TheLabel"));
    }

    [Test]
    public async Task DeleteImportLabelParameterBad()
    {
        // Arrange

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/imports/12345678901234567890123456");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        importRepository.Verify(ir => ir.DeleteImport(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Enable()
    {
        // Arrange
        var import = new
        {
            Enabled = true
        };
        var content = JsonContent.Create(import);

        // Act
        var response = await httpClient.PatchAsync($"api/settings/imports/theLabel", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        importRepository.Verify(ir => ir.SetEnabled(It.Is<string>(p => p == "theLabel"), It.Is<bool>(p => p == import.Enabled)));
    }

    [Test]
    public async Task EnableNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PatchAsync($"api/settings/imports/theLabel", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        importRepository.Verify(ir => ir.SetEnabled(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task EnableParameterBad()
    {
        // Arrange
        var import = new
        {
            Enabled = true
        };
        var content = JsonContent.Create(import);

        // Act
        var response = await httpClient.PatchAsync($"api/settings/imports/12345678901234567890123456", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        importRepository.Verify(ir => ir.SetEnabled(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task EnableBadContent()
    {
        // Arrange
        var import = new { };
        var content = JsonContent.Create(import);

        // Act
        var response = await httpClient.PatchAsync($"api/settings/imports/theLabel", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        importRepository.Verify(ir => ir.SetEnabled(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    private void AssertImport(Import import, TestImportDto dto)
    {
        Assert.That(dto.label, Is.EqualTo(import.Label));
        Assert.That(dto.channel, Is.EqualTo(import.Channel));
        Assert.That(dto.currency, Is.EqualTo(import.Currency.ToString().ToUpperInvariant()));
        Assert.That(dto.fromTimestamp, Is.EqualTo(DateTimeMapper.Map(import.FromTimestamp)));
        Assert.That(dto.currentTimestamp, Is.EqualTo(DateTimeMapper.Map(import.CurrentTimestamp != null ? import.CurrentTimestamp.Value.AddHours(-1) : null)));
        Assert.That(dto.enabled, Is.EqualTo(import.Enabled));
    }

    internal class TestImportSetDto
    {
        public TestImportDto[] imports { get; set; }
    }

    public class TestImportDto
    {
        public string label { get; set; }

        public string channel { get; set; }

        public string currency { get; set; }

        public string fromTimestamp { get; set; }

        public string currentTimestamp { get; set; }

        public bool enabled { get; set; }
    }

}
