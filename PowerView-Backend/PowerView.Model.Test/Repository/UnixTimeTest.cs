using System;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository;

[TestFixture]
public class UnixTimeTest
{
    [Test]
    public void ConstructorDateTimeAndToDateTime()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        var target = new UnixTime(dateTime);
        var dateTime2 = target.ToDateTime();

        // Assert
        Assert.That(dateTime2, Is.EqualTo(dateTime));
        Assert.That(dateTime2.Kind, Is.EqualTo(dateTime.Kind));
    }

    [Test]
    public void ConstructorLongAndToUnixTimeSeconds()
    {
        // Arrange
        var unixTime = 12354561;

        // Act
        var target = new UnixTime(unixTime);
        var unixTime2 = target.ToUnixTimeSeconds();

        // Assert
        Assert.That(unixTime2, Is.EqualTo(unixTime));
    }

    [Test]
    public void ConstructorDefaultAndToDateTimeAndToUnixTimeSeconds()
    {
        // Arrange

        // Act
        var target = new UnixTime();

        // Assert
        Assert.That(target.ToDateTime(), Is.EqualTo(DateTime.UnixEpoch));
        Assert.That(target.ToUnixTimeSeconds(), Is.Zero);
    }

    [Test]
    public void ConstructorDateTimeThrows()
    {
        // Arrange

        // Act & Assert
        Assert.That(() => new UnixTime(DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new UnixTime(DateTime.UnixEpoch - TimeSpan.FromSeconds(1)), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Now()
    {
        // Arrange

        // Act
        var target = UnixTime.Now;

        // Assert
        Assert.That(target.ToDateTime(), Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromMilliseconds(100)));
    }

    [Test]
    [TestCase(100, 100, 0)]
    [TestCase(99, 100, -1)]
    [TestCase(100, 99, 1)]
    public void CompareTo(long long1, long long2, int result)
    {
        // Arrange
        var target1 = new UnixTime(long1);
        var target2 = new UnixTime(long2);

        // Act
        var compare = target1.CompareTo(target2);

        // Assert
        Assert.That(compare, Is.EqualTo(result));
    }
    

    [Test]
    public void ImplicitOperatorsDateTime()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        UnixTime target = now;
        DateTime now2 = target;

        // Assert
        Assert.That(now2, Is.EqualTo(now));
        Assert.That(now2.Kind, Is.EqualTo(now.Kind));
    }


}