using System;
using System.Globalization;
using NUnit.Framework;

namespace PowerView.Service.Test
{
  [TestFixture]
  public class LocationContextTest
  {
    [Test]
    public void SetupThrows()
    {
      // Arrange
      var timeZoneInfo = TimeZoneInfo.Local;
      var cultureInfo = CultureInfo.CurrentCulture;
      var target = new LocationContext();

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
      var target = new LocationContext();

      // Act
      target.Setup(timeZoneInfo, cultureInfo);

      // Assert
      Assert.That(target.TimeZoneInfo, Is.SameAs(timeZoneInfo));
      Assert.That(target.CultureInfo, Is.SameAs(cultureInfo));
    }

    [Test]
    public void ConvertTimeZoneFromUtcThrows()
    {
      // Arrange
      var target = new LocationContext();
      target.Setup(TimeZoneHelper.GetDenmarkTimeZoneInfo(), CultureInfo.CurrentCulture);
      var dateTimeUnspecified = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Unspecified);

      // Act & Assert
      Assert.That(() => target.ConvertTimeFromUtc(dateTimeUnspecified), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.ConvertTimeFromUtc(DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConvertTimeZoneFromUtc()
    {
      // Arrange
      var target = new LocationContext();
      target.Setup(TimeZoneHelper.GetDenmarkTimeZoneInfo(), CultureInfo.CurrentCulture);
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Utc);

      // Act
      var changedDateTime = target.ConvertTimeFromUtc(dateTime);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 12, 30, 18, 31, 45)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Unspecified));
    }

    [Test]
    public void ConvertTimeZoneToUtcThrows()
    {
      // Arrange
      var target = new LocationContext();
      target.Setup(TimeZoneHelper.GetDenmarkTimeZoneInfo(), CultureInfo.CurrentCulture);
      var dateTimeLocal = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Local);

      // Act & Assert
      Assert.That(() => target.ConvertTimeToUtc(dateTimeLocal), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.ConvertTimeToUtc(DateTime.UtcNow), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConvertTimeToUtc()
    {
      // Arrange
      var target = new LocationContext();
      target.Setup(TimeZoneHelper.GetDenmarkTimeZoneInfo(), CultureInfo.CurrentCulture);
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Unspecified);

      // Act
      var changedDateTime = target.ConvertTimeToUtc(dateTime);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 12, 30, 16, 31, 45)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

  }
}
