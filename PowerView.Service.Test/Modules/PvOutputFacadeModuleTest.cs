using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using PowerView.Model;
using PowerView.Service.Mappers;
using PowerView.Service.Modules;
using Moq;
using Nancy.Testing;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class PvOutputFacadeModuleTest
  {
    private Mock<IReadingAccepter> readingAccepter;
    private Mock<ILiveReadingMapper> liveReadingMapper;
    private Mock<IPvOutputFacadeModuleConfigProvider> moduleConfigProvider;
    private Mock<IHttpWebRequestFactory> httpWebRequestFactory;

    private Browser browser;

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
      moduleConfigProvider = new Mock<IPvOutputFacadeModuleConfigProvider>();
      httpWebRequestFactory = new Mock<IHttpWebRequestFactory>();
     
      browser = new Browser(cfg =>
      {
        cfg.Module<PvOutputFacadeModule>();
        cfg.Dependency<IReadingAccepter>(readingAccepter.Object);
        cfg.Dependency<ILiveReadingMapper>(liveReadingMapper.Object);
        cfg.Dependency<IPvOutputFacadeModuleConfigProvider>(moduleConfigProvider.Object);
        cfg.Dependency<IHttpWebRequestFactory>(httpWebRequestFactory.Object);
      });
        
      forwardRequest = new HttpWebRequestMock();
      forwardResponse = new HttpWebResponseMock();

      moduleConfigProvider.Setup(mcp => mcp.PvOutputAddStatus).Returns(pvOutputAddStatus);
      moduleConfigProvider.Setup(mcp => mcp.PvDeviceLabel).Returns(deviceLabel);
      moduleConfigProvider.Setup(mcp => mcp.PvDeviceId).Returns(serialNumber);
      moduleConfigProvider.Setup(mcp => mcp.PvDeviceIdParam).Returns(serialNumberParam);
      moduleConfigProvider.Setup(mcp => mcp.ActualPowerP23L1Param).Returns(powerL1Param);
      moduleConfigProvider.Setup(mcp => mcp.ActualPowerP23L2Param).Returns(powerL2Param);
      moduleConfigProvider.Setup(mcp => mcp.ActualPowerP23L3Param).Returns(powerL3Param);
      httpWebRequestFactory.Setup(f => f.Create(It.IsAny<Uri>())).Returns(forwardRequest);
    }

    [Test]
    public void AddStatusPostMapsAndSavesToRepository()
    {
      // Arrange
      const string requestContentType = "TheRequestContentType";
      var requestBody = Encoding.UTF8.GetBytes("TheRequestBody");
      SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);
      var liveReading = new LiveReading(deviceLabel, serialNumber, DateTime.UtcNow, new [] { new RegisterValue("1.2.3.4.5.6", 1, 0, Unit.Watt) });
      liveReadingMapper.Setup(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), 
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>())).Returns(liveReading);

      // Act
      var response = browser.Post(AddStatusRoute, with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Body(new MemoryStream(requestBody), requestContentType);
        with.Query("d", "dV");
        with.Query("t", "tV");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(Nancy.HttpStatusCode.OK));
      Assert.That(forwardRequest.Method, Is.EqualTo("POST"));
      liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
        It.Is<Uri>(p => p.Query == "?d=dV&t=tV"), 
        It.Is<string>(p => p == requestContentType), EqualTo(requestBody),
        It.Is<string>(p => p == deviceLabel), It.Is<string>(p=> p == serialNumber), It.Is<string>(p => p == serialNumberParam),
        It.Is<string>(p => p == powerL1Param), It.Is<string>(p => p == powerL2Param), It.Is<string>(p => p == powerL3Param)));
      readingAccepter.Verify(ra => ra.Accept(It.Is<LiveReading[]>(lr => lr.Length == 1 && lr.First() == liveReading)));
    }

    [Test]
    public void AddStatusGetMapsAndSavesToRepository()
    {
      // Arrange
      SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);
      var liveReading = new LiveReading(deviceLabel, serialNumber, DateTime.UtcNow, new [] { new RegisterValue("1.2.3.4.5.6", 1, 0, Unit.Watt) });
      liveReadingMapper.Setup(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(liveReading);

      // Act
      var response = browser.Get(AddStatusRoute, with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(Nancy.HttpStatusCode.OK));
      Assert.That(forwardRequest.Method, Is.EqualTo("GET"));
      liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.Is<string>(p => p == deviceLabel), It.Is<string>(p=> p == serialNumber), It.Is<string>(p => p == serialNumberParam),
        It.Is<string>(p => p == powerL1Param), It.Is<string>(p => p == powerL2Param), It.Is<string>(p => p == powerL3Param)));
      readingAccepter.Verify(ra => ra.Accept(It.Is<LiveReading[]>(lr => lr.Length == 1 && lr.First() == liveReading)));
    }

    [Test]
    public void AddStatusPostPvDeviceLabelAbsent()
    {
      // Arrange
      moduleConfigProvider.Setup(mcp => mcp.PvDeviceLabel).Returns(string.Empty);
      SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);

      // Act
      var response = browser.Post(AddStatusRoute, with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Body(new MemoryStream(Encoding.UTF8.GetBytes("TheRequestBody")), "TheRequestContentType");
        with.Query("d", "dV");
        with.Query("t", "tV");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(Nancy.HttpStatusCode.OK));
      Assert.That(forwardRequest.Method, Is.EqualTo("POST"));
      liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      readingAccepter.Verify(ra => ra.Accept(It.IsAny<LiveReading[]>()), Times.Never);
    }

    [Test]
    public void AddStatusGetPvDeviceLabelAbsent()
    {
      // Arrange
      moduleConfigProvider.Setup(mcp => mcp.PvDeviceLabel).Returns(string.Empty);
      SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);

      // Act
      var response = browser.Get(AddStatusRoute, with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(Nancy.HttpStatusCode.OK));
      Assert.That(forwardRequest.Method, Is.EqualTo("GET"));
      liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      readingAccepter.Verify(ra => ra.Accept(It.IsAny<LiveReading[]>()), Times.Never);
    }

    [Test]
    public void AddStatusPostCopiesContentTypeAndBodyContent()
    {
      // Arrange
      const string requestContentType = "TheRequestContentType";
      var requestBody = Encoding.UTF8.GetBytes("TheRequestBody");
      const string responseContentType = "TheResponseContentType";
      var responseBody = Encoding.UTF8.GetBytes("TheResponseBody");
      SetupResponse(HttpStatusCode.OK, responseContentType, responseBody);

      // Act
      var response = browser.Post(AddStatusRoute, with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Body(new MemoryStream(requestBody), requestContentType);
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(Nancy.HttpStatusCode.OK));
      Assert.That(forwardRequest.ContentType, Is.EqualTo(requestContentType));
      Assert.That(forwardRequest.GetContentBytes(), Is.EqualTo(requestBody));
      Assert.That(response.ContentType, Is.EqualTo(responseContentType));
      Assert.That(response.Body, Is.EqualTo(responseBody));
    }

    [Test]
    public void AddStatusPostCopiesPvOutputHeaders()
    {
      // Arrange
      SetupResponse(HttpStatusCode.OK, string.Empty, new byte[0]);
      forwardResponse.Headers.Add("X-Rate-Limit-Remaining", "remaining");
      forwardResponse.Headers.Add("x-rate-limit-limit", "limit");
      forwardResponse.Headers.Add("X-RATE-LIMIT-RESET", "reset");

      // Act
      var response = browser.Post(AddStatusRoute, with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
        with.Header("X-PVOUTPUT-APIKEY", "whatnot");
        with.Header("x-pvoutput-systemid", "whatelsenot");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(Nancy.HttpStatusCode.OK));
      Assert.That(forwardRequest.Headers["x-pvoutput-apikey"], Is.EqualTo("whatnot"));
      Assert.That(forwardRequest.Headers["X-PVOUTPUT-SYSTEMID"], Is.EqualTo("whatelsenot"));
      Assert.That(response.Headers["X-Rate-Limit-Remaining"], Is.EqualTo("remaining"));
      Assert.That(response.Headers["X-Rate-Limit-Limit"], Is.EqualTo("limit"));
      Assert.That(response.Headers["X-Rate-Limit-Reset"], Is.EqualTo("reset"));
    }

    [Test]
    public void AddStatusPostPropagatesHttpWebExceptionRequest()
    {
      // Arrange
      var exception = new HttpWebException("Drugs are baaad - m'kay", WebExceptionStatus.ConnectFailure, null);
      forwardRequest.SetRequestStream(exception);

      // Act
      var response = browser.Post(AddStatusRoute, with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(Nancy.HttpStatusCode.InternalServerError));
      Assert.That(response.ContentType, Is.EqualTo("text/plain; charset=utf-8"));
      Assert.That(Encoding.UTF8.GetString(response.Body.ToArray()), Is.EqualTo("Internal error interacting with PVOutput. Error message:" + exception.Message));

      liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
        It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      readingAccepter.Verify(ra => ra.Accept(It.IsAny<LiveReading[]>()), Times.Never);
    }

    [Test]
    public void AddStatusPostPropagatesHttpWebExceptionResponse()
    {
      // Arrange
      var responseBodyContent = Encoding.UTF8.GetBytes("TheResponseBodyContent");
      forwardResponse.StatusCode = HttpStatusCode.BadRequest;
      forwardResponse.ContentType = "some/contentType";
      forwardResponse.SetContentBytes(responseBodyContent);

      var exception = new HttpWebException("Drugs are baaad - m'kay", WebExceptionStatus.ProtocolError, forwardResponse);
      forwardRequest.SetResponse(exception);

      // Act
      var response = browser.Post(AddStatusRoute, with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(Nancy.HttpStatusCode.BadRequest));
      Assert.That(response.ContentType, Is.EqualTo(forwardResponse.ContentType));
      Assert.That(response.Body.ToArray(), Is.EqualTo(responseBodyContent));

      liveReadingMapper.Verify(lrm => lrm.MapPvOutputArgs(
        It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), 
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
      readingAccepter.Verify(ra => ra.Accept(It.IsAny<LiveReading[]>()), Times.Never);
    }

    [Test]
    public void AddStatusPostPropagatesHttpWebExceptionResponseLessLogging()
    {
      // Arrange
      var responseBodyContent = Encoding.UTF8.GetBytes("TheResponseBodyContent");
      forwardResponse.StatusCode = HttpStatusCode.BadRequest;
      forwardResponse.ContentType = "some/contentType";
      forwardResponse.SetContentBytes(responseBodyContent);

      var exception = new HttpWebException("Drugs are baaad - m'kay", WebExceptionStatus.ConnectFailure, forwardResponse);
      forwardRequest.SetResponse(exception);

      // Act
      var response = browser.Post(AddStatusRoute, with => 
      {
        with.HttpRequest();
        with.HostName("localhost");
      });

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(Nancy.HttpStatusCode.BadRequest));
      Assert.That(response.ContentType, Is.EqualTo(forwardResponse.ContentType));
      Assert.That(response.Body.ToArray(), Is.EqualTo(responseBodyContent));

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

      // Act & Assert
      Assert.That(() => browser.Post(AddStatusRoute, with => {with.HttpRequest(); with.HostName("localhost");}), Throws.TypeOf<Exception>());
    }

    private void SetupResponse(HttpStatusCode statusCode, string contentType, byte[] responseBodyContent)
    {
      forwardRequest.SetResponse(forwardResponse);

      forwardResponse.StatusCode = statusCode;
      forwardResponse.ContentType = contentType;
      forwardResponse.SetContentBytes(responseBodyContent);
    }

    public static Stream EqualTo(byte[] expectedBytes)
    {
      return Match.Create<Stream>(s =>
      {
        using (var ms = new MemoryStream())
        {
          s.CopyTo(ms);
          var sArray = ms.ToArray();
          return sArray.SequenceEqual(expectedBytes);
        }
      });
    }

  }
}

