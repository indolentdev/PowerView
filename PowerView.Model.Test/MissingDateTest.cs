using System;
using NUnit.Framework;

namespace PowerView.Model.Test;

[TestFixture]
public class MissingDateTest
{
    [Test]
    public void ConstructorThrows()
    {
        // Arrange
        DateTime dt = DateTime.UtcNow;

        // Act & Assert
        Assert.That(() => new MissingDate(DateTime.Now, dt, dt), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new MissingDate(dt, DateTime.Now, dt), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new MissingDate(dt, dt, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
        // Arrange
        var dt = DateTime.UtcNow;
        var prev = dt.AddDays(-3);
        var next = dt.AddDays(4);

        // Act
        var target = new MissingDate(dt, prev, next);

        // Assert
        Assert.That(target.Timestamp, Is.EqualTo(dt));
        Assert.That(target.PreviousTimestamp, Is.EqualTo(prev));
        Assert.That(target.NextTimestamp, Is.EqualTo(next));
    }

}
