using System;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PowerView.Service.EventHub;

namespace PowerView.Service.Test
{
  [TestFixture]
  public class IntervalTriggerTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var logger = new NullLogger<IntervalTrigger>();
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var dateTimeUnspecified = new DateTime(2019, 10, 11, 1, 2, 3, DateTimeKind.Unspecified);

      // Act & Assert
      Assert.That(() => new IntervalTrigger(null, locationContext, DateTime.UtcNow), Throws.ArgumentNullException);
      Assert.That(() => new IntervalTrigger(logger, null, DateTime.UtcNow), Throws.ArgumentNullException);
      Assert.That(() => new IntervalTrigger(logger, locationContext, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new IntervalTrigger(logger, locationContext, dateTimeUnspecified), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void SetupThrows()
    {
      // Arrange
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var target = new IntervalTrigger(new NullLogger<IntervalTrigger>(), locationContext, DateTime.UtcNow);

      // Act & Assert
      Assert.That(() => target.Setup(TimeSpan.FromDays(1), TimeSpan.FromDays(1)), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase(0, false)]
    [TestCase(1454, false)]
    [TestCase(1455, true)]
    [TestCase(1456, true)]
    public void IsTriggerTime(int minutes, bool expected)
    {
      // Arrange
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var baseDateTime = TimeZoneHelper.GetDenmarkFixedMidnightAsUtc();
      var target = new IntervalTrigger(new NullLogger<IntervalTrigger>(), locationContext, baseDateTime);
      target.Setup(TimeSpan.FromMinutes(15), TimeSpan.FromDays(1));

      // Act
      var isTrigger = target.IsTriggerTime(baseDateTime + TimeSpan.FromMinutes(minutes));

      // Assert
      Assert.That(isTrigger, Is.EqualTo(expected));
    }

    [Test]
    public void AdvanceThrows()
    {
      // Arrange
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var baseDateTime = TimeZoneHelper.GetDenmarkFixedMidnightAsUtc();
      var target = new IntervalTrigger(new NullLogger<IntervalTrigger>(), locationContext, baseDateTime);
      target.Setup(TimeSpan.FromMinutes(15), TimeSpan.FromDays(1));
      var dateTimeUnspecified = new DateTime(2019, 10, 11, 1, 2, 3, DateTimeKind.Unspecified);

      // Act & Assert
      Assert.That(() => target.Advance(DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.Advance(dateTimeUnspecified), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void IsTriggerTimeAndAdvance()
    {
      // Arrange
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var baseDateTime = TimeZoneHelper.GetDenmarkFixedMidnightAsUtc();
      var target = new IntervalTrigger(new NullLogger<IntervalTrigger>(), locationContext, baseDateTime);
      target.Setup(TimeSpan.FromMinutes(15), TimeSpan.FromDays(1));

      // Act & Assert
      var dateTime = baseDateTime.AddMinutes(15);
      Assert.That(target.IsTriggerTime(dateTime), Is.False);

      dateTime = dateTime.AddDays(1);
      Assert.That(target.IsTriggerTime(dateTime), Is.True);
      target.Advance(dateTime);

      dateTime = dateTime.AddDays(1);
      Assert.That(target.IsTriggerTime(dateTime), Is.True);
      target.Advance(dateTime);
    }

    [Test]
    public void IsTriggerTimeAndAdvanceDoubleInterval()
    {
      // Arrange
      var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
      var baseDateTime = TimeZoneHelper.GetDenmarkFixedMidnightAsUtc();
      var target = new IntervalTrigger(new NullLogger<IntervalTrigger>(), locationContext, baseDateTime);
      target.Setup(TimeSpan.FromMinutes(15), TimeSpan.FromDays(1));

      // Act & Assert
      var dateTime = baseDateTime.AddMinutes(15);
      Assert.That(target.IsTriggerTime(dateTime), Is.False);

      dateTime = dateTime.AddDays(2);
      Assert.That(target.IsTriggerTime(dateTime), Is.True);
      target.Advance(dateTime);

      dateTime = dateTime.AddDays(1);
      Assert.That(target.IsTriggerTime(dateTime), Is.True);
      target.Advance(dateTime);
    }

  }
}
