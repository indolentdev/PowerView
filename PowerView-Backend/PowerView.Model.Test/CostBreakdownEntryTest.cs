
using System;
using System.Globalization;
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

    [Test]
    public void IntersectsWithThrows()
    {
        // Arrange
        var fromDate = DateTime.UtcNow;
        var toDate = fromDate.AddDays(20);
        const string name = "theName";
        const int startTime = 3;
        const int endTime = 21;
        const double amount = 1.12;
        var target = new CostBreakdownEntry(fromDate, toDate, name, startTime, endTime, amount);

        // Act & Assert
        Assert.That(() => target.IntersectsWith(DateTime.Now, toDate), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => target.IntersectsWith(fromDate, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase("2020-08-03T22:00:00Z", "2020-09-03T22:00:00Z", false)]
    [TestCase("2020-08-03T22:00:00Z", "2020-11-21T00:00:00Z", true)]
    [TestCase("2020-11-21T00:00:00Z", "2021-02-13T22:00:00Z", true)]
    [TestCase("2021-02-13T22:00:00Z", "2022-08-03T00:00:00Z", true)]
    [TestCase("2022-08-03T22:00:00Z", "2022-09-03T22:00:00Z", false)]
    public void IntersectsWith(string fromString, string toString, bool expectedResult)
    {
        // Arrange
        var fromDate = new DateTime(2020, 11, 20, 23, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2021, 2, 13, 23, 0, 0, DateTimeKind.Utc); ;
        const string name = "theName";
        const int startTime = 3;
        const int endTime = 21;
        const double amount = 1.12;
        var target = new CostBreakdownEntry(fromDate, toDate, name, startTime, endTime, amount);
        var from = DateTime.Parse(fromString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        var to = DateTime.Parse(toString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        // Act
        var result = target.IntersectsWith(from, to);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public void AppliesToDatesThrows()
    {
        // Arrange
        var fromDate = DateTime.UtcNow;
        var toDate = fromDate.AddDays(20);
        const string name = "theName";
        const int startTime = 3;
        const int endTime = 21;
        const double amount = 1.12;
        var target = new CostBreakdownEntry(fromDate, toDate, name, startTime, endTime, amount);

        // Act & Assert
        Assert.That(() => target.AppliesToDates(DateTime.Now, toDate), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => target.AppliesToDates(fromDate, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase("2020-08-03T22:00:00Z", "2020-09-03T22:00:00Z", false)]
    [TestCase("2020-08-03T22:00:00Z", "2020-11-21T00:00:00Z", false)]
    [TestCase("2020-11-21T00:00:00Z", "2021-02-13T22:00:00Z", true)]
    [TestCase("2021-02-13T22:00:00Z", "2022-08-03T00:00:00Z", false)]
    [TestCase("2022-08-03T22:00:00Z", "2022-09-03T22:00:00Z", false)]
    public void AppliesToDates(string fromString, string toString, bool expectedResult)
    {
        // Arrange
        var fromDate = new DateTime(2020, 11, 20, 23, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2021, 2, 13, 23, 0, 0, DateTimeKind.Utc); ;
        const string name = "theName";
        const int startTime = 3;
        const int endTime = 21;
        const double amount = 1.12;
        var target = new CostBreakdownEntry(fromDate, toDate, name, startTime, endTime, amount);
        var from = DateTime.Parse(fromString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        var to = DateTime.Parse(toString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        // Act
        var result = target.AppliesToDates(from, to);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    [TestCase("02:00:00", false)]
    [TestCase("02:59:59", false)]
    [TestCase("03:00:00", true)]
    [TestCase("21:00:00", true)]
    [TestCase("21:00:01", true)]
    [TestCase("21:59:59", true)]
    [TestCase("22:00:00", false)]
    public void AppliesToTime(string timeString, bool expectedResult)
    {
        // Arrange
        var fromDate = new DateTime(2020, 11, 20, 23, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2021, 2, 13, 23, 0, 0, DateTimeKind.Utc); ;
        const string name = "theName";
        const int startTime = 3;
        const int endTime = 21;
        const double amount = 1.12;
        var target = new CostBreakdownEntry(fromDate, toDate, name, startTime, endTime, amount);
        var time = TimeOnly.Parse(timeString, CultureInfo.InvariantCulture);

        // Act
        var result = target.AppliesToTime(time);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }


}