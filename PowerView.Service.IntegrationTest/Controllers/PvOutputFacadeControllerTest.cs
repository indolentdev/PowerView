using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Service.Controllers;
using PowerView.Service.Mappers;

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class PvOutputFacadeControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<IReadingAccepter> readingAccepter;
    private Mock<ILiveReadingMapper> liveReadingMapper;
    private PvOutputOptions options;
    private Mock<IHttpClientFactory> httpClientFactory;

    private const string AddStatusRoute = "/service/r2/addstatus.jsp";
    private Uri pvOutputAddStatus = new Uri("http://TheRealPvOutput/service/r2/addstatus.jsp");
    private const string deviceLabel = "TheDeviceLabel";
    private const string serialNumber = "1234";
    private const string serialNumberParam = "v12";
    private const string powerL1Param = "v7";
    private const string powerL2Param = "v8";
    private const string powerL3Param = "v9";

    [SetUp]
    public void SetUp()
    {
        readingAccepter = new Mock<IReadingAccepter>();
        liveReadingMapper = new Mock<ILiveReadingMapper>();
        options = new PvOutputOptions();
        httpClientFactory = new Mock<IHttpClientFactory>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(readingAccepter.Object);
                    sc.AddSingleton(liveReadingMapper.Object);
                    sc.AddSingleton((IOptions<PvOutputOptions>)options);
                    sc.AddSingleton(httpClientFactory.Object);
                });
            });

        httpClient = application.CreateClient();

        options.PvOutputAddStatusUrl = pvOutputAddStatus;
        options.PvDeviceLabel = deviceLabel;
        options.PvDeviceId = serialNumber;
        options.PvDeviceIdParam = serialNumberParam;
        options.ActualPowerP23L1Param = powerL1Param;
        options.ActualPowerP23L2Param = powerL2Param;
        options.ActualPowerP23L3Param = powerL3Param;
    }

    [TearDown]
    public void Teardown()
    {
        application?.Dispose();
    }

    [Test]
    public async Task AddStatusPostMapsAndSavesToRepository()
    {
        // Arrange
        var httpMessageHandler = SetupHttpClientFactory(HttpStatusCode.OK);
        var liveReading = new Reading(deviceLabel, serialNumber, DateTime.UtcNow, new[] { new RegisterValue("1.2.3.4.5.6", 1, 0, Unit.Watt) });

        liveReadingMapper.Setup(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(liveReading);
        const string requestBody = "TheRequestBody";
        var content = new StringContent(requestBody, Encoding.UTF8, "custom/content");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp?d=dV&t=tV", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        httpMessageHandler.Verify(x => x(It.Is<HttpRequestMessage>(p => p.Method == HttpMethod.Post), It.IsAny<CancellationToken>()));

        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
          It.Is<Uri>(p => p.Query == "?d=dV&t=tV"),
          It.Is<string>(p => p == "custom/content; charset=utf-8"), It.IsNotNull<Stream>(),
          It.Is<string>(p => p == deviceLabel), It.Is<string>(p => p == serialNumber), It.Is<string>(p => p == serialNumberParam),
          It.Is<string>(p => p == powerL1Param), It.Is<string>(p => p == powerL2Param), It.Is<string>(p => p == powerL3Param)));

        readingAccepter.Verify(ra => ra.Accept(It.Is<Reading[]>(lr => lr.Length == 1 && lr.First() == liveReading)));
    }

    [Test]
    public async Task AddStatusPostMapsStream()
    {
        // Arrange
        SetupHttpClientFactory(HttpStatusCode.OK);
        var liveReading = new Reading(deviceLabel, serialNumber, DateTime.UtcNow, new[] { new RegisterValue("1.2.3.4.5.6", 1, 0, Unit.Watt) });

        var mapBodyStream = new MemoryStream();
        liveReadingMapper.Setup(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(liveReading)
            .Callback<Uri, string, Stream, string, string, string, string, string, string>((u, c, b, dl, di, dip, l1p, l2p, l3p) =>
            {
                b.CopyTo(mapBodyStream);
                mapBodyStream.Position = 0;
            });
        const string requestBody = "TheRequestBody";
        var content = new StringContent(requestBody, Encoding.UTF8, "custom/content");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp?d=dV&t=tV", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(Encoding.UTF8.GetString(mapBodyStream.ToArray()), Is.EqualTo(requestBody));
    }


    [Test]
    public async Task AddStatusGetMapsAndSavesToRepository()
    {
        // Arrange
        var httpMessageHandler = SetupHttpClientFactory(HttpStatusCode.OK);
        var liveReading = new Reading(deviceLabel, serialNumber, DateTime.UtcNow, new[] { new RegisterValue("1.2.3.4.5.6", 1, 0, Unit.Watt) });
        liveReadingMapper.Setup(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(liveReading);

        // Act
        var response = await httpClient.GetAsync($"service/r2/addstatus.jsp?d=dV&t=tV");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        httpMessageHandler.Verify(x => x(It.Is<HttpRequestMessage>(p => p.Method == HttpMethod.Get), It.IsAny<CancellationToken>()));
        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(It.Is<Uri>(p => p.Query == "?d=dV&t=tV"), It.IsAny<string>(), It.IsAny<Stream>(),
          It.Is<string>(p => p == deviceLabel), It.Is<string>(p => p == serialNumber), It.Is<string>(p => p == serialNumberParam),
          It.Is<string>(p => p == powerL1Param), It.Is<string>(p => p == powerL2Param), It.Is<string>(p => p == powerL3Param)));
        readingAccepter.Verify(ra => ra.Accept(It.Is<Reading[]>(lr => lr.Length == 1 && lr.First() == liveReading)));
    }


    [Test]
    public async Task AddStatusPostPvDeviceLabelAbsent()
    {
        // Arrange
        options.PvDeviceLabel = null;
        var httpMessageHandler = SetupHttpClientFactory(HttpStatusCode.OK);
        const string requestBody = "TheRequestBody";
        var content = new StringContent(requestBody, Encoding.UTF8, "custom/content");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp?d=dV&t=tV", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        httpMessageHandler.Verify(x => x(It.Is<HttpRequestMessage>(p => p.Method == HttpMethod.Post), It.IsAny<CancellationToken>()));
        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<Reading[]>()), Times.Never);
    }

    [Test]
    public async Task AddStatusGetPvDeviceLabelAbsent()
    {
        // Arrange
        options.PvDeviceLabel = null;
        var httpMessageHandler = SetupHttpClientFactory(HttpStatusCode.OK);

        // Act
        var response = await httpClient.GetAsync($"service/r2/addstatus.jsp?d=dV&t=tV");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        httpMessageHandler.Verify(x => x(It.Is<HttpRequestMessage>(p => p.Method == HttpMethod.Get), It.IsAny<CancellationToken>()));
        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<Reading[]>()), Times.Never);
    }

    [Test]
    public async Task AddStatusPostCopiesContentTypeAndBodyContent()
    {
        // Arrange
        const string responseContentType = "custom/content-response";
        var responseBody = Encoding.UTF8.GetBytes("TheResponseBody");
        var httpMessageHandler = SetupHttpClientFactory(HttpStatusCode.OK, responseContentType, responseBody);
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        httpMessageHandler.Verify(x => x(It.Is<HttpRequestMessage>(p => p.Content.Headers.ContentType.Equals(content.Headers.ContentType) &&
                (p.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()).SequenceEqual(content.ReadAsByteArrayAsync().GetAwaiter().GetResult() )), 
            It.IsAny<CancellationToken>()));
        Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo(responseContentType));
        Assert.That(await response.Content.ReadAsByteArrayAsync(), Is.EqualTo(responseBody));
    }

    [Test]
    public async Task AddStatusPostCopiesPvOutputHeaders()
    {
        // Arrange
        var forwardResponse = new HttpResponseMessage(HttpStatusCode.OK);
        forwardResponse.Headers.Add("X-Rate-Limit-Remaining", "remaining");
        forwardResponse.Headers.Add("x-rate-limit-limit", "limit");
        forwardResponse.Headers.Add("X-RATE-LIMIT-RESET", "reset");
        var httpMessageHandler = SetupHttpClientFactory(forwardResponse);

        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");
        httpClient.DefaultRequestHeaders.Add("X-PVOUTPUT-APIKEY", "whatnot");
        httpClient.DefaultRequestHeaders.Add("x-pvoutput-systemid", "whatelsenot");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        httpMessageHandler.Verify(x => x(
            It.Is<HttpRequestMessage>(p => 
                new StringValues(p.Headers.GetValues("x-pvoutput-apikey").ToArray()) == "whatnot" &&
                new StringValues(p.Headers.GetValues("X-PVOUTPUT-SYSTEMID").ToArray()) == "whatelsenot"),
            It.IsAny<CancellationToken>()));
        Assert.That(response.Headers.GetValues("X-Rate-Limit-Remaining"), Is.EqualTo(new[] { "remaining" }));
        Assert.That(response.Headers.GetValues("X-Rate-Limit-Limit"), Is.EqualTo(new[] { "limit" }));
        Assert.That(response.Headers.GetValues("X-Rate-Limit-Reset"), Is.EqualTo(new[] { "reset" }));
    }

    [Test]
    public async Task AddStatusPostPropagatesTaskCanceledException()
    {
        // Arrange
        var httpMessageHandler = SetupHttpClientFactory();
        httpMessageHandler.Setup(x => x(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Throws(new TaskCanceledException("Drugs are baaad - m'kay"));
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.GatewayTimeout));
        Assert.That((await response.Content.ReadAsByteArrayAsync()), Is.Empty);

        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
          It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<Reading[]>()), Times.Never);
    }

    [Test]
    public async Task AddStatusPostPropagatesHttpRequestException()
    {
        // Arrange
        var httpMessageHandler = SetupHttpClientFactory();
        httpMessageHandler.Setup(x => x(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Throws(new HttpRequestException("Drugs are baaad - m'kay"));
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That((await response.Content.ReadAsByteArrayAsync()), Is.Empty);

        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
          It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<Reading[]>()), Times.Never);
    }

    [Test]
    public async Task AddStatusPostPropagatesErrorStatusCodeResponse()
    {
        // Arrange
        const string responseContentType = "some/contentType";
        var responseBodyContent = Encoding.UTF8.GetBytes("TheResponseBodyContent");
        var httpMessageHandler = SetupHttpClientFactory(HttpStatusCode.BadRequest, responseContentType, responseBodyContent);
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType.ToString(), Is.EqualTo(responseContentType));
        Assert.That(await response.Content.ReadAsByteArrayAsync(), Is.EqualTo(responseBodyContent));

        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
          It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<Reading[]>()), Times.Never);
    }

    [Test]
    public void AddStatusPostThrows()
    {
        // Arrange
        var httpMessageHandler = SetupHttpClientFactory();
        httpMessageHandler.Setup(x => x(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException());
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => httpClient.PostAsync($"service/r2/addstatus.jsp", content));
    }

    private Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>> SetupHttpClientFactory(HttpStatusCode statusCode, string contentType = null, byte[] responseBodyContent = null)
    {
        var response = new HttpResponseMessage(statusCode);

        if (! string.IsNullOrEmpty(contentType) && responseBodyContent != null)
        {
            var content = new ByteArrayContent(responseBodyContent);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            response.Content = content;
        }

        return SetupHttpClientFactory(response);
    }

    private Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>> SetupHttpClientFactory(HttpResponseMessage response)
    {
        var httpMessageHandler = SetupHttpClientFactory();
        httpMessageHandler.Setup(x => x(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(response);
        return httpMessageHandler;
    }

    private Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>> SetupHttpClientFactory()
    {
        var func = new Mock<Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>>();
        var stub = new HttpMessageHandlerStub(func.Object);
        var httpClient = new HttpClient(stub);
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        return func;
    }

}
