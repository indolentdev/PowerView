using System;
using System.Text;
using Autofac.Features.OwnedInstances;
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
    private Mock<IFactory> repositoryFactory;

    private Mock<ISettingRepository> settingRepository;

    [SetUp]
    public void SetUp()
    {
      httpWebRequestFactory = new Mock<IHttpWebRequestFactory>();
      repositoryFactory = new Mock<IFactory>();

      settingRepository = new Mock<ISettingRepository>();
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new LocationResolver(null, repositoryFactory.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new LocationResolver(httpWebRequestFactory.Object, null), Throws.TypeOf<ArgumentNullException>());
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
      repositoryFactory.Verify(f => f.Create<ISettingRepository>(), Times.Never);
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
      repositoryFactory.Verify(f => f.Create<ISettingRepository>(), Times.Never);
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
      repositoryFactory.Verify(f => f.Create<ISettingRepository>(), Times.Never);
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
      repositoryFactory.Verify(f => f.Create<ISettingRepository>(), Times.Never);
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
      repositoryFactory.Verify(f => f.Create<ISettingRepository>(), Times.Never);
    }

    [Test]
    public void ResolveTimeZone()
    {
      // Arrange
      var timeZoneId = "TheTimeZone";
      SetupHttpFactory("{\"status\":\"success\",\"timezone\":\"" + timeZoneId + "\"}");
      SetupRepositoryFactory();

      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      repositoryFactory.Verify(f => f.Create<ISettingRepository>());
      settingRepository.Verify(r => r.Upsert(Settings.TimeZoneId, timeZoneId));
    }

    [Test]
    public void ResolveCountryToOneCultureInfo()
    {
      // Arrange
      SetupHttpFactory("{\"status\":\"success\",\"country\":\"Denmark\"}");
      SetupRepositoryFactory();

      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      repositoryFactory.Verify(f => f.Create<ISettingRepository>());
      settingRepository.Verify(r => r.Upsert(Settings.CultureInfoName, "da-DK"));
    }

    [Test]
    public void ResolveCountryToNoCultureInfo()
    {
      // Arrange
      SetupHttpFactory("{\"status\":\"success\",\"country\":\"CountryThatDoesNotExist\"}");
      SetupRepositoryFactory();

      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      repositoryFactory.Verify(f => f.Create<ISettingRepository>(), Times.Never);
    }

    [Test]
    public void ResolveCountryToMoreThanOneCultureInfo()
    {
      // Arrange
      SetupHttpFactory("{\"status\":\"success\",\"country\":\"United States\"}");
      SetupRepositoryFactory();

      var target = CreateTarget();

      // Act
      target.Resolve();

      // Assert
      repositoryFactory.Verify(f => f.Create<ISettingRepository>(), Times.Never);
    }

    private void SetupHttpFactory(string responseContent)
    {
      var response = new HttpWebResponseMock();
      response.SetContentBytes(Encoding.UTF8.GetBytes(responseContent));
      var request = new HttpWebRequestMock();
      request.SetResponse(response);
      httpWebRequestFactory.Setup(f => f.Create(It.IsAny<Uri>())).Returns(request);
    }

    private void SetupRepositoryFactory()
    {
      var owned = new Owned<ISettingRepository>(settingRepository.Object, new Mock<IDisposable>().Object);
      repositoryFactory.Setup(f => f.Create<ISettingRepository>()).Returns(owned);
    }

    private LocationResolver CreateTarget()
    {
      return new LocationResolver(httpWebRequestFactory.Object, repositoryFactory.Object);
    }

  }
}

