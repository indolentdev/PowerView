using System;
using System.Globalization;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class DateTimeResolutionDividerTest
  {
    [Test]
    public void GetPeriodEndThrows()
    {
      // Arrange
      var period = "day";
      var start = DateTime.UtcNow;

      // Act & Assert
      Assert.That(() => DateTimeResolutionDivider.GetPeriodEnd(null, start), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => DateTimeResolutionDivider.GetPeriodEnd("bogus", start), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetPeriodEnd(period, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase("day", "2017-12-30T02:32:16.123Z", "2017-12-31T02:32:16.123Z")]
    [TestCase("month", "2017-12-30T02:32:16.123Z", "2018-01-30T02:32:16.123Z")]
    [TestCase("month", "2017-12-31T02:32:16.123Z", "2018-01-31T02:32:16.123Z")]
    [TestCase("month", "2019-06-30T22:00:00.000Z", "2019-07-31T22:00:00.000Z")]
    [TestCase("year", "2017-12-30T02:32:16.123Z", "2018-12-30T02:32:16.123Z")]
    public void GetPeriodEnd(string period, string startString, string endString)
    {
      // Arrange
      var start = DateTime.Parse(startString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var end = DateTime.Parse(endString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

      // Act
      var dateTime = DateTimeResolutionDivider.GetPeriodEnd(period, start);

      // Assert
      Assert.That(dateTime, Is.EqualTo(end));
    }

    [Test]
    public void GetNextThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => DateTimeResolutionDivider.GetNext(null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => DateTimeResolutionDivider.GetNext("minutes"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetNext("minutes-2-2"), Throws.TypeOf<ArgumentOutOfRangeException>());

      Assert.That(() => DateTimeResolutionDivider.GetNext("minutes-61"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetNext("minutes-0"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetNext("minutes-5.123"), Throws.TypeOf<ArgumentOutOfRangeException>());

      Assert.That(() => DateTimeResolutionDivider.GetNext("days-0"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetNext("days-2"), Throws.TypeOf<ArgumentOutOfRangeException>());

      Assert.That(() => DateTimeResolutionDivider.GetNext("months-0"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetNext("months-2"), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase("2-minutes", "2017-12-30T02:32:16.123Z", "2017-12-30T02:34:16.123Z")]
    [TestCase("2.5-minutes", "2017-12-30T02:32:16.123Z", "2017-12-30T02:34:46.123Z")]
    [TestCase("5-minutes", "2017-12-30T02:34:56.123Z", "2017-12-30T02:39:56.123Z")]
    [TestCase("10-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T02:47:56.123Z")]
    [TestCase("15-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T02:52:56.123Z")]
    [TestCase("30-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T03:07:56.123Z")]
    [TestCase("60-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T03:37:56.123Z")]
    [TestCase("1-days", "2017-12-30T02:37:56.123Z", "2017-12-31T02:37:56.123Z")]
    [TestCase("1-months", "2017-12-30T02:37:56.123Z", "2018-01-30T02:37:56.123Z")]
    [TestCase("1-months", "2017-10-31T02:37:56.123Z", "2017-11-30T02:37:56.123Z")]
    [TestCase("1-months", "2017-11-30T02:37:56.123Z", "2017-12-31T02:37:56.123Z")]
    public void GetNext(string interval, string inDateTimeString, string outDateTimeString)
    {
      // Arrange
      var inDateTime = DateTime.Parse(inDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var outDateTime = DateTime.Parse(outDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var target = DateTimeResolutionDivider.GetNext(interval);

      // Act
      var dateTime = target(inDateTime);

      // Assert
      Assert.That(dateTime, Is.EqualTo(outDateTime));
      Assert.That(dateTime.Kind, Is.EqualTo(outDateTime.Kind));
    }

    [Test]
    public void GetResolutionDividerThrows()
    {
      // Arrange
      var origin = DateTime.UtcNow;
      const string interval = "5-minutes";

      // Act & Assert
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(DateTime.Now, interval), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(origin, null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(origin, string.Empty), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(origin, "--"), Throws.TypeOf<ArgumentOutOfRangeException>());

      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(origin, "0-minutes"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(origin, "61-minutes"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(origin, "1.234567-minutes"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(origin, "2-days"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(origin, "2-months"), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase("2017-11-30T23:00:00.000Z", "2-minutes", "2017-12-30T02:32:16.123Z", "2017-12-30T02:32:00.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "2-minutes", "2017-12-30T02:37:16.123Z", "2017-12-30T02:36:00.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "2.5-minutes", "2017-12-30T02:32:16.123Z", "2017-12-30T02:30:00.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "2.5-minutes", "2017-12-30T02:32:36.123Z", "2017-12-30T02:32:30.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "2.5-minutes", "2017-12-30T02:37:16.123Z", "2017-12-30T02:35:00.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "2.5-minutes", "2017-12-30T02:37:36.123Z", "2017-12-30T02:37:30.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "5-minutes", "2017-12-30T02:34:56.123Z", "2017-12-30T02:30:00.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "10-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T02:30:00.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "15-minutes", "2017-12-30T02:47:56.123Z", "2017-12-30T02:45:00.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "30-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T02:30:00.000Z")]
    [TestCase("2017-11-30T23:00:00.000Z", "60-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T02:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-02-28T22:55:03.000Z", "2019-02-27T23:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-02-28T23:00:01.000Z", "2019-02-28T23:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-03-01T22:59:59.000Z", "2019-02-28T23:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-03-02T22:59:59.000Z", "2019-03-01T23:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-03-03T22:59:59.000Z", "2019-03-02T23:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-06-30T21:55:03.000Z", "2019-06-29T22:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-06-30T22:00:01.000Z", "2019-06-30T22:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-07-01T21:59:59.000Z", "2019-06-30T22:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-07-02T21:59:59.000Z", "2019-07-01T22:00:00.000Z")]
    [TestCase("2019-06-01T00:00:00.000Z", "1-months", "2019-12-30T02:37:56.123Z", "2019-12-01T00:00:00.000Z")]
    [TestCase("2019-06-30T23:00:00.000Z", "1-months", "2019-12-31T02:37:56.123Z", "2019-11-30T23:00:00.000Z")]
    [TestCase("2019-06-30T23:00:00.000Z", "1-months", "2019-01-31T02:37:56.123Z", "2018-12-31T23:00:00.000Z")]
    [TestCase("2019-06-30T23:00:00.000Z", "1-months", "2019-06-30T22:55:56.123Z", "2019-05-31T23:00:00.000Z")]
    public void ResolutionDivider(string originDateTimeString, string dividerId, string inDateTimeString, string outDateTimeString)
    {
      // Arrange
      var originDateTime = DateTime.Parse(originDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var inDateTime = DateTime.Parse(inDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var outDateTime = DateTime.Parse(outDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var target = DateTimeResolutionDivider.GetResolutionDivider(originDateTime, dividerId);

      // Act
      var dateTime = target(inDateTime);

      // Assert
      Assert.That(dateTime, Is.EqualTo(outDateTime));
      Assert.That(dateTime.Kind, Is.EqualTo(outDateTime.Kind));
    }

    [Test]
    public void GetResolutionDividerThrowsold()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider(string.Empty), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider("--"), Throws.TypeOf<ArgumentOutOfRangeException>());

      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider("0-minutes"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider("61-minutes"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider("1.234567-minutes"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider("2-days"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => DateTimeResolutionDivider.GetResolutionDivider("2-months"), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase("2-minutes", "2017-12-30T02:32:16.123Z", "2017-12-30T02:32:00.000Z")]
    [TestCase("2-minutes", "2017-12-30T02:37:16.123Z", "2017-12-30T02:36:00.000Z")]
    [TestCase("2.5-minutes", "2017-12-30T02:32:16.123Z", "2017-12-30T02:30:00.000Z")]
    [TestCase("2.5-minutes", "2017-12-30T02:32:36.123Z", "2017-12-30T02:32:30.000Z")]
    [TestCase("2.5-minutes", "2017-12-30T02:37:16.123Z", "2017-12-30T02:35:00.000Z")]
    [TestCase("2.5-minutes", "2017-12-30T02:37:36.123Z", "2017-12-30T02:37:30.000Z")]
    [TestCase("5-minutes", "2017-12-30T02:34:56.123Z", "2017-12-30T02:30:00.000Z")]
    [TestCase("10-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T02:30:00.000Z")]
    [TestCase("15-minutes", "2017-12-30T02:47:56.123Z", "2017-12-30T02:45:00.000Z")]
    [TestCase("30-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T02:30:00.000Z")]
    [TestCase("60-minutes", "2017-12-30T02:37:56.123Z", "2017-12-30T02:00:00.000Z")]
    [TestCase("1-days", "2017-12-30T02:37:56.123Z", "2017-12-30T12:00:00.000Z")]
    [TestCase("1-months", "2017-12-30T02:37:56.123Z", "2017-12-01T12:00:00.000Z")]
    public void ResolutionDividerold(string dividerId, string inDateTimeString, string outDateTimeString)
    {
      // Arrange
      var inDateTime = DateTime.Parse(inDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var outDateTime = DateTime.Parse(outDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var target = DateTimeResolutionDivider.GetResolutionDivider(dividerId);

      // Act
      var dateTime = target(inDateTime);

      // Assert
      Assert.That(dateTime, Is.EqualTo(outDateTime));
      Assert.That(dateTime.Kind, Is.EqualTo(outDateTime.Kind));
    }
  }
}
