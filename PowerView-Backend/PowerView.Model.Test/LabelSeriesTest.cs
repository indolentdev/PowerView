using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class LabelSeriesTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            const string label = "label";
            var timeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { new TimeRegisterValue("d1", DateTime.UtcNow, new UnitValue()) } }
      };

            // Act & Assert
            Assert.That(() => new LabelSeries<TimeRegisterValue>(null, timeRegisterValues), Throws.ArgumentNullException);
            Assert.That(() => new LabelSeries<TimeRegisterValue>(string.Empty, timeRegisterValues), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new LabelSeries<TimeRegisterValue>(label, null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new LabelSeries<TimeRegisterValue>(label,
              new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { "1.2.3.4.5.6", null } }), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            const string label = "label";
            var timeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { new TimeRegisterValue("d1", DateTime.UtcNow, new UnitValue()) } }
      };

            // Act
            var target = new LabelSeries<TimeRegisterValue>(label, timeRegisterValues);

            // Assert
            Assert.That(target.Label, Is.EqualTo(label));
        }

        [Test]
        public void Enumerable()
        {
            // Arrange
            const string label = "label";
            var timeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { new TimeRegisterValue("d1", DateTime.UtcNow, new UnitValue()) } }
      };
            var target = new LabelSeries<TimeRegisterValue>(label, timeRegisterValues);

            // Act
            var obisCodes = target.ToList();

            // Assert
            Assert.That(obisCodes, Is.EqualTo(timeRegisterValues.Keys));
        }

        [Test]
        [TestCase("1.2.3.4.5.6", true)]
        [TestCase("6.5.4.3.2.1", false)]
        public void ContainsObisCode(string obisCode, bool expected)
        {
            // Arrange
            var dt = new DateTime(2015, 02, 13, 19, 30, 00, DateTimeKind.Utc);
            var sv1 = new TimeRegisterValue("1", dt, 21, 1, Unit.WattHour);
            var sv2 = new TimeRegisterValue("1", dt.AddMinutes(5), 22, 1, Unit.WattHour);
            var timeRegisterValues = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { "1.2.3.4.5.6", new [] { sv1, sv2 } }
      };
            var target = new LabelSeries<TimeRegisterValue>("label", timeRegisterValues);

            // Act
            var containsObisCode = target.ContainsObisCode(obisCode);

            // Assert
            Assert.That(containsObisCode, Is.EqualTo(expected));
        }

        [Test]
        public void IndexerOrderedByTimestamp()
        {
            // Arrange
            const string label = "label";
            ObisCode obisCode = "1.2.3.4.5.6";
            var timeRegisterValues = new[] { new TimeRegisterValue("sn1", DateTime.UtcNow + TimeSpan.FromHours(1), 11, Unit.WattHour), new TimeRegisterValue("sn1", DateTime.UtcNow, 11, Unit.WattHour) };
            var target = new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

            // Act
            var timeRegisterValuesIndexer = target[obisCode];

            // Assert
            Assert.That(timeRegisterValuesIndexer, Is.EqualTo(timeRegisterValues.Reverse().ToList()));
        }

        [Test]
        public void IndexerReadOnly()
        {
            // Arrange
            const string label = "label";
            ObisCode obisCode = "1.2.3.4.5.6";
            var timeRegisterValues = new[] { new TimeRegisterValue("sn1", DateTime.UtcNow, 11, Unit.WattHour) };
            var target = new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

            // Act & Assert
            var timeRegisterValuesIndexer = target[obisCode];

            // Assert
            Assert.That(timeRegisterValuesIndexer.IsReadOnly, Is.True);
        }

        [Test]
        public void IndexerUnknownObisCode()
        {
            // Arrange
            const string label = "label";
            ObisCode obisCode = "1.2.3.4.5.6";
            var target = new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>());

            // Act
            var timeRegisterValuesIndexer = target[obisCode];

            // Assert
            Assert.That(timeRegisterValuesIndexer, Is.Empty);
        }

        [Test]
        public void GetCumulativeSeries_Empty()
        {
            // Arrange
            const string label = "label";
            var target = new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>());

            // Act
            var cumulatives = target.GetCumulativeSeries();

            // Assert
            Assert.That(cumulatives, Is.Empty);
        }

        [Test]
        public void GetCumulativeSeries()
        {
            // Arrange
            const string label = "label";
            var dt = DateTime.UtcNow;
            var obisCode = ObisCode.ColdWaterVolume1;
            var target = new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { obisCode, new [] { new TimeRegisterValue("d1", dt, new UnitValue()) } },
        { ObisCode.ColdWaterFlow1, new [] { new TimeRegisterValue("d1", dt, new UnitValue()) } }
      });

            // Act

            // Assert
            var cumulatives = target.GetCumulativeSeries();

            // Assert
            Assert.That(cumulatives.Count, Is.EqualTo(1));
            Assert.That(cumulatives.ContainsKey(obisCode));
            Assert.That(cumulatives[obisCode], Is.EqualTo(new[] { new TimeRegisterValue("d1", dt, new UnitValue()) }));
        }

        [Test]
        public void GetNonCumulativeSeries_Empty()
        {
            // Arrange
            const string label = "label";
            var target = new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>());

            // Act
            var nonCumulatives = target.GetNonCumulativeSeries();

            // Assert
            Assert.That(nonCumulatives, Is.Empty);
        }

        [Test]
        public void GetNonCumulativeSeries()
        {
            // Arrange
            const string label = "label";
            var dt = DateTime.UtcNow;
            var obisCode = ObisCode.ColdWaterFlow1;
            var target = new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> {
        { obisCode, new [] { new TimeRegisterValue("d1", dt, new UnitValue()) } },
        { ObisCode.ColdWaterVolume1, new [] { new TimeRegisterValue("d1", dt, new UnitValue()) } }
      });

            // Act

            // Assert
            var cumulatives = target.GetNonCumulativeSeries();

            // Assert
            Assert.That(cumulatives.Count, Is.EqualTo(1));
            Assert.That(cumulatives.ContainsKey(obisCode));
            Assert.That(cumulatives[obisCode], Is.EqualTo(new[] { new TimeRegisterValue("d1", dt, new UnitValue()) }));
        }

        [Test]
        public void Add()
        {
            // Arrange
            const string label = "label";
            var target = new LabelSeries<TimeRegisterValue>(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>());

            ObisCode obisCode = "1.2.3.4.5.6";
            var baseTime = new DateTime(2019, 7, 30, 18, 2, 12, DateTimeKind.Utc);
            var timeRegisterValues = new[]
            {
        new TimeRegisterValue("sn1", baseTime, 11, Unit.WattHour)
      };
            var dict = new Dictionary<ObisCode, IList<TimeRegisterValue>> { { obisCode, timeRegisterValues } };

            // Act
            target.Add(dict);

            // Assert
            Assert.That(target, Is.EqualTo(new[] { obisCode }));
            Assert.That(target[obisCode], Is.EqualTo(timeRegisterValues));
        }


    }
}
