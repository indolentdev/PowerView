using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class DateTimeHelperTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var timeZoneInfo = GetTimeZoneInfo();
      var origin = new DateTime(2019, 09, 22, 23, 0, 0, DateTimeKind.Utc);

      // Act & Assert
      Assert.That(() => new DateTimeHelper(null, origin), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new DateTimeHelper(timeZoneInfo, DateTime.SpecifyKind(origin, DateTimeKind.Local)), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetPeriodEndThrows()
    {
      // Arrange
      var timeZoneInfo = GetTimeZoneInfo();
      var origin = new DateTime(2019, 09, 22, 22, 0, 0, DateTimeKind.Utc);
      var target = new DateTimeHelper(timeZoneInfo, origin);

      // Act & Assert
      Assert.That(() => target.GetPeriodEnd(null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.GetPeriodEnd("pp"), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase("day", "2018-12-31T23:00Z", "2019-01-01T23:00Z")]
    [TestCase("day", "2019-03-29T23:00Z", "2019-03-30T23:00Z")] // Before Norm -> DST
    [TestCase("day", "2019-03-30T23:00Z", "2019-03-31T22:00Z")] // Norm -> DST
    [TestCase("day", "2019-03-31T22:00Z", "2019-04-01T22:00Z")] // After Norm -> DST
    [TestCase("day", "2019-10-25T22:00Z", "2019-10-26T22:00Z")] // Before DST -> Norm
    [TestCase("day", "2019-10-26T22:00Z", "2019-10-27T23:00Z")] // DST -> Norm
    [TestCase("day", "2019-10-27T23:00Z", "2019-10-28T23:00Z")] // After DST -> Norm

    [TestCase("month", "2018-12-31T23:00Z", "2019-01-31T23:00Z")]
    [TestCase("month", "2019-01-31T23:00Z", "2019-02-28T23:00Z")] // Before Norm -> DST
    [TestCase("month", "2019-02-28T23:00Z", "2019-03-31T22:00Z")] // Norm -> DST
    [TestCase("month", "2019-03-31T22:00Z", "2019-04-30T22:00Z")] // After Norm -> DST
    [TestCase("month", "2019-08-31T22:00Z", "2019-09-30T22:00Z")] // Before DST -> Norm
    [TestCase("month", "2019-09-30T22:00Z", "2019-10-31T23:00Z")] // DST -> Norm
    [TestCase("month", "2019-10-31T23:00Z", "2019-11-30T23:00Z")] // After DST -> Norm

    [TestCase("year", "2018-12-31T23:00Z", "2019-12-31T23:00Z")]
    [TestCase("year", "2018-07-31T22:00Z", "2019-07-31T22:00Z")]
    public void GetPeriodEnd(string period, string originString, string endString)
    {
      // Arrange
      var timeZoneInfo = GetTimeZoneInfo();
      var origin = DateTime.Parse(originString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var end = DateTime.Parse(endString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var target = new DateTimeHelper(timeZoneInfo, origin);

      // Act
      var dateTime = target.GetPeriodEnd(period);

      // Assert
      Assert.That(dateTime, Is.EqualTo(end));
      Assert.That(dateTime.Kind, Is.EqualTo(end.Kind));
    }

    [Test]
    [TestCase(null, typeof(ArgumentNullException))]
    [TestCase("", typeof(ArgumentOutOfRangeException))]
    [TestCase("--", typeof(ArgumentOutOfRangeException))]
    [TestCase("0-minutes", typeof(ArgumentOutOfRangeException))]
    [TestCase("61-minutes", typeof(ArgumentOutOfRangeException))]
    [TestCase("1.234567-minutes", typeof(ArgumentOutOfRangeException))]
    [TestCase("2-days", typeof(ArgumentOutOfRangeException))]
    [TestCase("2-months", typeof(ArgumentOutOfRangeException))]
    [TestCase("them-minutes", typeof(FormatException))]
    [TestCase("them-days", typeof(FormatException))]
    [TestCase("them-months", typeof(FormatException))]
    public void GetNextThrows(string interval, Type exceptionType)
    {
      // Arrange
      var timeZoneInfo = GetTimeZoneInfo();
      var origin = new DateTime(2019, 9, 23, 22, 0, 0, DateTimeKind.Utc);
      var target = new DateTimeHelper(timeZoneInfo, origin);

      // Act & Assert
      Assert.That(() => target.GetNext(interval), Throws.TypeOf(exceptionType));
    }

    [Test]
    [TestCase("2-minutes", "2019-09-23T22:00Z", "2019-09-23T22:02Z")]
    [TestCase("2.5-minutes", "2019-09-23T22:00Z", "2019-09-23T22:02:30Z")]
    [TestCase("5-minutes", "2019-09-23T22:00Z", "2019-09-23T22:05Z")]
    [TestCase("10-minutes", "2019-09-23T22:00Z", "2019-09-23T22:10Z")]
    [TestCase("15-minutes", "2019-09-23T22:00Z", "2019-09-23T22:15Z")]
    [TestCase("30-minutes", "2019-09-23T22:00Z", "2019-09-23T22:30Z")]
    [TestCase("60-minutes", "2019-09-23T22:00Z", "2019-09-23T23:00Z")]
    [TestCase("1-days", "2019-03-29T23:00Z", "2019-03-30T23:00Z")] // Before Norm -> DST
    [TestCase("1-days", "2019-03-30T23:00Z", "2019-03-31T22:00Z")] // Norm -> DST
    [TestCase("1-days", "2019-03-31T22:00Z", "2019-04-01T22:00Z")] // After Norm -> DST
    [TestCase("1-days", "2019-10-25T22:00Z", "2019-10-26T22:00Z")] // Before DST -> Norm
    [TestCase("1-days", "2019-10-26T22:00Z", "2019-10-27T23:00Z")] // DST -> Norm
    [TestCase("1-days", "2019-10-27T23:00Z", "2019-10-28T23:00Z")] // After DST -> Norm

    [TestCase("1-months", "2019-01-31T23:00Z", "2019-02-28T23:00Z")] // Before Norm -> DST
    [TestCase("1-months", "2019-02-28T23:00Z", "2019-03-31T22:00Z")] // Norm -> DST
    [TestCase("1-months", "2019-03-31T22:00Z", "2019-04-30T22:00Z")] // After Norm -> DST
    [TestCase("1-months", "2019-08-31T22:00Z", "2019-09-30T22:00Z")] // Before DST -> Norm
    [TestCase("1-months", "2019-09-30T22:00Z", "2019-10-31T23:00Z")] // DST -> Norm 
    [TestCase("1-months", "2019-10-31T23:00Z", "2019-11-30T23:00Z")] // After DST -> Norm
    [TestCase("1-months", "2019-02-15T23:00Z", "2019-03-15T23:00Z")]
    public void GetNext(string interval, string inDateTimeString, string outDateTimeString)
    {
      // Arrange
      var timeZoneInfo = GetTimeZoneInfo();
      var inDateTime = DateTime.Parse(inDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var outDateTime = DateTime.Parse(outDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var target = new DateTimeHelper(timeZoneInfo, inDateTime);

      // Act
      var getNext = target.GetNext(interval);
      var next = getNext(inDateTime);

      // Assert
      Assert.That(next, Is.EqualTo(outDateTime));
      Assert.That(next.Kind, Is.EqualTo(outDateTime.Kind));
    }

    [Test]
    public void GetNextSuccessiveNormalToDst()
    {
      // Arrange
      var timeZoneinfo = TimeZoneInfo.FindSystemTimeZoneById("CET");
      var start = new DateTime(2020, 3, 28, 12, 40, 0, DateTimeKind.Utc);
      var interval = "60-minutes";
      var periodStart = start;
      var periodEnd = new DateTime(2020, 3, 29, 11, 40, 0, DateTimeKind.Utc);

      var target = new DateTimeHelper(timeZoneinfo, start);
      var getNext = target.GetNext(interval);

      var categories = new System.Collections.Generic.List<DateTime>();

      // Act
      var categoryTimestamp = periodStart;
      while (categoryTimestamp < periodEnd)
      {
        categories.Add(categoryTimestamp);
        categoryTimestamp = getNext(categoryTimestamp);
      }

      // Assert
      Assert.That(categories.Count, Is.EqualTo(23));
    }

    [Test]
    [TestCase(null, typeof(ArgumentNullException))]
    [TestCase("", typeof(ArgumentOutOfRangeException))]
    [TestCase("--", typeof(ArgumentOutOfRangeException))]
    [TestCase("0-minutes", typeof(ArgumentOutOfRangeException))]
    [TestCase("61-minutes", typeof(ArgumentOutOfRangeException))]
    [TestCase("1.234567-minutes", typeof(ArgumentOutOfRangeException))]
    [TestCase("2-days", typeof(ArgumentOutOfRangeException))]
    [TestCase("2-months", typeof(ArgumentOutOfRangeException))]
    [TestCase("them-minutes", typeof(FormatException))]
    [TestCase("them-days", typeof(FormatException))]
    [TestCase("them-months", typeof(FormatException))]
    public void GetDividerThrows(string interval, Type exceptionType)
    {
      // Arrange
      var timeZoneInfo = GetTimeZoneInfo();
      var start = new DateTime(2019, 9, 23, 22, 0, 0, DateTimeKind.Utc);
      var target = new DateTimeHelper(timeZoneInfo, start);

      // Act & Assert
      Assert.That(() => target.GetDivider(interval), Throws.TypeOf(exceptionType));
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
    [TestCase("2019-02-28T23:00:00.000Z", "60-minutes", "2019-03-31T00:01:00.000Z", "2019-03-31T00:00:00.000Z")] // Before Norm -> DST
    [TestCase("2019-02-28T23:00:00.000Z", "60-minutes", "2019-03-31T01:01:00.000Z", "2019-03-31T01:00:00.000Z")] // Norm -> DST
    [TestCase("2019-02-28T23:00:00.000Z", "60-minutes", "2019-03-31T02:01:00.000Z", "2019-03-31T02:00:00.000Z")] // Norm -> DST
    [TestCase("2019-02-28T23:00:00.000Z", "60-minutes", "2019-03-31T03:01:00.000Z", "2019-03-31T03:00:00.000Z")] // After Norm -> DST
    [TestCase("2019-06-30T22:00:00.000Z", "60-minutes", "2019-10-27T00:01:00.000Z", "2019-10-27T00:00:00.000Z")] // Before DST -> Norm
    [TestCase("2019-06-30T22:00:00.000Z", "60-minutes", "2019-10-27T01:01:00.000Z", "2019-10-27T01:00:00.000Z")] // DST -> Norm
    [TestCase("2019-06-30T22:00:00.000Z", "60-minutes", "2019-10-27T02:01:00.000Z", "2019-10-27T02:00:00.000Z")] // Before DST -> Norm

    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-02-28T22:55:03.000Z", "2019-02-27T23:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-02-28T23:00:01.000Z", "2019-02-28T23:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-03-01T22:59:59.000Z", "2019-02-28T23:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-03-02T22:59:59.000Z", "2019-03-01T23:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-03-03T22:59:59.000Z", "2019-03-02T23:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-06-30T21:55:03.000Z", "2019-06-29T22:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-06-30T22:00:01.000Z", "2019-06-30T22:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-07-01T21:59:59.000Z", "2019-06-30T22:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-07-02T21:59:59.000Z", "2019-07-01T22:00:00.000Z")]
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-03-30T23:01:00.000Z", "2019-03-30T23:00:00.000Z")] // Before Norm -> DST
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-03-31T23:01:00.000Z", "2019-03-31T22:00:00.000Z")] // Norm -> DST
    [TestCase("2019-02-28T23:00:00.000Z", "1-days", "2019-04-30T23:01:00.000Z", "2019-04-30T22:00:00.000Z")] // After Norm -> DST
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-10-26T22:01:00.000Z", "2019-10-26T22:00:00.000Z")] // Before DST -> Norm
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-10-27T22:01:00.000Z", "2019-10-27T23:00:00.000Z")] // DST -> Norm
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-10-28T22:01:00.000Z", "2019-10-28T23:00:00.000Z")] // After DST -> Norm
    [TestCase("2019-06-30T22:00:00.000Z", "1-days", "2019-10-28T23:01:00.000Z", "2019-10-28T23:00:00.000Z")] // After DST -> Norm

    [TestCase("2019-05-27T22:00:00.000Z", "1-months", "2019-06-25T02:37:00.000Z", "2019-05-27T22:00:00.000Z")]
    [TestCase("2019-05-27T22:00:00.000Z", "1-months", "2019-07-02T02:37:00.000Z", "2019-06-27T22:00:00.000Z")]
    [TestCase("2019-05-31T22:00:00.000Z", "1-months", "2019-07-02T02:37:00.000Z", "2019-06-30T22:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-months", "2019-08-02T02:37:00.000Z", "2019-07-31T22:00:00.000Z")]
    [TestCase("2019-06-30T22:00:00.000Z", "1-months", "2019-08-31T21:55:00.000Z", "2019-07-31T22:00:00.000Z")]
    [TestCase("2019-01-31T23:00:00.000Z", "1-months", "2019-02-28T23:01:00.000Z", "2019-02-28T23:00:00.000Z")] // Before Norm -> DST
    [TestCase("2019-02-28T23:00:00.000Z", "1-months", "2019-03-31T23:01:00.000Z", "2019-03-31T22:00:00.000Z")] // Norm -> DST
    [TestCase("2019-03-31T22:00:00.000Z", "1-months", "2019-04-30T22:01:00.000Z", "2019-04-30T22:00:00.000Z")] // After Norm -> DST
    [TestCase("2019-08-31T22:00:00.000Z", "1-months", "2019-09-30T22:01:00.000Z", "2019-09-30T22:00:00.000Z")] // Before DST -> Norm
    [TestCase("2019-09-30T22:00:00.000Z", "1-months", "2019-10-31T21:55:00.000Z", "2019-09-30T22:00:00.000Z")] // Before DST -> Norm
    [TestCase("2019-09-30T22:00:00.000Z", "1-months", "2019-10-31T22:55:00.000Z", "2019-09-30T22:00:00.000Z")] // Before DST -> Norm
    [TestCase("2019-09-30T22:00:00.000Z", "1-months", "2019-10-31T23:01:00.000Z", "2019-10-31T23:00:00.000Z")] // DST -> Norm
    [TestCase("2019-09-30T22:00:00.000Z", "1-months", "2019-11-30T22:55:00.000Z", "2019-10-31T23:00:00.000Z")] // After DST -> Norm
    [TestCase("2019-10-31T23:00:00.000Z", "1-months", "2019-11-30T23:01:00.000Z", "2019-11-30T23:00:00.000Z")] // After DST -> Norm
    public void GetDivider(string originString, string interval, string inDateTimeString, string outDateTimeString)
    {
      // Arrange
      var timeZoneInfo = GetTimeZoneInfo();
      var origin = DateTime.Parse(originString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var inDateTime = DateTime.Parse(inDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var outDateTime = DateTime.Parse(outDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      var target = new DateTimeHelper(timeZoneInfo, origin);
            // Act
      var divider = target.GetDivider(interval);
      var dateTime = divider(inDateTime);

      // Assert
      Assert.That(dateTime, Is.EqualTo(outDateTime));
      Assert.That(dateTime.Kind, Is.EqualTo(outDateTime.Kind));
    }


    private static TimeZoneInfo GetTimeZoneInfo()
    {
      return TimeZoneHelper.GetDenmarkTimeZoneInfo();
    }

  }
}
