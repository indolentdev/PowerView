using System;
using NUnit.Framework;
using Moq;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class LocationProviderTest
  {
    private Mock<ISettingRepository> settingRepository;

    [SetUp]
    public void SetUp()
    { 
      settingRepository = new Mock<ISettingRepository>();
    }

    [Test]
    public void ConstructurThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new LocationProvider(null, "Not null or empty", "Not null or empty"), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetTimeZoneFromConfig()
    {
      // Arrange
      var timeZone = "Europe/Berlin";
      var target = CreateTarget(configuredTimeZoneId: timeZone);

      // Act
      var timeZoneInfo = target.GetTimeZone();

      // Assert
      Assert.That(timeZoneInfo.Id, Is.EqualTo(timeZone));
    }

    [Test]
    public void GetTimeZoneFromDb()
    {
      // Arrange
      const string timeZone = "Europe/Berlin";
      settingRepository.Setup(sr => sr.Get(Settings.TimeZoneId)).Returns(timeZone);
      var target = CreateTarget();

      // Act
      var timeZoneInfo = target.GetTimeZone();

      // Assert
      Assert.That(timeZoneInfo.Id, Is.EqualTo(timeZone));
    }

    [Test]
    public void GetTimeZoneFromDefault()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var timeZoneInfo = target.GetTimeZone();

      // Assert
      Assert.That(timeZoneInfo.Id, Is.Not.EqualTo("UTC"));
    }

    [Test]
    public void GetTimeZoneBadTimeZoneFallsThrough()
    {
      // Arrange
      var target = CreateTarget(configuredTimeZoneId: "BadTimeZone");

      // Act
      var timeZoneInfo = target.GetTimeZone();

      // Assert
      Assert.That(timeZoneInfo.Id, Is.Not.EqualTo("UTC"));
    }

    [Test]
    public void GetTimeZoneFromConfigTakesPrecedence()
    {
      // Arrange
      var timeZone = "Europe/Berlin";
      var target = CreateTarget(configuredTimeZoneId: timeZone);

      // Act
      target.GetTimeZone();

      // Assert
      settingRepository.Verify(sr => sr.Get(It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetTimeZoneBadFromDatabaseProceedsToDefault()
    {
      // Arrange
      var target = CreateTarget(configuredTimeZoneId: null);

      // Act
      var timeZoneInfo = target.GetTimeZone();

      // Assert
      settingRepository.Verify(sr => sr.Get(It.IsAny<string>()), Times.Once());
      Assert.That(timeZoneInfo, Is.Not.Null);
    }

    [Test]
    public void GetCultureInfoFromConfig()
    {
      // Arrange
      var cultureInfoName = "de-DE";
      var target = CreateTarget(configuredCultureInfo: cultureInfoName);

      // Act
      var cultureInfo = target.GetCultureInfo();

      // Assert
      Assert.That(cultureInfo.Name, Is.EqualTo(cultureInfoName));
    }

    [Test]
    public void GetCultureInfoFromDb()
    {
      // Arrange
      const string cultureInfoName = "de-DE";
      settingRepository.Setup(sr => sr.Get(Settings.CultureInfoName)).Returns(cultureInfoName);
      var target = CreateTarget();

      // Act
      var cultureInfo = target.GetCultureInfo();

      // Assert
      Assert.That(cultureInfo.Name, Is.EqualTo(cultureInfoName));
    }

    [Test]
    public void GetCultureInfoFromDefault()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var cultureInfo = target.GetCultureInfo();

      // Assert
      Assert.That(cultureInfo.Name, Is.EqualTo(System.Globalization.CultureInfo.CurrentCulture.Name));
    }

    [Test]
    public void GetCultureInfoBadCultureInfoFallsThrough()
    {
      // Arrange
      var target = CreateTarget(configuredCultureInfo: "BadCultureInfo");

      // Act
      var cultureInfo = target.GetCultureInfo();

      // Assert
      Assert.That(cultureInfo.Name, Is.EqualTo(System.Globalization.CultureInfo.CurrentCulture.Name));
    }

    [Test]
    public void GetCultureInfoFromConfigTakesPrecedence()
    {
      // Arrange
      var cultureInfoName = "de-DE";
      var target = CreateTarget(configuredCultureInfo: cultureInfoName);

      // Act
      target.GetCultureInfo();

      // Assert
      settingRepository.Verify(sr => sr.Get(It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetCultureInfoBadFromDatabaseProceedsToDefault()
    {
      // Arrange
      var target = CreateTarget(configuredCultureInfo: null);

      // Act
      var cultureInfo = target.GetCultureInfo();

      // Assert
      settingRepository.Verify(sr => sr.Get(It.IsAny<string>()), Times.Once());
      Assert.That(cultureInfo, Is.Not.Null);
    }

    private LocationProvider CreateTarget(string configuredTimeZoneId = null, string configuredCultureInfo = null)
    {
      return new LocationProvider(settingRepository.Object, configuredTimeZoneId, configuredCultureInfo);
    }

  }
}
