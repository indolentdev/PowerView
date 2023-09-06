using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PowerView.Model.Test;

[TestFixture]
public class CostBreakDownTest
{
    [Test]
    public void ConstructorThrows()
    {
        // Arrange
        const string title = "theTitle";
        var curency = Unit.Eur;
        const int vat = 2;
        var entries = new List<CostBreakdownEntry>();

        // Act & Assert
        Assert.That(() => new CostBreakdown(null, curency, vat, entries), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdown(string.Empty, curency, vat, entries), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdown(title, Unit.Watt, vat, entries), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdown(title, curency, -1, entries), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdown(title, curency, 101, entries), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdown(title, curency, vat, null), Throws.ArgumentNullException);
    }

    [Test]
    public void ConstructorAndProperties()
    {
        // Arrange
        const string title = "theTitle";
        var currency = Unit.Eur;
        const int vat = 2;
        var entries = new List<CostBreakdownEntry> { new CostBreakdownEntry(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(4), "theName", 3, 21, 12.34) }; 

        // Act
        var target = new CostBreakdown(title, currency, vat, entries);

        // Assert
        Assert.That(target.Title, Is.EqualTo(title));
        Assert.That(target.Currency, Is.EqualTo(currency));
        Assert.That(target.Vat, Is.EqualTo(vat));
        Assert.That(target.Entries, Is.EqualTo(entries));
    }

}