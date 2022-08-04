using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class ExportRepositoryTest : DbTestFixtureWithSchema
    {
        [Test]
        public void GetLiveCumulativeSeriesThrows()
        {
            // Arrange
            var dtLocal = DateTime.Now;
            var dtUtc = DateTime.UtcNow;
            var labelsEmpty = new string[0];
            var labels = new[] { "lbl1", "lbl2" };
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.GetLiveCumulativeSeries(dtLocal, dtUtc, labels), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => target.GetLiveCumulativeSeries(dtUtc, dtLocal, labels), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => target.GetLiveCumulativeSeries(dtUtc, dtUtc, null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.GetLiveCumulativeSeries(dtUtc, dtUtc, labelsEmpty), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetLiveCumulativeSeriesOutsideTimestampFilter()
        {
            // Arrange
            var label = "Thelabel";
            var timestamp = new DateTime(2015, 02, 12, 22, 0, 0, DateTimeKind.Utc);
            Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp },
              new Db.LiveRegister { ObisCode = ObisCode.ColdWaterVolume1, Value = 1 });
            var target = CreateTarget();

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(timestamp + TimeSpan.FromHours(1), timestamp + TimeSpan.FromHours(20), new[] { label });

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet, Is.Empty);
        }

        [Test]
        public void GetLiveCumulativeSeriesOutsideLabelsFilter()
        {
            // Arrange
            var label = "Thelabel";
            var timestamp = new DateTime(2015, 02, 12, 22, 0, 0, DateTimeKind.Utc);
            Insert(new Db.LiveReading { Label = "Other" + label, DeviceId = "1", Timestamp = timestamp },
              new Db.LiveRegister { ObisCode = ObisCode.ColdWaterVolume1, Value = 1 });
            var target = CreateTarget();

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(timestamp + TimeSpan.FromHours(-1), timestamp + TimeSpan.FromHours(1), new[] { label });

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet, Is.Empty);
        }

        [Test]
        public void GetLiveCumulativeSeriesOutsideObisCodesFilter()
        {
            // Arrange
            var label = "Thelabel";
            var timestamp = new DateTime(2015, 02, 12, 22, 0, 0, DateTimeKind.Utc);
            Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp },
              new Db.LiveRegister { ObisCode = ObisCode.ColdWaterFlow1, Value = 1 }); // non cumulative obis code
            var target = CreateTarget();

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(timestamp + TimeSpan.FromHours(-1), timestamp + TimeSpan.FromHours(1), new[] { label });

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet, Is.Empty);
        }

        [Test]
        public void GetLiveCumulativeSeriesForOneLabel()
        {
            // Arrange
            const string label = "TheLabel";
            var timestamp = new DateTime(2015, 02, 13, 22, 0, 0, DateTimeKind.Local).ToUniversalTime();
            Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActiveEnergyA14, Value = 1, Unit = (byte)Unit.WattHour },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActualPowerP14, Value = 11, Unit = (byte)Unit.Watt });
            Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp + TimeSpan.FromMinutes(5) },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActiveEnergyA14, Value = 2, Unit = (byte)Unit.WattHour },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActualPowerP14, Value = 22, Unit = (byte)Unit.Watt });
            Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp + TimeSpan.FromMinutes(10) },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActiveEnergyA14, Value = 3, Unit = (byte)Unit.WattHour });
            var target = CreateTarget();
            var start = timestamp;
            var end = start + TimeSpan.FromDays(1);

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(start, end, new[] { label });

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
            var labelProfile = labelSeriesSet.First();
            Assert.That(labelProfile.Label, Is.EqualTo(label));
            Assert.That(labelProfile, Is.EqualTo(new[] { ObisCode.ElectrActiveEnergyA14 }));
            Assert.That(labelProfile[ObisCode.ElectrActiveEnergyA14].Count(), Is.EqualTo(3));
        }

        [Test]
        public void GetLiveCumulativeSeriesForTwoLabels()
        {
            // Arrange
            var timestamp = new DateTime(2015, 02, 13, 22, 0, 0, DateTimeKind.Local).ToUniversalTime();
            const string label1 = "TheLabe1l";
            Insert(new Db.LiveReading { Label = label1, DeviceId = "1", Timestamp = timestamp },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActiveEnergyA14, Value = 1, Unit = (byte)Unit.WattHour });
            Insert(new Db.LiveReading { Label = label1, DeviceId = "1", Timestamp = timestamp + TimeSpan.FromMinutes(5) },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActiveEnergyA14, Value = 2, Unit = (byte)Unit.WattHour });
            const string label2 = "TheLabe12";
            Insert(new Db.LiveReading { Label = label2, DeviceId = "2", Timestamp = timestamp },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActiveEnergyA14, Value = 10, Unit = (byte)Unit.WattHour });
            Insert(new Db.LiveReading { Label = label2, DeviceId = "2", Timestamp = timestamp + TimeSpan.FromMinutes(5) },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActiveEnergyA14, Value = 20, Unit = (byte)Unit.WattHour });

            var target = CreateTarget();
            var start = timestamp;
            var end = start + TimeSpan.FromDays(1);

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(start, end, new [] { label1 , label2 } );

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Select(x => x.Label).ToList(), Is.EquivalentTo(new [] { label1, label2 }));
        }

        [Test]
        public void GetLiveCumulativeSeriesStartBoundary()
        {
            // Arrange
            const string label = "TheLabel";
            var timestamp = new DateTime(2015, 02, 13, 22, 0, 0, DateTimeKind.Local).ToUniversalTime();
            Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActiveEnergyA14, Value = 1, Unit = (byte)Unit.WattHour });
            var target = CreateTarget();
            var start = timestamp;
            var end = start + TimeSpan.FromDays(1);

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(start, end, new[] { label });

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetLiveCumulativeSeriesEndBoundary()
        {
            // Arrange
            const string label = "TheLabel";
            var timestamp = new DateTime(2015, 02, 13, 22, 0, 0, DateTimeKind.Local).ToUniversalTime();
            Insert(new Db.LiveReading { Label = label, DeviceId = "1", Timestamp = timestamp },
              new Db.LiveRegister { ObisCode = ObisCode.ElectrActiveEnergyA14, Value = 1, Unit = (byte)Unit.WattHour });
            var target = CreateTarget();
            var start = timestamp - TimeSpan.FromDays(1);
            var end = timestamp;

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(start, end, new[] { label });

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
        }

        private void Insert<TReading, TRegister>(TReading reading, params TRegister[] registers)
          where TReading : class, IDbReading
          where TRegister : class, IDbRegister
        {
            DbContext.InsertReadings(reading);
            foreach (var register in registers)
            {
                register.ReadingId = reading.Id;
            }
            DbContext.InsertRegisters(registers);
        }

        private ExportRepository CreateTarget()
        {
            return new ExportRepository(DbContext);
        }

    }
}
