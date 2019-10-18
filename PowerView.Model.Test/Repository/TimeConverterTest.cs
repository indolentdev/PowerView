using System;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class TimeConverterTest
  {
  
    [Test]
    public void ReduceDay()
    {
      // Arrange
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Unspecified);

      // Act
      var changedDateTime = TimeConverter.Reduce(DateTimeResolution.Day, dateTime);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 12, 30, 0, 0, 0)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Unspecified));
    }

    [Test]
    public void ReduceMonth()
    {
      // Arrange
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Unspecified);

      // Act
      var changedDateTime = TimeConverter.Reduce(DateTimeResolution.Month, dateTime);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 12, 1, 0, 0, 0)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Unspecified));
    }

    [Test]
    public void ReduceYear()
    {
      // Arrange
      var dateTime = new DateTime(2015, 12, 30, 17, 31, 45, DateTimeKind.Unspecified);

      // Act
      var changedDateTime = TimeConverter.Reduce(DateTimeResolution.Year, dateTime);

      // Assert
      Assert.That(changedDateTime, Is.EqualTo(new DateTime(2015, 1, 1, 0, 0, 0)));
      Assert.That(changedDateTime.Kind, Is.EqualTo(DateTimeKind.Unspecified));
    }

    [Test]
    public void IsGreaterThanResolutionFraction()
    {
      // Arrange
      var resolution = DateTimeResolution.Day;
      var fraction = 1.0;

      // Act & Assert
      Assert.That(() => TimeConverter.IsGreaterThanResolutionFraction(resolution, fraction, DateTime.UtcNow), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => TimeConverter.IsGreaterThanResolutionFraction(resolution, fraction, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void IsGreaterThanResolutionFractionDayTrue()
    {
      // Arrange
      var dateTime = new DateTime(2015, 12, 30, 14, 31, 45, DateTimeKind.Unspecified);
      var fraction = 14.5d / 24d; // 14:30 out of 24 hours..

      // Act
      var result = TimeConverter.IsGreaterThanResolutionFraction(DateTimeResolution.Day, fraction, dateTime);

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    public void IsGreaterThanResolutionFractionDayFalse()
    {
      // Arrange
      var dateTime = new DateTime(2015, 12, 30, 14, 29, 45, DateTimeKind.Unspecified);
      var fraction = 14.5d / 24d; // 14:30 out of 24 hours..

      // Act
      var result = TimeConverter.IsGreaterThanResolutionFraction(DateTimeResolution.Day, fraction, dateTime);

      // Assert
      Assert.That(result, Is.False);
    }

    [Test]
    public void IsGreaterThanResolutionFractionMonthTrue()
    {
      // Arrange
      var dateTime = new DateTime(2015, 12, 18, 20, 31, 45, DateTimeKind.Unspecified);
      var fraction = 17d / 31d; // 17 out of 31 days..

      // Act
      var result = TimeConverter.IsGreaterThanResolutionFraction(DateTimeResolution.Month, fraction, dateTime);

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    public void IsGreaterThanResolutionFractionMonthFalse()
    {
      // Arrange
      var dateTime = new DateTime(2015, 12, 18, 20, 31, 45, DateTimeKind.Unspecified);
      var fraction = 18d / 31d; // 18 out of 31 days..

      // Act
      var result = TimeConverter.IsGreaterThanResolutionFraction(DateTimeResolution.Month, fraction, dateTime);

      // Assert
      Assert.That(result, Is.False);
    }

    [Test]
    public void IsGreaterThanResolutionFractionYearTrue()
    {
      // Arrange
      var dateTime = new DateTime(2015, 11, 18, 20, 31, 45, DateTimeKind.Unspecified);
      var fraction = 10d / 12d; // 10 out of 12 months..

      // Act
      var result = TimeConverter.IsGreaterThanResolutionFraction(DateTimeResolution.Year, fraction, dateTime);

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    public void IsGreaterThanResolutionFractionYearFalse()
    {
      // Arrange
      var dateTime = new DateTime(2015, 10, 31, 23, 31, 45, DateTimeKind.Unspecified);
      var fraction = 10d / 12d; // 10 out of 12 months..

      // Act
      var result = TimeConverter.IsGreaterThanResolutionFraction(DateTimeResolution.Year, fraction, dateTime);

      // Assert
      Assert.That(result, Is.False);
    }

  }
}
