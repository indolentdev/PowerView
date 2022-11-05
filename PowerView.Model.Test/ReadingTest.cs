using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PowerView.Model.Test;

[TestFixture]
public class ReadingTest
{
    [Test]
    public void ConstructorThrows()
    {
        // Arrange
        const string label = "lbl";
        const string deviceId = "deviceId";
        var timestamp = DateTime.UtcNow;
        var values = new List<RegisterValue> { new RegisterValue(ObisCode.ColdWaterFlow1, 1, 2, Unit.CubicMetrePrHour) };

        // Act & Assert
        Assert.That(() => new Reading(null, deviceId, timestamp, values), Throws.ArgumentNullException);
        Assert.That(() => new Reading(label, null, timestamp, values), Throws.ArgumentNullException);
        Assert.That(() => new Reading(label, deviceId, DateTime.Now, values), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Reading(label, deviceId, timestamp, null), Throws.ArgumentNullException);
    }

    [Test]
    public void ConstructorAndProperties()
    {
        // Arrange
        const string label = "lbl";
        const string deviceId = "deviceId";
        var timestamp = DateTime.UtcNow;
        var values = new List<RegisterValue> { new RegisterValue(ObisCode.ColdWaterFlow1, 1, 2, Unit.CubicMetrePrHour) };

        // Act
        var target = new Reading(label, deviceId, timestamp, values);

        // Assert
        Assert.That(target.Label, Is.EqualTo(label));
        Assert.That(target.DeviceId, Is.EqualTo(deviceId));
        Assert.That(target.Timestamp, Is.EqualTo(timestamp));
    }

    [Test]
    public void GetRegisterValues()
    {
        // Arrange
        const string label = "lbl";
        const string deviceId = "deviceId";
        var timestamp = DateTime.UtcNow;
        var values = new List<RegisterValue> { new RegisterValue(ObisCode.ColdWaterFlow1, 1, 2, Unit.CubicMetrePrHour) };
        var target = new Reading(label, deviceId, timestamp, values);

        // Act
        var targetValues = target.GetRegisterValues();

        // Assert
        Assert.That(targetValues, Is.EqualTo(values));
        Assert.That(targetValues, Is.Not.SameAs(values));
    }

}