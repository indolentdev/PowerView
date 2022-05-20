using System;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Moq;
using PowerView.Service.EventHub;
using PowerView.Model.Repository;

namespace PowerView.Service.Test.EventHub
{  
  [TestFixture]
  public class LocationResolverTest
  {
    private Mock<IHttpWebRequestFactory> httpWebRequestFactory;
    private Mock<ISettingRepository> settingRepository;

    [SetUp]
    public void SetUp()
    {
      httpWebRequestFactory = new Mock<IHttpWebRequestFactory>();
      settingRepository = new Mock<ISettingRepository>();
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var logger = new NullLogger<LocationResolver>();

      // Act & Assert
      Assert.That(() => new LocationResolver(null, httpWebRequestFactory.Object, settingRepository.Object), Throws.ArgumentNullException);
      Assert.That(() => new LocationResolver(logger, null, settingRepository.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new LocationResolver(logger, httpWebRequestFactory.Object, null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void ResolveHttpError()
    {
      // Arrange
      var request = new HttpWebRequestMock();
      request.SetResponse(new HttpWebException());
      httpWebRequestFactory.Setup(f => f.Create(It.IsAny<Uri>())).Returns(request);
      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ResolveJsonError()
    {
      // Arrange
      SetupHttpFactory("Bad json object");
      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ResolveEmptyContentError()
    {
      // Arrange
      SetupHttpFactory(string.Empty);
      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ResolveContentError()
    {
      // Arrange
      SetupHttpFactory("{\"status\":\"failed\"}");
      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ResolveSuccessNoProperties()
    {
      // Arrange
      SetupHttpFactory("{\"status\":\"success\"}");
      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ResolveTimeZone()
    {
      // Arrange
      var timeZoneId = "TheTimeZone";
      SetupHttpFactory("{\"status\":\"success\",\"timezone\":\"" + timeZoneId + "\"}");

      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      settingRepository.Verify(r => r.Upsert(Settings.TimeZoneId, timeZoneId));
    }

    [Test]
    public void ResolveCountryToOneCultureInfo()
    {
      // Arrange
      SetupHttpFactory("{\"status\":\"success\",\"country\":\"Czechia\"}");

      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      settingRepository.Verify(r => r.Upsert(Settings.CultureInfoName, "cs-CZ"));
    }

    [Test]
    public void ResolveCountryToNoCultureInfo()
    {
      // Arrange
      SetupHttpFactory("{\"status\":\"success\",\"country\":\"CountryThatDoesNotExist\"}");

      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      settingRepository.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ResolveCountryToMoreThanOneCultureInfo()
    {
      // Arrange
      SetupHttpFactory("{\"status\":\"success\",\"country\":\"Denmark\"}");

      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      settingRepository.Verify(r => r.Upsert(Settings.CultureInfoName, "da-DK"));
    }

    private void SetupHttpFactory(string responseContent)
    {
      var response = new HttpWebResponseMock();
      response.SetContentBytes(Encoding.UTF8.GetBytes(responseContent));
      var request = new HttpWebRequestMock();
      request.SetResponse(response);
      httpWebRequestFactory.Setup(f => f.Create(It.IsAny<Uri>())).Returns(request);
    }

    private LocationResolver CreateTarget()
    {
      return new LocationResolver(new NullLogger<LocationResolver>(), httpWebRequestFactory.Object, settingRepository.Object);
    }

  }
}

