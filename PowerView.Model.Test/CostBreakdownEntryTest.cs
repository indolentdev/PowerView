
using System;
using NUnit.Framework;

namespace PowerView.Model.Test;

[TestFixture]
public class CostBreakDownEntryTest
{
    [Test]
    public void ConstructorThrows()
    {
        // Arrange
        var fromDate = DateTime.UtcNow;
        var toDate = fromDate.AddDays(20);
        const string name = "theName";
        const int startTime = 3;
        const int endTime = 21;
        const double amount = 1.12;

        // Act & Assert
        Assert.That(() => new CostBreakdownEntry(DateTime.Now, toDate, name, startTime, endTime, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdownEntry(fromDate, DateTime.Now, name, startTime, endTime, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdownEntry(toDate, fromDate, name, startTime, endTime, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdownEntry(fromDate, toDate, null, startTime, endTime, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdownEntry(fromDate, toDate, string.Empty, startTime, endTime, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdownEntry(fromDate, toDate, name, -1, endTime, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdownEntry(fromDate, toDate, name, 23, endTime, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdownEntry(fromDate, toDate, name, startTime, 0, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdownEntry(fromDate, toDate, name, startTime, 24, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new CostBreakdownEntry(fromDate, toDate, name, endTime, startTime, amount), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
        // Arrange
        var fromDate = DateTime.UtcNow;
        var toDate = fromDate.AddDays(20);
        const string name = "theName";
        const int startTime = 3;
        const int endTime = 21;
        const double amount = 1.12;

        // Act
        var target = new CostBreakdownEntry(fromDate, toDate, name, startTime, endTime, amount);

        // Assert
        Assert.That(target.FromDate, Is.EqualTo(fromDate));
        Assert.That(target.ToDate, Is.EqualTo(toDate));
        Assert.That(target.Name, Is.EqualTo(name));
        Assert.That(target.StartTime, Is.EqualTo(startTime));
        Assert.That(target.EndTime, Is.EqualTo(endTime));
        Assert.That(target.Amount, Is.EqualTo(amount));
    }

}