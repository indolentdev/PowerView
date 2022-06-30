using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Controllers;
using PowerView.Service.DisconnectControl;
using PowerView.Service.Dtos;

namespace PowerView.Service.IntegrationTest;

public class DeviceOnDemandControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;

    private Mock<IDisconnectControlCache> disconnectControlCache;

    [SetUp]
    public void Setup()
    {
        disconnectControlCache = new Mock<IDisconnectControlCache>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(disconnectControlCache.Object);
                });
            });

        httpClient = application.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        application?.Dispose();
    }

    [Test]
    public async Task GetOnDemand_CallsDisconnectControlCache()
    {
        // Arrange
        SetupDisconnectControlCacheGetOutputStatus();

        // Act
        var response = await httpClient.GetAsync("api/devices/ondemand?label=theLabel");

        // Assert
        disconnectControlCache.Verify(x => x.GetOutputStatus("theLabel"));
    }

    [Test]
    public async Task GetOnDemand()
    {
        // Arrange
        var seriesName1 = new SeriesName("lbl", "0.1.96.3.10.255");
        var connected1 = true;
        var seriesName2 = new SeriesName("lbl", "0.2.96.3.10.255");
        var connected2 = false;
        SetupDisconnectControlCacheGetOutputStatus((seriesName1, connected1), (seriesName2, connected2));

        // Act
        var response = await httpClient.GetAsync("api/devices/ondemand?label=theLabel");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        dynamic json = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.That(json, Is.Not.Null);
        Assert.That(json.TryGetProperty("items", out JsonElement items), Is.True);
        Assert.That(items.GetArrayLength(), Is.EqualTo(2));
        AssertDisconnectControl(seriesName1, connected1, items.EnumerateArray().First());
        AssertDisconnectControl(seriesName2, connected2, items.EnumerateArray().Last());
    }

    private static void AssertDisconnectControl(ISeriesName seriesName, bool connected, JsonElement item)
    {
        Assert.That(item.TryGetProperty("label", out var label), Is.True);
        Assert.That(label.GetString(), Is.EqualTo(seriesName.Label));
        Assert.That(item.TryGetProperty("obisCode", out var obisCode), Is.True);
        Assert.That(obisCode.GetString(), Is.EqualTo(seriesName.ObisCode.ToString()));
        Assert.That(item.TryGetProperty("kind", out var kind), Is.True);
        Assert.That(kind.GetString(), Is.EqualTo("Method"));
        Assert.That(item.TryGetProperty("index", out var index), Is.True);
        Assert.That(index.GetInt32(), Is.EqualTo(connected ? 2 : 1));
    }

    [Test]
    public async Task GetOnDemand_QueryParameterLabelAbsent()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync("api/devices/ondemand");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetOnDemand_QueryParameterLabelEmpty()
    {
        // Arrange

        // Act
        var response = await httpClient.GetAsync("api/devices/ondemand?label=");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private void SetupDisconnectControlCacheGetOutputStatus(params (ISeriesName SeriesName, bool Output)[] values)
    {
        disconnectControlCache.Setup(x => x.GetOutputStatus(It.IsAny<string>()))
            .Returns(values.ToDictionary(x => x.SeriesName, x => x.Output));
    }

}
