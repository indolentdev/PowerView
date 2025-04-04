using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class TimeRegisterValueLabelSeriesSetTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new TimeRegisterValueLabelSeriesSet(DateTime.UtcNow, DateTime.UtcNow, null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new TimeRegisterValueLabelSeriesSet(DateTime.Now, DateTime.UtcNow, new TimeRegisterValueLabelSeries[0]), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new TimeRegisterValueLabelSeriesSet(DateTime.UtcNow, DateTime.Now, new TimeRegisterValueLabelSeries[0]), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            var start = DateTime.UtcNow - TimeSpan.FromDays(1);
            var end = DateTime.UtcNow;
            var labelSeries = new[] { new TimeRegisterValueLabelSeries("A", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()) };

            // Act
            var target = new TimeRegisterValueLabelSeriesSet(start, end, labelSeries);

            // Assert
            Assert.That(target.Start, Is.EqualTo(start));
            Assert.That(target.End, Is.EqualTo(end));
        }


        [Test]
        public void Enumerable()
        {
            // Arrange
            var labelSeries = new[] {
        new TimeRegisterValueLabelSeries("A", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()),
        new TimeRegisterValueLabelSeries("B", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>())
      };

            // Act
            var target = CreateTarget(DateTime.UtcNow, DateTime.UtcNow, labelSeries);

            // Assert
            Assert.That(target, Is.EqualTo(labelSeries));
        }

        [Test]
        public void NormalizeThrows()
        {
            // Arrange
            var labelSeries = new[] {
        new TimeRegisterValueLabelSeries("A", new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>>()),
      };
            var target = CreateTarget(DateTime.UtcNow, DateTime.UtcNow, labelSeries);

            // Act
            Assert.That(() => target.Normalize(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Normalize()
        {
            // Arrange
            const string label = "label";
            ObisCode obisCode = "1.2.3.4.5.6";
            var now = DateTime.UtcNow;
            var baseTime = new DateTime(now.Year, now.Month, now.Day, 18, 2, 12, DateTimeKind.Utc);
            var timeRegisterValues = new[]
            {
        new TimeRegisterValue("sn1", baseTime, 11, Unit.WattHour),
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(4), 12, Unit.WattHour),
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(11), 13, Unit.WattHour),
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(16), 14, Unit.WattHour),
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(19), 15, Unit.WattHour),
        new TimeRegisterValue("sn1", baseTime + TimeSpan.FromMinutes(25), 16, Unit.WattHour),
      };
            var labelSeries = new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });
            var target = new TimeRegisterValueLabelSeriesSet(baseTime, baseTime + TimeSpan.FromMinutes(30), new[] { labelSeries });
            var locationContext = TimeZoneHelper.GetDenmarkLocationContext();
            var timeDivider = new DateTimeHelper(locationContext, DateTime.Today.ToUniversalTime()).GetDivider("10-minutes");

            // Act
            var target2 = target.Normalize(timeDivider);

            // Assert
            Assert.That(target2.Start, Is.EqualTo(target.Start));
            Assert.That(target2.End, Is.EqualTo(target.End));
            Assert.That(target2.Count(), Is.EqualTo(target.Count()));
            Assert.That(target2.First()[obisCode], Is.EqualTo(new NormalizedTimeRegisterValue[]
            {
        timeRegisterValues[0].Normalize(timeDivider), timeRegisterValues[2].Normalize(timeDivider), timeRegisterValues[4].Normalize(timeDivider)
            }));
        }
        /*
            [Test]
            public void Add()
            {
              // Arrange
              var baseTime = new DateTime(2019, 7, 30, 18, 2, 12, DateTimeKind.Utc);
              var target = new TimeRegisterValueLabelSeriesSet(baseTime, baseTime + TimeSpan.FromMinutes(30), new TimeRegisterValueLabelSeries[0]);
              const string label = "label";
              ObisCode obisCode = "1.2.3.4.5.6";
              var timeRegisterValues = new[]
              {
                new TimeRegisterValue("sn1", baseTime, 11, Unit.WattHour)
              };
              var labelSeries = new TimeRegisterValueLabelSeries(label, new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { obisCode, timeRegisterValues } });

              // Act
              target.Add(new[] { labelSeries });

              // Assert
              Assert.That(target, Is.EqualTo(new[] { labelSeries }));
            }
        */
        private static TimeRegisterValueLabelSeriesSet CreateTarget(DateTime start, DateTime end, IList<TimeRegisterValueLabelSeries> labelSeries)
        {
            return new TimeRegisterValueLabelSeriesSet(start, end, labelSeries);
        }

    }
}

