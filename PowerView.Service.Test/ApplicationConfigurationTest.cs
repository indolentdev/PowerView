using System;
using System.Globalization;
using NUnit.Framework;

namespace PowerView.Service.Test
{
  [TestFixture]
  public class ApplicationConfigurationTest
  {
    [Test]
    public void SetupThrows()
    {
      // Arrange
      var timeZoneInfo = TimeZoneInfo.Local;
      var cultureInfo = CultureInfo.CurrentCulture;
      var target = new ApplicationConfiguration();

      // Act & Assert
      Assert.That(() => target.Setup(null, cultureInfo), Throws.ArgumentNullException);
      Assert.That(() => target.Setup(timeZoneInfo, null), Throws.ArgumentNullException);
    }

    [Test]
    public void SetupAndProperties()
    {
      // Arrange
      var timeZoneInfo = TimeZoneInfo.Local;
      var cultureInfo = CultureInfo.CurrentCulture;
      var target = new ApplicationConfiguration();

      // Act
      target.Setup(timeZoneInfo, cultureInfo);

      // Assert
      Assert.That(target.TimeZoneInfo, Is.SameAs(timeZoneInfo));
      Assert.That(target.CultureInfo, Is.SameAs(cultureInfo));
    }

  }
}
