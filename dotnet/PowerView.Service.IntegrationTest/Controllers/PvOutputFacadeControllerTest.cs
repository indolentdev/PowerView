
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    private Mock<IHttpWebRequestFactory> httpWebRequestFactory;

    private const string AddStatusRoute = "/service/r2/addstatus.jsp";
    private Uri pvOutputAddStatus = new Uri("http://TheRealPvOutput/service/r2/addstatus.jsp");
    private const string deviceLabel = "TheDeviceLabel";
    private const string serialNumber = "1234";
    private const string serialNumberParam = "v12";
    private const string powerL1Param = "v7";
    private const string powerL2Param = "v8";
    private const string powerL3Param = "v9";
    private HttpWebRequestMock forwardRequest;
    private HttpWebResponseMock forwardResponse;

    [SetUp]
    public void SetUp()
    {
        readingAccepter = new Mock<IReadingAccepter>();
        liveReadingMapper = new Mock<ILiveReadingMapper>();
        options = new PvOutputOptions();
        httpWebRequestFactory = new Mock<IHttpWebRequestFactory>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(readingAccepter.Object);
                    sc.AddSingleton(liveReadingMapper.Object);
                    sc.AddSingleton((IOptions<PvOutputOptions>)options);
                    sc.AddSingleton(httpWebRequestFactory.Object);
                });
            });

        httpClient = application.CreateClient();

        forwardRequest = new HttpWebRequestMock();
        forwardResponse = new HttpWebResponseMock();

        options.PvOutputAddStatusUrl = pvOutputAddStatus;
        options.PvDeviceLabel = deviceLabel;
        options.PvDeviceId = serialNumber;
        options.PvDeviceIdParam = serialNumberParam;
        options.ActualPowerP23L1Param = powerL1Param;
        options.ActualPowerP23L2Param = powerL2Param;
        options.ActualPowerP23L3Param = powerL3Param;

        httpWebRequestFactory.Setup(f => f.Create(It.IsAny<Uri>())).Returns(forwardRequest);
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
        SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);
        var liveReading = new LiveReading(deviceLabel, serialNumber, DateTime.UtcNow, new[] { new RegisterValue("1.2.3.4.5.6", 1, 0, Unit.Watt) });

        liveReadingMapper.Setup(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(liveReading);
        const string requestBody = "TheRequestBody";
        var content = new StringContent(requestBody, Encoding.UTF8, "custom/content");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp?d=dV&t=tV", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        Assert.That(forwardRequest.Method, Is.EqualTo("POST"));

        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
          It.Is<Uri>(p => p.Query == "?d=dV&t=tV"),
          It.Is<string>(p => p == "custom/content; charset=utf-8"), It.IsNotNull<Stream>(),
          It.Is<string>(p => p == deviceLabel), It.Is<string>(p => p == serialNumber), It.Is<string>(p => p == serialNumberParam),
          It.Is<string>(p => p == powerL1Param), It.Is<string>(p => p == powerL2Param), It.Is<string>(p => p == powerL3Param)));

        readingAccepter.Verify(ra => ra.Accept(It.Is<LiveReading[]>(lr => lr.Length == 1 && lr.First() == liveReading)));
    }

    [Test]
    public async Task AddStatusPostMapsStream()
    {
        // Arrange
        SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);
        var liveReading = new LiveReading(deviceLabel, serialNumber, DateTime.UtcNow, new[] { new RegisterValue("1.2.3.4.5.6", 1, 0, Unit.Watt) });

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
        SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);
        var liveReading = new LiveReading(deviceLabel, serialNumber, DateTime.UtcNow, new[] { new RegisterValue("1.2.3.4.5.6", 1, 0, Unit.Watt) });
        liveReadingMapper.Setup(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(liveReading);

        // Act
        var response = await httpClient.GetAsync($"service/r2/addstatus.jsp?d=dV&t=tV");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(forwardRequest.Method, Is.EqualTo("GET"));
        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(It.Is<Uri>(p => p.Query == "?d=dV&t=tV"), It.IsAny<string>(), It.IsAny<Stream>(),
          It.Is<string>(p => p == deviceLabel), It.Is<string>(p => p == serialNumber), It.Is<string>(p => p == serialNumberParam),
          It.Is<string>(p => p == powerL1Param), It.Is<string>(p => p == powerL2Param), It.Is<string>(p => p == powerL3Param)));
        readingAccepter.Verify(ra => ra.Accept(It.Is<LiveReading[]>(lr => lr.Length == 1 && lr.First() == liveReading)));
    }


    [Test]
    public async Task AddStatusPostPvDeviceLabelAbsent()
    {
        // Arrange
        options.PvDeviceLabel = null;
        SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);
        const string requestBody = "TheRequestBody";
        var content = new StringContent(requestBody, Encoding.UTF8, "custom/content");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp?d=dV&t=tV", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(forwardRequest.Method, Is.EqualTo("POST"));
        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<LiveReading[]>()), Times.Never);
    }

    [Test]
    public async Task AddStatusGetPvDeviceLabelAbsent()
    {
        // Arrange
        options.PvDeviceLabel = null;
        SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);

        // Act
        var response = await httpClient.GetAsync($"service/r2/addstatus.jsp?d=dV&t=tV");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(forwardRequest.Method, Is.EqualTo("GET"));
        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<LiveReading[]>()), Times.Never);
    }

    [Test]
    public async Task AddStatusPostCopiesContentTypeAndBodyContent()
    {
        // Arrange
        const string responseContentType = "custom/content-response";
        var responseBody = Encoding.UTF8.GetBytes("TheResponseBody");
        SetupResponse(HttpStatusCode.OK, responseContentType, responseBody);
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(forwardRequest.ContentType, Is.EqualTo(content.Headers.ContentType.ToString()));
        Assert.That(forwardRequest.GetContentBytes(), Is.EqualTo(await content.ReadAsByteArrayAsync()));
        Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo(responseContentType));
        Assert.That(await response.Content.ReadAsByteArrayAsync(), Is.EqualTo(responseBody));
    }

    [Test]
    public async Task AddStatusPostCopiesPvOutputHeaders()
    {
        // Arrange
        SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);
        forwardResponse.Headers.Add("X-Rate-Limit-Remaining", "remaining");
        forwardResponse.Headers.Add("x-rate-limit-limit", "limit");
        forwardResponse.Headers.Add("X-RATE-LIMIT-RESET", "reset");
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");
        httpClient.DefaultRequestHeaders.Add("X-PVOUTPUT-APIKEY", "whatnot");
        httpClient.DefaultRequestHeaders.Add("x-pvoutput-systemid", "whatelsenot");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(forwardRequest.Headers["x-pvoutput-apikey"], Is.EqualTo("whatnot"));
        Assert.That(forwardRequest.Headers["X-PVOUTPUT-SYSTEMID"], Is.EqualTo("whatelsenot"));
        Assert.That(response.Headers.GetValues("X-Rate-Limit-Remaining"), Is.EqualTo(new[] { "remaining" }));
        Assert.That(response.Headers.GetValues("X-Rate-Limit-Limit"), Is.EqualTo(new[] { "limit" }));
        Assert.That(response.Headers.GetValues("X-Rate-Limit-Reset"), Is.EqualTo(new[] { "reset" }));
    }

    [Test]
    public async Task AddStatusPostPropagatesHttpWebExceptionRequest()
    {
        // Arrange
        var exception = new HttpWebException("Drugs are baaad - m'kay", WebExceptionStatus.ConnectFailure, null);
        forwardRequest.SetRequestStream(exception);
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That(response.Content.Headers.ContentType.ToString(), Is.EqualTo("text/plain; charset=utf-8"));
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Internal error interacting with PVOutput. Error message:" + exception.Message));

        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
          It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<LiveReading[]>()), Times.Never);
    }


    [Test]
    public async Task AddStatusPostPropagatesHttpWebExceptionResponse()
    {
        // Arrange
        forwardResponse.StatusCode = HttpStatusCode.BadRequest;
        forwardResponse.ContentType = "some/contentType";
        var responseBodyContent = Encoding.UTF8.GetBytes("TheResponseBodyContent");
        forwardResponse.SetContentBytes(responseBodyContent);

        var exception = new HttpWebException("Drugs are baaad - m'kay", WebExceptionStatus.ProtocolError, forwardResponse);
        forwardRequest.SetResponse(exception);
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType.ToString(), Is.EqualTo(forwardResponse.ContentType));
        Assert.That(await response.Content.ReadAsByteArrayAsync(), Is.EqualTo(responseBodyContent));

        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
          It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<LiveReading[]>()), Times.Never);
    }

    [Test]
    public async Task AddStatusPostPropagatesHttpWebExceptionResponseLessLogging()
    {
        // Arrange
        forwardResponse.StatusCode = HttpStatusCode.BadRequest;
        forwardResponse.ContentType = "some/contentType";
        var responseBodyContent = Encoding.UTF8.GetBytes("TheResponseBodyContent");
        forwardResponse.SetContentBytes(responseBodyContent);

        var exception = new HttpWebException("Drugs are baaad - m'kay", WebExceptionStatus.ConnectFailure, forwardResponse);
        forwardRequest.SetResponse(exception);
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act
        var response = await httpClient.PostAsync($"service/r2/addstatus.jsp", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType.ToString(), Is.EqualTo(forwardResponse.ContentType));
        Assert.That(await response.Content.ReadAsByteArrayAsync(), Is.EqualTo(responseBodyContent));

        liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
          It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        readingAccepter.Verify(ra => ra.Accept(It.IsAny<LiveReading[]>()), Times.Never);
    }

    [Test]
    public void AddStatusPostThrows()
    {
        // Arrange
        forwardRequest.SetResponse(new InvalidOperationException());
        var content = new StringContent("TheRequestBody", Encoding.UTF8, "custom/content-request");

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => httpClient.PostAsync($"service/r2/addstatus.jsp", content));
    }

    private void SetupResponse(HttpStatusCode statusCode, string contentType, byte[] responseBodyContent)
    {
        forwardRequest.SetResponse(forwardResponse);

        forwardResponse.StatusCode = statusCode;
        forwardResponse.ContentType = contentType;
        forwardResponse.SetContentBytes(responseBodyContent);
    }

}
