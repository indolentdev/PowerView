using System;
using System.Globalization;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class DateTimeResolutionDividerTest
  {
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
