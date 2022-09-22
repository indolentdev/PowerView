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
            byte coldWaterVolume1 = 1;
            DbContext.InsertObisCodes((coldWaterVolume1, ObisCode.ColdWaterVolume1));
            var timestamp = new DateTime(2015, 02, 12, 22, 0, 0, DateTimeKind.Utc);
            (var labels, var _) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.LiveRegister { ObisId = coldWaterVolume1, Value = 1 });
            var target = CreateTarget();

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(timestamp + TimeSpan.FromHours(1), timestamp + TimeSpan.FromHours(20), labels);

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
            byte coldWaterVolume1 = 1;
            DbContext.InsertObisCodes((coldWaterVolume1, ObisCode.ColdWaterVolume1));
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.LiveRegister { ObisId = coldWaterVolume1, Value = 1 });
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
            var timestamp = new DateTime(2015, 02, 12, 22, 0, 0, DateTimeKind.Utc);
            byte coldWaterFlow1 = 1;
            DbContext.InsertObisCodes((coldWaterFlow1, ObisCode.ColdWaterFlow1));
            (var labels, var _) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.LiveRegister { ObisId = coldWaterFlow1, Value = 1 }); // non cumulative obis code
            var target = CreateTarget();

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(timestamp + TimeSpan.FromHours(-1), timestamp + TimeSpan.FromHours(1), labels);

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet, Is.Empty);
        }

        [Test]
        public void GetLiveCumulativeSeriesForOneLabel()
        {
            // Arrange
            var timestamp = new DateTime(2015, 02, 13, 22, 0, 0, DateTimeKind.Local).ToUniversalTime();
            byte electrActiveEnergyA14 = 1;
            byte electrActualPowerP14 = 2;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14), (electrActualPowerP14, ObisCode.ElectrActualPowerP14));
            (var labels, var _) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 1, Unit = (byte)Unit.WattHour },
              new Db.LiveRegister { ObisId = electrActualPowerP14, Value = 11, Unit = (byte)Unit.Watt });
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp + TimeSpan.FromMinutes(5) },
              new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 2, Unit = (byte)Unit.WattHour },
              new Db.LiveRegister { ObisId = electrActualPowerP14, Value = 22, Unit = (byte)Unit.Watt });
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp + TimeSpan.FromMinutes(10) },
              new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 3, Unit = (byte)Unit.WattHour });
            var target = CreateTarget();
            var start = timestamp;
            var end = start + TimeSpan.FromDays(1);

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(start, end, labels);

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
            var labelProfile = labelSeriesSet.First();
            Assert.That(labelProfile.Label, Is.EqualTo(labels.First()));
            Assert.That(labelProfile, Is.EqualTo(new[] { ObisCode.ElectrActiveEnergyA14 }));
            Assert.That(labelProfile[ObisCode.ElectrActiveEnergyA14].Count(), Is.EqualTo(3));
        }

        [Test]
        public void GetLiveCumulativeSeriesForTwoLabels()
        {
            // Arrange
            var timestamp = new DateTime(2015, 02, 13, 22, 0, 0, DateTimeKind.Local).ToUniversalTime();
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            (var labels1, var _) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 1, Unit = (byte)Unit.WattHour });
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp + TimeSpan.FromMinutes(5) },
              new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 2, Unit = (byte)Unit.WattHour });
            (var labels2, var _) = DbContext.Insert(new Db.LiveReading { LabelId = 2, DeviceId = 20, Timestamp = timestamp },
              new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 10, Unit = (byte)Unit.WattHour });
            DbContext.Insert(new Db.LiveReading { LabelId = 2, DeviceId = 20, Timestamp = timestamp + TimeSpan.FromMinutes(5) },
              new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 20, Unit = (byte)Unit.WattHour });

            var target = CreateTarget();
            var start = timestamp;
            var end = start + TimeSpan.FromDays(1);

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(start, end, labels1.Concat(labels2).ToList());

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Select(x => x.Label).ToList(), Is.EquivalentTo(labels1.Concat(labels2).ToList()));
        }

        [Test]
        public void GetLiveCumulativeSeriesStartBoundary()
        {
            // Arrange
            var timestamp = new DateTime(2015, 02, 13, 22, 0, 0, DateTimeKind.Local).ToUniversalTime();
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            (var labels, var _) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 1, Unit = (byte)Unit.WattHour });
            var target = CreateTarget();
            var start = timestamp;
            var end = start + TimeSpan.FromDays(1);

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(start, end, labels);

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
            var timestamp = new DateTime(2015, 02, 13, 22, 0, 0, DateTimeKind.Local).ToUniversalTime();
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            (var labels, var _) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = timestamp },
              new Db.LiveRegister { ObisId = electrActiveEnergyA14, Value = 1, Unit = (byte)Unit.WattHour });
            var target = CreateTarget();
            var start = timestamp - TimeSpan.FromDays(1);
            var end = timestamp;

            // Act
            var labelSeriesSet = target.GetLiveCumulativeSeries(start, end, labels);

            // Assert
            Assert.That(labelSeriesSet, Is.Not.Null);
            Assert.That(labelSeriesSet.Start, Is.EqualTo(start));
            Assert.That(labelSeriesSet.End, Is.EqualTo(end));
            Assert.That(labelSeriesSet.Count(), Is.EqualTo(1));
        }

        private ExportRepository CreateTarget()
        {
            return new ExportRepository(DbContext);
        }

    }
}
