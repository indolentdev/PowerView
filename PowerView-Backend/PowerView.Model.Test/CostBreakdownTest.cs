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
        Assert.That(() => new CostBreakdown(null, curency, vat, entries), Throws.ArgumentNullException);
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
    public void GetEntriesByPeriodsEmpty()
    {
        // Arrange
        var target = new CostBreakdown("Hep", Unit.Dkk, 25, Array.Empty<CostBreakdownEntry>());

        // Act
        var entriesPeriods = target.GetEntriesByPeriods();

        // Assert
        Assert.That(entriesPeriods, Is.Empty);
    }

    [Test]
    public void GetEntriesByPeriods()
    {
        // Arrange
        var t1 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = t1.AddMonths(1);
        var t3 = t1.AddMonths(2);
        var t4 = new DateTime(2000, 3, 31, 0, 0, 0, DateTimeKind.Utc);
        var t5 = new DateTime(2000, 4, 30, 0, 0, 0, DateTimeKind.Utc);
        var t6 = new DateTime(2000, 5, 31, 0, 0, 0, DateTimeKind.Utc);
        var t7 = new DateTime(2000, 6, 30, 0, 0, 0, DateTimeKind.Utc);

        var e1a = new CostBreakdownEntry(t1, t4, "e1a", 0, 23, 11);
        var e1b = new CostBreakdownEntry(t1, t4, "e1b", 0, 23, 12);
        var e2 = new CostBreakdownEntry(t1, t5, "e2", 0, 23, 21);
        var e3 = new CostBreakdownEntry(t2, t5, "e3", 0, 23, 31);
        var e4 = new CostBreakdownEntry(t3, t6, "e4", 0, 23, 41);
        var e5 = new CostBreakdownEntry(t3, t7, "e5", 0, 23, 51);

        var target = new CostBreakdown("Hep", Unit.Dkk, 25, new[] { e1a, e1b, e2, e3, e4, e5 });

        // Act
        var entriesPeriods = target.GetEntriesByPeriods();

        // Assert
        Assert.That(entriesPeriods.Count, Is.EqualTo(6));
        AssertEntryGroup((t1, t2), new [] { e1a, e1b, e2 }, entriesPeriods);
        AssertEntryGroup((t2, t3), new[] { e1a, e1b, e2, e3 }, entriesPeriods);
        AssertEntryGroup((t3, t4), new[] { e1a, e1b, e2, e3, e4, e5 }, entriesPeriods);
        AssertEntryGroup((t4 + TimeSpan.FromDays(1), t5), new[] { e2, e3, e4, e5 }, entriesPeriods);
        AssertEntryGroup((t5 + TimeSpan.FromDays(1), t6), new[] { e4, e5 }, entriesPeriods);
        AssertEntryGroup((t6 + TimeSpan.FromDays(1), t7), new[] { e5 }, entriesPeriods);
    }

    [Test]
    public void ApplyThrows()
    {
        // Arrange
        var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
        var values = Array.Empty<NormalizedDurationRegisterValue>();
        var target = new CostBreakdown("Hep", Unit.Dkk, 25, new List<CostBreakdownEntry>());

        // Act & Assert
        Assert.That(() => target.Apply(null, values), Throws.ArgumentNullException);
        Assert.That(() => target.Apply(locationContext, null), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyEmpty()
    {
        // Arrange
        var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
        var values = Array.Empty<NormalizedDurationRegisterValue>();
        var entries = new List<CostBreakdownEntry> { new CostBreakdownEntry(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(4), "theName", 3, 21, 12.34) };
        var target = new CostBreakdown("theTitle", Unit.Eur, 25, entries);

        // Act
        var result = target.Apply(locationContext, values);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Apply()
    {
        // Arrange
        var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
        var values = new [] { new NormalizedDurationRegisterValue(
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new UnitValue(12.34, Unit.Eur), "EnergiDataService" ) };
        var entry = new CostBreakdownEntry(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
          new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), "entry", 3, 21, 12.34);
        var target = new CostBreakdown("theTitle", Unit.Eur, 25, new [] { entry });

        // Act
        var result = target.Apply(locationContext, values);

        // Assert
        Assert.That(result.Select(x => x.Value).ToList(), Is.EqualTo(new [] { new NormalizedDurationRegisterValue(
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new UnitValue(12.34*2*1.25, Unit.Eur), "EnergiDataService", "theTitle" )
        }));
        Assert.That(result.First().Entries, Is.EqualTo(new[] { entry }));
    }

    [Test]
    public void ApplySkippedByEntryDate()
    {
        // Arrange
        var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
        var values = new[] { new NormalizedDurationRegisterValue(
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new UnitValue(12.34, Unit.Eur), "EnergiDataService" ) };
        var entry = new CostBreakdownEntry(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
          new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), "entry", 3, 21, 12.34);
        var target = new CostBreakdown("theTitle", Unit.Eur, 25, new[] { entry });

        // Act
        var result = target.Apply(locationContext, values);

        // Assert
        Assert.That(result.Select(x => x.Value).ToList(), Is.EqualTo(new[] { new NormalizedDurationRegisterValue(
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new UnitValue(12.34*1.25, Unit.Eur), "EnergiDataService", "theTitle" )
        }));
        Assert.That(result.First().Entries, Is.Empty);
    }

    [Test]
    public void ApplySkippedByEntryTime()
    {
        // Arrange
        var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
        var values = new[] { new NormalizedDurationRegisterValue(
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new UnitValue(12.34, Unit.Eur), "EnergiDataService" ) };
        var entry = new CostBreakdownEntry(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
          new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), "entry", 0, 15, 12.34);
        var target = new CostBreakdown("theTitle", Unit.Eur, 25, new[] { entry });

        // Act
        var result = target.Apply(locationContext, values);

        // Assert
        Assert.That(result.Select(x => x.Value).ToList(), Is.EqualTo(new[] { new NormalizedDurationRegisterValue(
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new UnitValue(12.34*1.25, Unit.Eur), "EnergiDataService", "theTitle" )
        }));
        Assert.That(result.First().Entries, Is.Empty);
    }

    [Test]
    public void ApplyIncompatibleUnits()
    {
        // Arrange
        var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
        var values = new[] { new NormalizedDurationRegisterValue(
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 0, 0, DateTimeKind.Utc),
            new UnitValue(12.34, Unit.Eur), "EnergiDataService" ) };
        var entry = new CostBreakdownEntry(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
          new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), "entry", 3, 21, 12.34);
        var target = new CostBreakdown("theTitle", Unit.Dkk, 25, new[] { entry });

        // Act & Assert
        Assert.That(() => target.Apply(locationContext, values), Throws.TypeOf<DataMisalignedException>());
    }

    private static void AssertEntryGroup((DateTime FromDate, DateTime ToDate) expectedKey, CostBreakdownEntry[] expectedValue, IDictionary<(DateTime FromDate, DateTime ToDate), IReadOnlyList<CostBreakdownEntry>> actual)
    {
        Assert.That(actual.Keys, Contains.Item(expectedKey));
        Assert.That(actual[expectedKey], Is.EquivalentTo(expectedValue));
    }

}