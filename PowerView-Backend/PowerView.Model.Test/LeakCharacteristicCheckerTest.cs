using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class LeakCharacteristicCheckerTest
  {
    [Test]
    public void HasLeakCharacteristicThrows()
    {
      // Arrange
      var labelSeries = new LabelSeries<NormalizedDurationRegisterValue>("Lbl", new Dictionary<ObisCode, IEnumerable<NormalizedDurationRegisterValue>>());
      var obisCode = ObisCode.ColdWaterVolume1Delta;
      var dateTime = DateTime.UtcNow;
      var target = new LeakCharacteristicChecker(new NullLogger<LeakCharacteristicChecker>());

      // Act & Assert
      Assert.That(() => target.GetLeakCharacteristic(null, obisCode, dateTime, dateTime), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.GetLeakCharacteristic(labelSeries, obisCode.ToPeriod(), dateTime, dateTime), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.GetLeakCharacteristic(labelSeries, obisCode, DateTime.Now, dateTime), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.GetLeakCharacteristic(labelSeries, obisCode, dateTime, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetLeakCharacteristicPresent()
    {
      // Arrange
      ObisCode oc = ObisCode.ColdWaterVolume1Delta;
      var time = new DateTime(2016, 12, 29, 0, 29, 0, DateTimeKind.Utc);
      var values = Enumerable.Range(0, 13).Select(i =>
        new NormalizedDurationRegisterValue(time.AddMinutes(i * 30 - 30), time.AddMinutes(i * 30), time.AddMinutes(i * 30 - 30), time.AddMinutes(i * 30),
        new UnitValue(0.5, Unit.CubicMetre)));
      var normalizedDurationRegisterValues = new Dictionary<ObisCode, IEnumerable<NormalizedDurationRegisterValue>> { { oc, values } };
      var labelSeries = new LabelSeries<NormalizedDurationRegisterValue>("Label", normalizedDurationRegisterValues);
      var start = new DateTime(2016, 12, 29, 0, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2016, 12, 29, 6, 0, 0, DateTimeKind.Utc);
      var target = new LeakCharacteristicChecker(new NullLogger<LeakCharacteristicChecker>());

      // Act
      var leakCharacteristic = target.GetLeakCharacteristic(labelSeries, oc, start, end);

      // Assert
      Assert.That(leakCharacteristic, Is.EqualTo(new UnitValue(6, Unit.CubicMetre)));
    }

    [Test]
    public void GetLeakCharacteristicAbsent()
    {
      // Arrange
      ObisCode oc = ObisCode.ColdWaterVolume1Delta;
      var time = new DateTime(2016, 12, 29, 0, 29, 0, DateTimeKind.Utc);
      var values = Enumerable.Range(0, 13).Select(i =>
        new NormalizedDurationRegisterValue(time.AddMinutes(i * 30 - 30), time.AddMinutes(i * 30), time.AddMinutes(i * 30 - 30), time.AddMinutes(i * 30),
        new UnitValue(0, Unit.CubicMetre)));
      var normalizedDurationRegisterValues = new Dictionary<ObisCode, IEnumerable<NormalizedDurationRegisterValue>> { { oc, values } };
      var labelSeries = new LabelSeries<NormalizedDurationRegisterValue>("Label", normalizedDurationRegisterValues);
      var start = new DateTime(2016, 12, 29, 0, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2016, 12, 29, 6, 0, 0, DateTimeKind.Utc);
      var target = new LeakCharacteristicChecker(new NullLogger<LeakCharacteristicChecker>());

      // Act
      var leakCharacteristic = target.GetLeakCharacteristic(labelSeries, oc, start, end);

      // Assert
      Assert.That(leakCharacteristic, Is.EqualTo(new UnitValue(0, Unit.CubicMetre)));
    }

    [Test]
    public void GetLeakCharacteristicInsufficientReadings()
    {
      // Arrange
      ObisCode oc = ObisCode.ColdWaterVolume1Delta;
      var time = new DateTime(2016, 12, 29, 0, 29, 0, DateTimeKind.Utc);
      var values = Enumerable.Range(0, 8).Select(i =>
        new NormalizedDurationRegisterValue(time.AddMinutes(i * 30 - 30), time.AddMinutes(i * 30), time.AddMinutes(i * 30 - 30), time.AddMinutes(i * 30),
        new UnitValue(i * Math.Pow(10, -3), Unit.CubicMetre)));
      var normalizedDurationRegisterValues = new Dictionary<ObisCode, IEnumerable<NormalizedDurationRegisterValue>> { { oc, values } };
      var labelSeries = new LabelSeries<NormalizedDurationRegisterValue>("Label", normalizedDurationRegisterValues);
      var start = new DateTime(2016, 12, 29, 0, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2016, 12, 29, 6, 0, 0, DateTimeKind.Utc);
      var target = new LeakCharacteristicChecker(new NullLogger<LeakCharacteristicChecker>());

      // Act
      var leakCharacteristic = target.GetLeakCharacteristic(labelSeries, oc, start, end);

      // Assert
      Assert.That(leakCharacteristic, Is.Null);
    }

  }
}
