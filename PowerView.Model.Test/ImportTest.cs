using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PowerView.Model.Test;

[TestFixture]
public class ImportTest
{
    [Test]
    public void ConstructorThrows()
    {
        // Arrange
        const string label = "theLabel";
        const string channel = "DK1";
        var currency = Unit.Eur;
        var fromTimestamp = new DateTime(2020, 11, 20, 23, 0, 0, DateTimeKind.Utc);
        var currentTimestamp = new DateTime(2021, 11, 20, 23, 0, 0, DateTimeKind.Utc);
        var enabled = true;

        // Act & Assert
        Assert.That(() => new Import(null, channel, currency, fromTimestamp, currentTimestamp, enabled), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Import(string.Empty, channel, currency, fromTimestamp, currentTimestamp, enabled), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Import(label, null, currency, fromTimestamp, currentTimestamp, enabled), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Import(label, string.Empty, currency, fromTimestamp, currentTimestamp, enabled), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Import(label, channel, Unit.Watt, fromTimestamp, currentTimestamp, enabled), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Import(label, channel, currency, DateTime.Now, currentTimestamp, enabled), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Import(label, channel, currency, fromTimestamp, DateTime.Now, enabled), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase(Unit.Dkk, true)]
    [TestCase(Unit.Eur, false)]
    public void ConstructorAndProperties(Unit currency, bool includeCurrentTimestamp)
    {
        // Arrange
        const string label = "theLabel";
        const string channel = "DK1";
        var fromTimestamp = new DateTime(2020, 11, 20, 23, 0, 0, DateTimeKind.Utc);
        DateTime? currentTimestamp = null;
        if (includeCurrentTimestamp) currentTimestamp = new DateTime(2021, 11, 20, 23, 0, 0, DateTimeKind.Utc);
        var enabled = true;

        // Act
        var target = new Import(label, channel, currency, fromTimestamp, currentTimestamp, enabled);

        // Assert
        Assert.That(target.Label, Is.EqualTo(label));
        Assert.That(target.Channel, Is.EqualTo(channel));
        Assert.That(target.Currency, Is.EqualTo(currency));
        Assert.That(target.FromTimestamp, Is.EqualTo(fromTimestamp));
        Assert.That(target.CurrentTimestamp, Is.EqualTo(currentTimestamp));
        Assert.That(target.Enabled, Is.EqualTo(enabled));
    }

}