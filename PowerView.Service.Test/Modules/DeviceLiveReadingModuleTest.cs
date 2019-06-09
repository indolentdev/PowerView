using System;
using System.IO;
using System.Linq;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;
using PowerView.Service.Modules;
using Moq;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;

namespace PowerView.Service.Test.Modules
{
  [TestFixture]
  public class DeviceLiveReadingModuleTest
  {
    private Mock<ILiveReadingMapper> liveReadingMapper;
    private Mock<IReadingAccepter> readingAccepter;

    private Browser browser;

    [SetUp]
    public void SetUp()
    {
      liveReadingMapper = new Mock<ILiveReadingMapper>();
      readingAccepter = new Mock<IReadingAccepter>();

      browser = new Browser(cfg =>
      {
        cfg.Module<DeviceLiveReadingModule>();
        cfg.Dependency<ILiveReadingMapper>(liveReadingMapper.Object);
        cfg.Dependency<IReadingAccepter>(readingAccepter.Object);
      });
    }

    [Test]
    public void LiveReadingPost()
    {
      // Arrange
      var liveReading = new LiveReading("lbl", "1", DateTime.UtcNow, new [] { new RegisterValue("1.2.3.4.5.6", 1, 0, Unit.Watt) });
      liveReadingMapper.Setup(lrm => lrm.Map(It.IsAny<string>(), It.IsAny<Stream>())).Returns(new [] { liveReading });

      // Act
      var result = browser.Post("/api/devices/livereadings", with => with.HttpRequest());

      // Assert
      Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      readingAccepter.Verify(lrr => lrr.Accept(
        It.Is<LiveReading[]>(lr => lr.Length == 1 && lr.First() == liveReading)));
    }

    [Test]
    public void LiveReadingPostMapperThrowsArgumentOutOfRangeException()
    {
      // Arrange
      liveReadingMapper.Setup(lrm => lrm.Map(It.IsAny<string>(), It.IsAny<Stream>())).Throws(new ArgumentOutOfRangeException());

      // Act & Assert
      Assert.That(() => browser.Post("/api/devices/livereadings", with => with.HttpRequest()), Throws.TypeOf<Exception>());
    }

    [Test]
    public void LiveReadingPostRepositoryThrowsDataStoreException()
    {
      // Arrange
      readingAccepter.Setup(ra => ra.Accept(It.IsAny<LiveReading[]>())).Throws(new DataStoreException());

      // Act & Assert
      Assert.That(() => browser.Post("/api/devices/livereadings", with => with.HttpRequest()), Throws.TypeOf<Exception>());
    }

    [Test]
    public void LiveReadingPostRepositoryThrowsDataStoreBusyException()
    {
      // Arrange
      readingAccepter.Setup(ra => ra.Accept(It.IsAny<LiveReading[]>())).Throws(new DataStoreBusyException());

      // Act 
      var response = browser.Post("/api/devices/livereadings", with => with.HttpRequest());

      // Assert
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
      Assert.That(response.ReasonPhrase, Is.Not.Null);
    }

  }
}

