using System;
using System.Collections.Generic;
using System.Linq;
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

    [Test]
    public void GetEntriesByPeriods()
    {
        // Arrange
        var dateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var plus1Month = dateTime.AddMonths(1);
        var plus2Months = dateTime.AddMonths(2);
        var plus3Months = dateTime.AddMonths(3);
        var plus4Months = dateTime.AddMonths(4);
        var plus5Months = dateTime.AddMonths(5);

        var e1a = new CostBreakdownEntry(dateTime, plus3Months, "e1a", 0, 23, 11);
        var e1b = new CostBreakdownEntry(dateTime, plus3Months, "e1b", 0, 23, 12);
        var e2 = new CostBreakdownEntry(dateTime, plus5Months, "e2", 0, 23, 21);
        var e3 = new CostBreakdownEntry(plus1Month, plus2Months, "e3", 0, 23, 31);
        var e4 = new CostBreakdownEntry(plus2Months, plus4Months, "e4", 0, 23, 41);
        var e5 = new CostBreakdownEntry(plus2Months, plus5Months, "e5", 0, 23, 51);

        var target = new CostBreakdown("Hep", Unit.Dkk, 25, new[] { e1a, e1b, e2, e3, e4, e5 });

        // Act
        var entriesPeriods = target.GetEntriesByPeriods();

        // Assert
        AssertEntryGroup((dateTime, plus3Months), new [] { e1a, e1b, e3 }, entriesPeriods);
        AssertEntryGroup((dateTime, plus5Months), new[] { e1a, e1b, e2, e3, e4, e5 }, entriesPeriods);
        AssertEntryGroup((plus1Month, plus2Months), new[] { e3 }, entriesPeriods);
        AssertEntryGroup((plus2Months, plus4Months), new[] { e4 }, entriesPeriods);
        AssertEntryGroup((plus2Months, plus5Months), new[] { e4, e5 }, entriesPeriods);
        Assert.That(entriesPeriods.Count, Is.EqualTo(5));
    }

    private static void AssertEntryGroup((DateTime FromDate, DateTime ToDate) expectedKey, CostBreakdownEntry[] expectedValue, IDictionary<(DateTime FromDate, DateTime ToDate), IReadOnlyList<CostBreakdownEntry>> actual)
    {
        Assert.That(actual.Keys, Contains.Item(expectedKey));
        Assert.That(actual[expectedKey], Is.EquivalentTo(expectedValue));
    }

}