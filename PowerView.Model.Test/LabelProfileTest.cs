using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class LabelProfileTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var utcNow = DateTime.UtcNow;
      var timeRegisterValue = new Dictionary<ObisCode, ICollection<TimeRegisterValue>>();

      // Act & Assert
      Assert.That(() => new LabelProfile(null, utcNow, timeRegisterValue), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new LabelProfile(string.Empty, utcNow, timeRegisterValue), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new LabelProfile("label", DateTime.Now, timeRegisterValue), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new LabelProfile("label", utcNow, null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      const string label = "label";
      var timeRegisterValues = new Dictionary<ObisCode, ICollection<TimeRegisterValue>>();

      // Act
      var target = CreateTarget(label, DateTime.UtcNow, timeRegisterValues);

      // Assert
      Assert.That(target.Label, Is.EqualTo(label));
    }

    [Test]
    public void ContainsObisCode()
    {
      // Arrange
      var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
      var sv1 = new TimeRegisterValue("1", dt, 21, 1, Unit.WattHour);
      var sv2 = new TimeRegisterValue("1", dt.AddMinutes(5), 22, 1, Unit.WattHour);
      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { sv1, sv2 } }
      };
      var target = CreateTarget("label", dt, values);

      // Act
      var containsObisCode = target.ContainsObisCode("1.2.3.4.5.6");

      // Assert
      Assert.That(containsObisCode, Is.True);
    }

    [Test]
    public void IndexOperator()
    {
      // Arrange
      var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
      var sv1 = new TimeRegisterValue("1", dt, 21, 1, Unit.WattHour);
      var sv2 = new TimeRegisterValue("1", dt.AddMinutes(5), 22, 1, Unit.WattHour);
      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { sv1, sv2 } }
      };
      var target = CreateTarget("label", dt, values);

      // Act
      var obisCodeSet = target["1.2.3.4.5.6"];

      // Assert
      Assert.That(obisCodeSet, Is.EqualTo(values["1.2.3.4.5.6"]));
    }

    [Test]
    public void SingleObisCodeMultipleUnitTypesNotSupported()
    {
      // Arrange
      var dt = new DateTime(2015,02,13,19,30,00,DateTimeKind.Utc);
      var sv1 = new TimeRegisterValue("1", dt, 21, 1, Unit.WattHour);
      var sv2 = new TimeRegisterValue("1", dt.AddMinutes(5), 22, 1, Unit.Watt);
      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { sv1, sv2 } }
      };

      // Act & Assert
      Assert.That(() => CreateTarget("label", dt, values), Throws.TypeOf<DataMisalignedException>());
    }

    [Test]
    public void SeriesNotCumulative()
    {
      // Arrange
      var unit = Unit.Watt;
      var dt = new DateTime(2015,02,13,00,00,00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 212, 1, unit);
      var sv3 = new TimeRegisterValue("1", dt.AddHours(3), 213, 1, unit);
      var sv4 = new TimeRegisterValue("1", dt.AddHours(4), 214, 1, unit);
      var sv5 = new TimeRegisterValue("1", dt.AddHours(5), 215, 1, unit);
      var sv6 = new TimeRegisterValue("1", dt.AddHours(6), 216, 1, unit);
      var sv7 = new TimeRegisterValue("1", dt.AddHours(7), 220, 1, unit);
      var sv8 = new TimeRegisterValue("1", dt.AddHours(8), 224, 1, unit);
      var sv9 = new TimeRegisterValue("1", dt.AddHours(9), 225, 1, unit);
      var sv10 = new TimeRegisterValue("1", dt.AddHours(10), 230, 1, unit);
      var sv11 = new TimeRegisterValue("1", dt.AddHours(11), 236, 1, unit);
      var sv12 = new TimeRegisterValue("1", dt.AddHours(12), 242, 1, unit);
      var sv13 = new TimeRegisterValue("1", dt.AddHours(13), 250, 1, unit);
      var sv14 = new TimeRegisterValue("1", dt.AddHours(14), 258, 1, unit);
      var sv15 = new TimeRegisterValue("1", dt.AddHours(15), 264, 1, unit);
      var sv16 = new TimeRegisterValue("1", dt.AddHours(16), 270, 1, unit);
      var sv17 = new TimeRegisterValue("1", dt.AddHours(17), 276, 1, unit);
      var sv18 = new TimeRegisterValue("1", dt.AddHours(18), 280, 1, unit);
      var sv19 = new TimeRegisterValue("1", dt.AddHours(19), 284, 1, unit);
      var sv20 = new TimeRegisterValue("1", dt.AddHours(20), 286, 1, unit);
      var sv21 = new TimeRegisterValue("1", dt.AddHours(21), 288, 1, unit);
      var sv22 = new TimeRegisterValue("1", dt.AddHours(22), 289, 1, unit);
      var sv23 = new TimeRegisterValue("1", dt.AddHours(23), 290, 1, unit);
      var sv24 = new TimeRegisterValue("1", dt.AddHours(24), 291, 1, unit);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9, 
            sv10, sv11, sv12, sv13, sv14, sv15, sv16, sv17, sv18, sv19,
            sv20, sv21, sv22, sv23, sv24 } }
      };

      // Act
      var target = CreateTarget("label", dt, values);

      // Assert
      Assert.That(target, Is.EqualTo(new [] { values.First().Key }));
      var profileValues = new [] {
        new TimeRegisterValue("1", dt, 2100, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 2110, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 2120, unit),
        new TimeRegisterValue("1", dt.AddHours(3), 2130, unit),
        new TimeRegisterValue("1", dt.AddHours(4), 2140, unit),
        new TimeRegisterValue("1", dt.AddHours(5), 2150, unit),
        new TimeRegisterValue("1", dt.AddHours(6), 2160, unit),
        new TimeRegisterValue("1", dt.AddHours(7), 2200, unit),
        new TimeRegisterValue("1", dt.AddHours(8), 2240, unit),
        new TimeRegisterValue("1", dt.AddHours(9), 2250, unit),
        new TimeRegisterValue("1", dt.AddHours(10), 2300, unit),
        new TimeRegisterValue("1", dt.AddHours(11), 2360, unit),
        new TimeRegisterValue("1", dt.AddHours(12), 2420, unit),
        new TimeRegisterValue("1", dt.AddHours(13), 2500, unit),
        new TimeRegisterValue("1", dt.AddHours(14), 2580, unit),
        new TimeRegisterValue("1", dt.AddHours(15), 2640, unit),
        new TimeRegisterValue("1", dt.AddHours(16), 2700, unit),
        new TimeRegisterValue("1", dt.AddHours(17), 2760, unit),
        new TimeRegisterValue("1", dt.AddHours(18), 2800, unit),
        new TimeRegisterValue("1", dt.AddHours(19), 2840, unit),
        new TimeRegisterValue("1", dt.AddHours(20), 2860, unit),
        new TimeRegisterValue("1", dt.AddHours(21), 2880, unit),
        new TimeRegisterValue("1", dt.AddHours(22), 2890, unit),
        new TimeRegisterValue("1", dt.AddHours(23), 2900, unit),
        new TimeRegisterValue("1", dt.AddHours(24), 2910, unit)
      };
      Assert.That(target[values.First().Key], Is.EqualTo(profileValues));
    }

    [Test]
    public void SeriesCumulaitveExpands()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015,02,13,00,00,00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 212, 1, unit);
      var sv3 = new TimeRegisterValue("1", dt.AddHours(3), 213, 1, unit);
      var sv4 = new TimeRegisterValue("1", dt.AddHours(4), 214, 1, unit);
      var sv5 = new TimeRegisterValue("1", dt.AddHours(5), 215, 1, unit);
      var sv6 = new TimeRegisterValue("1", dt.AddHours(6), 216, 1, unit);
      var sv7 = new TimeRegisterValue("1", dt.AddHours(7), 220, 1, unit);
      var sv8 = new TimeRegisterValue("1", dt.AddHours(8), 224, 1, unit);
      var sv9 = new TimeRegisterValue("1", dt.AddHours(9), 225, 1, unit);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.0.1.8.0.255", new [] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 } }
      };

      // Act
      var target = CreateTarget("label", dt, values);

      // Assert
      Assert.That(target, Is.EqualTo(new ObisCode[] { values.First().Key, "1.66.1.8.0.255", "1.65.1.8.0.255" }));
      var periodValues = new [] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 20, unit),
        new TimeRegisterValue("1", dt.AddHours(3), 30, unit),
        new TimeRegisterValue("1", dt.AddHours(4), 40, unit),
        new TimeRegisterValue("1", dt.AddHours(5), 50, unit),
        new TimeRegisterValue("1", dt.AddHours(6), 60, unit),
        new TimeRegisterValue("1", dt.AddHours(7), 100, unit),
        new TimeRegisterValue("1", dt.AddHours(8), 140, unit),
        new TimeRegisterValue("1", dt.AddHours(9), 150, unit),
      };
      Assert.That(target["1.66.1.8.0.255"], Is.EqualTo(periodValues));

      var deltaValues = new [] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(3), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(4), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(5), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(6), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(7), 40, unit),
        new TimeRegisterValue("1", dt.AddHours(8), 40, unit),
        new TimeRegisterValue("1", dt.AddHours(9), 10, unit),
      };
      Assert.That(target["1.65.1.8.0.255"], Is.EqualTo(deltaValues));
    }

    [Test]
    public void SeriesCumulaitveExpandsCrossingSerialNumbers()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015,02,13,00,00,00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 213, 1, unit);
      var sv3 = new TimeRegisterValue("2", dt.AddHours(3), 301, 1, unit);
      var sv4 = new TimeRegisterValue("2", dt.AddHours(4), 302, 1, unit);
      var sv5 = new TimeRegisterValue("2", dt.AddHours(5), 303, 1, unit);
      var sv6 = new TimeRegisterValue("2", dt.AddHours(6), 305, 1, unit);
      var sv7 = new TimeRegisterValue("3", dt.AddHours(7), 101, 1, unit);
      var sv8 = new TimeRegisterValue("3", dt.AddHours(8), 102, 1, unit);
      var sv9 = new TimeRegisterValue("3", dt.AddHours(9), 104, 1, unit);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.0.1.8.0.255", new [] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 } }
      };

      // Act
      var target = CreateTarget("label", dt, values);

      // Assert
      Assert.That(target, Is.EqualTo(new ObisCode[] { values.First().Key, "1.66.1.8.0.255", "1.65.1.8.0.255" }));
      var periodValues = new [] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 30, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(3), 30+0, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(4), 30+10, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(5), 30+20, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(6), 30+40, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(7), 30+40, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(8), 30+40+10, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(9), 30+40+30, unit)
      };
      Assert.That(target["1.66.1.8.0.255"], Is.EqualTo(periodValues));

      var deltaValues = new [] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 20, unit),
        new TimeRegisterValue("2", dt.AddHours(3), 0, unit),
        new TimeRegisterValue("2", dt.AddHours(4), 10, unit),
        new TimeRegisterValue("2", dt.AddHours(5), 10, unit),
        new TimeRegisterValue("2", dt.AddHours(6), 20, unit),
        new TimeRegisterValue("3", dt.AddHours(7), 0, unit),
        new TimeRegisterValue("3", dt.AddHours(8), 10, unit),
        new TimeRegisterValue("3", dt.AddHours(9), 20, unit)
      };
      Assert.That(target["1.65.1.8.0.255"], Is.EqualTo(deltaValues));
    }

    [Test]
    public void SeriesCumulaitveExpandsCrossingSerialNumbersAndBack()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 213, 1, unit);
      var sv3 = new TimeRegisterValue("2", dt.AddHours(3), 301, 1, unit);
      var sv4 = new TimeRegisterValue("2", dt.AddHours(4), 302, 1, unit);
      var sv5 = new TimeRegisterValue("2", dt.AddHours(5), 303, 1, unit);
      var sv6 = new TimeRegisterValue("2", dt.AddHours(6), 305, 1, unit);
      var sv7 = new TimeRegisterValue("1", dt.AddHours(7), 215, 1, unit);
      var sv8 = new TimeRegisterValue("1", dt.AddHours(8), 217, 1, unit);
      var sv9 = new TimeRegisterValue("1", dt.AddHours(9), 218, 1, unit);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.0.1.8.0.255", new [] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 } }
      };

      // Act
      var target = CreateTarget("label", dt, values);

      // Assert
      Assert.That(target, Is.EqualTo(new ObisCode[] { values.First().Key, "1.66.1.8.0.255", "1.65.1.8.0.255" }));
      var periodValues = new[] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 30, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(3), 30+0, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(4), 30+10, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(5), 30+20, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(6), 30+40, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(7), 30+40, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(8), 30+40+20, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(9), 30+40+30, unit)
      };
      Assert.That(target["1.66.1.8.0.255"], Is.EqualTo(periodValues));

      var deltaValues = new[] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 20, unit),
        new TimeRegisterValue("2", dt.AddHours(3), 0, unit),
        new TimeRegisterValue("2", dt.AddHours(4), 10, unit),
        new TimeRegisterValue("2", dt.AddHours(5), 10, unit),
        new TimeRegisterValue("2", dt.AddHours(6), 20, unit),
        new TimeRegisterValue("1", dt.AddHours(7), 0, unit),
        new TimeRegisterValue("1", dt.AddHours(8), 20, unit),
        new TimeRegisterValue("1", dt.AddHours(9), 10, unit)
      };
      Assert.That(target["1.65.1.8.0.255"], Is.EqualTo(deltaValues));
    }

    [Test]
    public void SeriesCumulaitveExpandsCrossingSerialNumberWithOneReadingAndBack()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 213, 1, unit);
      var sv3 = new TimeRegisterValue("2", dt.AddHours(3), 301, 1, unit);
      var sv4 = new TimeRegisterValue("1", dt.AddHours(7), 215, 1, unit);
      var sv5 = new TimeRegisterValue("1", dt.AddHours(8), 217, 1, unit);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.0.1.8.0.255", new [] { sv0, sv1, sv2, sv3, sv4, sv5 } }
      };

      // Act
      var target = CreateTarget("label", dt, values);

      // Assert
      Assert.That(target, Is.EqualTo(new ObisCode[] { values.First().Key, "1.66.1.8.0.255", "1.65.1.8.0.255" }));
      var periodValues = new[] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 30, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(3), 30+0, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(7), 30+0, unit),
        new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, dt.AddHours(8), 30+0+20, unit)
      };
      Assert.That(target["1.66.1.8.0.255"], Is.EqualTo(periodValues));

      var deltaValues = new[] {
        new TimeRegisterValue("1", dt, 0, unit),
        new TimeRegisterValue("1", dt.AddHours(1), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(2), 20, unit),
        new TimeRegisterValue("2", dt.AddHours(3), 0, unit),
        new TimeRegisterValue("1", dt.AddHours(7), 0, unit),
        new TimeRegisterValue("1", dt.AddHours(8), 20, unit)
      };
      Assert.That(target["1.65.1.8.0.255"], Is.EqualTo(deltaValues));
    }

    [Test]
    public void SeriesCumulativeFiltersBeforeStart()
    {
      // Arrange
      var unit = Unit.WattHour;
      var dt = new DateTime(2015,02,13,00,00,00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, unit);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, unit);
      var sv2 = new TimeRegisterValue("1", dt.AddHours(2), 212, 1, unit);
      var sv3 = new TimeRegisterValue("1", dt.AddHours(3), 213, 1, unit);
      var sv4 = new TimeRegisterValue("1", dt.AddHours(4), 215, 1, unit);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.0.1.8.0.255", new [] { sv0, sv1, sv2, sv3, sv4 } }
      };

      // Act
      var target = CreateTarget("label", dt.AddHours(2), values);

      // Assert
      Assert.That(target, Is.EqualTo(new ObisCode[] { values.First().Key, "1.66.1.8.0.255", "1.65.1.8.0.255" }));
      var periodValues = new [] {
        new TimeRegisterValue("1", dt.AddHours(2), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(3), 20, unit),
        new TimeRegisterValue("1", dt.AddHours(4), 40, unit),
      };
      Assert.That(target["1.66.1.8.0.255"], Is.EqualTo(periodValues));

      var deltaValues = new [] {
        new TimeRegisterValue("1", dt.AddHours(2), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(3), 10, unit),
        new TimeRegisterValue("1", dt.AddHours(4), 20, unit),
      };
      Assert.That(target["1.65.1.8.0.255"], Is.EqualTo(deltaValues));
    }

    [Test]
    public void SeriesCumulativeExpandsToEmptyIgnored()
    {
      // Arrange
      var wh = Unit.WattHour;
      var dt = new DateTime(2015,02,13,00,00,00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 210, 1, wh);
      var sv1 = new TimeRegisterValue("1", dt.AddHours(1), 211, 1, wh);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.0.1.8.0.255", new [] { sv0, sv1 } }
      };

      // Act
      var target = CreateTarget("label", dt.AddHours(2), values);

      // Assert
      Assert.That(target, Is.EqualTo(new ObisCode[] { values.First().Key }));
    }

    [Test]
    public void SeriesCumulaitveExpandsToAverageValues()
    {
      // Arrange
      var wh = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 960936, 1, wh);
      var sv1 = new TimeRegisterValue("1", dt.AddMinutes(5), 960944, 1, wh);
      var sv2 = new TimeRegisterValue("1", dt.AddMinutes(10), 960952, 1, wh);
      var sv3 = new TimeRegisterValue("1", dt.AddMinutes(15), 960960, 1, wh);
      var sv4 = new TimeRegisterValue("1", dt.AddMinutes(20), 960968, 1, wh);
      var sv5 = new TimeRegisterValue("1", dt.AddMinutes(25), 960977, 1, wh);
      var sv6 = new TimeRegisterValue("1", dt.AddMinutes(30), 960985, 1, wh);
      var sv7 = new TimeRegisterValue("1", dt.AddMinutes(35), 960992, 1, wh);
      var sv8 = new TimeRegisterValue("1", dt.AddMinutes(40), 961000, 1, wh);
      var sv9 = new TimeRegisterValue("1", dt.AddMinutes(45), 961007, 1, wh);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.0.1.8.0.255", new [] { sv0, sv1, sv2, sv3, sv4, sv5, sv6, sv7, sv8, sv9 } }
      };

      // Act
      var target = CreateTarget("label", dt, values);

      // Assert
      Assert.That(target, Contains.Item((ObisCode)"1.67.1.7.0.255"));
      var w = Unit.Watt;
      var averageValues = new[] {
        new TimeRegisterValue("1", dt.AddMinutes(5), 960, w),
        new TimeRegisterValue("1", dt.AddMinutes(10), 960, w),
        new TimeRegisterValue("1", dt.AddMinutes(15), 960, w),
        new TimeRegisterValue("1", dt.AddMinutes(20), 960, w),
        new TimeRegisterValue("1", dt.AddMinutes(25), 1080, w),
        new TimeRegisterValue("1", dt.AddMinutes(30), 960, w),
        new TimeRegisterValue("1", dt.AddMinutes(35), 840, w),
        new TimeRegisterValue("1", dt.AddMinutes(40), 960, w),
        new TimeRegisterValue("1", dt.AddMinutes(45), 840, w),
      };
      Assert.That(target["1.67.1.7.0.255"], Is.EqualTo(averageValues));
    }

    [Test]
    public void SeriesCumulaitveExpandsToAverageValuesExcludesSerialNumberChange()
    {
      // Arrange
      var wh = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 960936, 1, wh);
      var sv1 = new TimeRegisterValue("1", dt.AddMinutes(5), 960944, 1, wh);
      var sv2 = new TimeRegisterValue("2", dt.AddMinutes(10), 960952, 1, wh);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.0.1.8.0.255", new [] { sv0, sv1, sv2 } }
      };

      // Act
      var target = CreateTarget("label", dt, values);

      // Assert
      Assert.That(target, Contains.Item((ObisCode)"1.67.1.7.0.255"));
      var w = Unit.Watt;
      var averageValues = new[] {
        new TimeRegisterValue("1", dt.AddMinutes(5), 960, w)
      };
      Assert.That(target["1.67.1.7.0.255"], Is.EqualTo(averageValues));
    }

    [Test]
    [TestCase(61)] // Max 6 min interval
    [TestCase(181)] // Min 2 min interval
    public void SeriesCumulaitveExpandsToAverageValuesExcludesMinAndMaxReadingInterval(int timestampAdjustmentSeconds)
    {
      // Arrange
      var wh = Unit.WattHour;
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 960936, 1, wh);
      var sv1 = new TimeRegisterValue("1", dt.AddMinutes(5), 960944, 1, wh);
      var sv2 = new TimeRegisterValue("1", dt.AddMinutes(10).AddSeconds(timestampAdjustmentSeconds), 960952, 1, wh);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { "1.0.1.8.0.255", new [] { sv0, sv1, sv2 } }
      };

      // Act
      var target = CreateTarget("label", dt, values);

      // Assert
      Assert.That(target, Contains.Item((ObisCode)"1.67.1.7.0.255"));
      var w = Unit.Watt;
      var averageValues = new[] {
        new TimeRegisterValue("1", dt.AddMinutes(5), 960, w)
      };
      Assert.That(target["1.67.1.7.0.255"], Is.EqualTo(averageValues));
    }

    [Test]
    [TestCase("1.0.1.8.0.255", Unit.WattHour, "1.67.1.7.0.255", Unit.Watt)]
    [TestCase("1.0.2.8.0.255", Unit.WattHour, "1.67.2.7.0.255", Unit.Watt)]
    [TestCase("6.0.1.0.0.255", Unit.WattHour, "6.67.8.0.0.255", Unit.Watt)]
    [TestCase("6.0.2.0.0.255", Unit.CubicMetre, "6.67.9.0.0.255", Unit.CubicMetrePrHour)]
    [TestCase("8.0.1.0.0.255", Unit.CubicMetre, "8.67.2.0.0.255", Unit.CubicMetrePrHour)]
    public void SeriesCumulaitveExpandsToAverageValuesObisCodesAndUnits(string cumulativeObisCode, Unit cumulativeUnit, string actualObisCode, Unit actualUnit)
    {
      // Arrange
      var dt = new DateTime(2015, 02, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime();
      var sv0 = new TimeRegisterValue("1", dt, 960936, 1, cumulativeUnit);
      var sv1 = new TimeRegisterValue("1", dt.AddMinutes(5), 960944, 1, cumulativeUnit);

      var values = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> {
        { cumulativeObisCode, new [] { sv0, sv1 } }
      };

      // Act
      var target = CreateTarget("label", dt, values);

      // Assert
      Assert.That(target, Contains.Item((ObisCode)actualObisCode));
      var averageValues = new[] {
        new TimeRegisterValue("1", dt.AddMinutes(5), 960, actualUnit),
      };
      Assert.That(target[actualObisCode], Is.EqualTo(averageValues));
    }

    [Test]
    public void GetAllObisCodes()
    {
      // Arrange
      ObisCode oc1 = "1.2.3.4.5.6";
      ObisCode oc2 = "6.5.4.3.2.1";
      var timeRegisterValues = new Dictionary<ObisCode, ICollection<TimeRegisterValue>>
      {
        { oc1, new [] { new TimeRegisterValue() } },
        { oc2, new [] { new TimeRegisterValue() } }
      };
      var target = CreateTarget("Label", DateTime.UtcNow, timeRegisterValues);

      // Ac
      var obisCodes = target.GetAllObisCodes();

      // Assert
      Assert.That(obisCodes, Is.EqualTo(new [] {oc1, oc2}));
    }

    [Test]
    public void HasLeakCharacteristicThrows()
    {
      // Arrange
      var target = new LabelProfile("Lbl", DateTime.UtcNow, new Dictionary<ObisCode, ICollection<TimeRegisterValue>>());
      var obisCode = ObisCode.ColdWaterVolume1Delta;
      var dateTime = DateTime.UtcNow;

      // Act & Assert
      Assert.That(() => target.GetLeakCharacteristic(obisCode.ToPeriod(), dateTime, dateTime), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.GetLeakCharacteristic(obisCode, DateTime.Now, dateTime), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.GetLeakCharacteristic(obisCode, dateTime, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetLeakCharacteristicPresent()
    {
      // Arrange
      ObisCode oc = ObisCode.ColdWaterVolume1Delta;
      var time = new DateTime(2016, 12, 29, 0, 29, 0, DateTimeKind.Utc);
      var values = Enumerable.Range(0, 13).Select(i => new TimeRegisterValue("1", time.AddMinutes(i*30), 0.5, Unit.CubicMetre));
      var timeRegisterValues = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> { {oc, values.ToArray() } };
      var target = new LabelProfile("Label", time, timeRegisterValues);
      var start = new DateTime(2016, 12, 29, 0, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2016, 12, 29, 6, 0, 0, DateTimeKind.Utc);

      // Act
      var leakCharacteristic = target.GetLeakCharacteristic(oc, start, end);
      
      // Assert
      Assert.That(leakCharacteristic, Is.EqualTo(new UnitValue(6, Unit.CubicMetre)));
    }

    [Test]
    public void GetLeakCharacteristicAbsent()
    {
      // Arrange
      ObisCode oc = ObisCode.ColdWaterVolume1Delta;
      var time = new DateTime(2016, 12, 29, 0, 29, 0, DateTimeKind.Utc);
      var values = Enumerable.Range(0, 13).Select(i => new TimeRegisterValue("1", time.AddMinutes(i*30), 0, Unit.CubicMetre));
      var timeRegisterValues = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> { {oc, values.ToArray() } };
      var target = new LabelProfile("Label", time, timeRegisterValues);
      var start = new DateTime(2016, 12, 29, 0, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2016, 12, 29, 6, 0, 0, DateTimeKind.Utc);

      // Act
      var leakCharacteristic = target.GetLeakCharacteristic(oc, start, end);

      // Assert
      Assert.That(leakCharacteristic, Is.EqualTo(new UnitValue(0, Unit.CubicMetre)));
    }

    [Test]
    public void GetLeakCharacteristicInsufficientReadings()
    {
      // Arrange
      ObisCode oc = ObisCode.ColdWaterVolume1Delta;
      var time = new DateTime(2016, 12, 29, 0, 29, 0, DateTimeKind.Utc);
      var values = Enumerable.Range(0, 8).Select(i => new TimeRegisterValue("1", time.AddMinutes(i*30), i*Math.Pow(10, -3), Unit.CubicMetre));
      var timeRegisterValues = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> { {oc, values.ToArray() } };
      var target = new LabelProfile("Label", time, timeRegisterValues);
      var start = new DateTime(2016, 12, 29, 0, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2016, 12, 29, 6, 0, 0, DateTimeKind.Utc);

      // Act
      var leakCharacteristic = target.GetLeakCharacteristic(oc, start, end);

      // Assert
      Assert.That(leakCharacteristic, Is.Null);
    }

    private static LabelProfile CreateTarget(string label, DateTime start, IDictionary<ObisCode, ICollection<TimeRegisterValue>> timeRegisterValues)
    {
      return new LabelProfile(label, start, timeRegisterValues);
    }

  }
}

