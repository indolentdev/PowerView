using System;
using NUnit.Framework;
using Moq;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class TimeConverterTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new TimeConverter(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void ChangeTimeZoneFromUtcThrows()
    {
      // Arrange
      var target = CreateTarget();
      var dateTimeUnspecified = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Unspecified);

      // Act & Assert
      Assert.That(() => target.ChangeTimeZoneFromUtc(dateTimeUnspecified), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.ChangeTimeZoneFromUtc(DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ChangeTimeZoneFromUtc()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Utc);

      // Act
      var changedDateTime = target.ChangeTimeZoneFromUtc(dateTime);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 12, 30, 11, 31, 45)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Unspecified));
    }

    [Test]
    public void ChangeTimeZoneToUtcThrows()
    {
      // Arrange
      var target = CreateTarget();
      var dateTimeLocal = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Local);

      // Act & Assert
      Assert.That(() => target.ChangeTimeZoneToUtc(dateTimeLocal), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.ChangeTimeZoneToUtc(DateTime.UtcNow), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ChangeTimeToUtc()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Unspecified);

      // Act
      var changedDateTime = target.ChangeTimeZoneToUtc(dateTime);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 12, 30, 23, 31, 45)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void ReduceDay()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Local);

      // Act
      var changedDateTime = target.Reduce(dateTime, DateTimeResolution.Day);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 12, 30, 0, 0, 0)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Local));
    }

    [Test]
    public void ReduceMonth()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Local);

      // Act
      var changedDateTime = target.Reduce(dateTime, DateTimeResolution.Month);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 12, 1, 0, 0, 0)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Local));
    }

    [Test]
    public void ReduceYear()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Local);

      // Act
      var changedDateTime = target.Reduce(dateTime, DateTimeResolution.Year);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 1, 1, 0, 0, 0)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Local));
    }

    [Test]
    public void IsGreaterThanResolutionFraction()
    {
      // Arrange
      var target = CreateTarget();
      var resolution = DateTimeResolution.Day;
      var fraction = 1.0;
      var dateTimeUnspecified = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Unspecified);

      // Act & Assert
      Assert.That(() => target.IsGreaterThanResolutionFraction(resolution, fraction, dateTimeUnspecified), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.IsGreaterThanResolutionFraction(resolution, fraction, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void IsGreaterThanResolutionFractionDayTrue()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 12, 30, 20, 31, 45, DateTimeKind.Utc); // 14:31:45 El_Salvador time..
      var fraction = 14.5d / 24d; // 14:30 out of 24 hours..

      // Act
      var result = target.IsGreaterThanResolutionFraction(DateTimeResolution.Day, fraction, dateTime);

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    public void IsGreaterThanResolutionFractionDayFalse()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 12, 30, 20, 21, 45, DateTimeKind.Utc); // 14:21:45 El_Salvador time..
      var fraction = 14.5d / 24d; // 14:30 out of 24 hours..

      // Act
      var result = target.IsGreaterThanResolutionFraction(DateTimeResolution.Day, fraction, dateTime);

      // Assert
      Assert.That(result, Is.False);
    }

    [Test]
    public void IsGreaterThanResolutionFractionMonthTrue()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 12, 18, 20, 31, 45, DateTimeKind.Utc);
      var fraction = 17d / 31d; // 17 out of 31 days..

      // Act
      var result = target.IsGreaterThanResolutionFraction(DateTimeResolution.Month, fraction, dateTime);

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    public void IsGreaterThanResolutionFractionMonthFalse()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 12, 18, 20, 31, 45, DateTimeKind.Utc);
      var fraction = 18d / 31d; // 18 out of 31 days..

      // Act
      var result = target.IsGreaterThanResolutionFraction(DateTimeResolution.Month, fraction, dateTime);

      // Assert
      Assert.That(result, Is.False);
    }

    [Test]
    public void IsGreaterThanResolutionFractionYearTrue()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 11, 18, 20, 31, 45, DateTimeKind.Utc);
      var fraction = 10d / 12d; // 10 out of 12 months..

      // Act
      var result = target.IsGreaterThanResolutionFraction(DateTimeResolution.Year, fraction, dateTime);

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    public void IsGreaterThanResolutionFractionYearFalse()
    {
      // Arrange
      var target = CreateTarget();
      var dateTime = new DateTime(2015, 10, 31, 23, 31, 45, DateTimeKind.Utc);
      var fraction = 10d / 12d; // 10 out of 12 months..

      // Act
      var result = target.IsGreaterThanResolutionFraction(DateTimeResolution.Year, fraction, dateTime);

      // Assert
      Assert.That(result, Is.False);
    }

    private TimeConverter CreateTarget(string timeZoneId = "America/El_Salvador")
    {
      var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
      var lp = new Mock<ILocationProvider>();
      lp.Setup(x => x.GetTimeZone()).Returns(tzi);
      return new TimeConverter(lp.Object);
    }

  }
}

