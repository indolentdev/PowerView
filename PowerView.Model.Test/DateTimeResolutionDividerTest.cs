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
    [TestCase("2-minutes", true, "2017-12-30T02:32:16.123Z", "2017-12-30T02:34:16.123Z")]
    [TestCase("2-minutes", false, "2017-12-30T02:32:16.123Z", "2017-12-30T02:30:16.123Z")]
    [TestCase("2.5-minutes", true, "2017-12-30T02:32:16.123Z", "2017-12-30T02:34:46.123Z")]
    [TestCase("2.5-minutes", false, "2017-12-30T02:32:16.123Z", "2017-12-30T02:29:46.123Z")]
    [TestCase("5-minutes", true, "2017-12-30T02:34:56.123Z", "2017-12-30T02:39:56.123Z")]
    [TestCase("5-minutes", false, "2017-12-30T02:34:56.123Z", "2017-12-30T02:29:56.123Z")]
    [TestCase("10-minutes", true, "2017-12-30T02:37:56.123Z", "2017-12-30T02:47:56.123Z")]
    [TestCase("10-minutes", false, "2017-12-30T02:37:56.123Z", "2017-12-30T02:27:56.123Z")]
    [TestCase("15-minutes", true, "2017-12-30T02:37:56.123Z", "2017-12-30T02:52:56.123Z")]
    [TestCase("15-minutes", false, "2017-12-30T02:37:56.123Z", "2017-12-30T02:22:56.123Z")]
    [TestCase("30-minutes", true, "2017-12-30T02:37:56.123Z", "2017-12-30T03:07:56.123Z")]
    [TestCase("30-minutes", false, "2017-12-30T02:37:56.123Z", "2017-12-30T02:07:56.123Z")]
    [TestCase("60-minutes", true, "2017-12-30T02:37:56.123Z", "2017-12-30T03:37:56.123Z")]
    [TestCase("60-minutes", false, "2017-12-30T02:37:56.123Z", "2017-12-30T01:37:56.123Z")]
    [TestCase("1-days", true, "2017-12-30T02:37:56.123Z", "2017-12-31T02:37:56.123Z")]
    [TestCase("1-days", false, "2017-12-30T02:37:56.123Z", "2017-12-29T02:37:56.123Z")]
    [TestCase("1-months", true, "2017-12-30T02:37:56.123Z", "2018-01-30T02:37:56.123Z")]
    [TestCase("1-months", false, "2017-12-30T02:37:56.123Z", "2017-11-30T02:37:56.123Z")]
    public void GetNext(string interval, bool forward, string inDateTimeString, string outDateTimeString)
    {
      // Arrange
      var inDateTime = DateTime.Parse(inDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var outDateTime = DateTime.Parse(outDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var target = DateTimeResolutionDivider.GetNext(interval, forward);

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
    public void ResolutionDivider(string dividerId, string inDateTimeString, string outDateTimeString)
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
