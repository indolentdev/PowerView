using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class GaugeRepositoryTest : DbTestFixtureWithSchema
    {
        [Test]
        public void GetThrows()
        {
            // Arrange
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.GetLatest(DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetLatest()
        {
            // Arrange
            var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            var target = CreateTarget();
            var rd1 = new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            var rd2 = new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            var rd3 = new Db.MonthReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            var rd4 = new Db.YearReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            DbContext.InsertReadings(rd1, rd2, rd3, rd4);
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            var rg1 = new Db.LiveRegister { ReadingId = rd1.Id, ObisId = electrActiveEnergyA14, Value = 1, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg2 = new Db.DayRegister { ReadingId = rd2.Id, ObisId = electrActiveEnergyA14, Value = 2, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg3 = new Db.MonthRegister { ReadingId = rd3.Id, ObisId = electrActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg4 = new Db.YearRegister { ReadingId = rd4.Id, ObisId = electrActiveEnergyA14, Value = 4, Scale = 1, Unit = (byte)Unit.WattHour };
            DbContext.InsertRegisters(rg1, rg2, rg3, rg4);

            // Act
            var values = target.GetLatest(dt);

            // Assert
            Assert.That(values.Count, Is.EqualTo(4));
            Assert.That(values.First().Name, Is.EqualTo(GaugeSetName.Latest));
            Assert.That(values.First().GaugeValues.Count, Is.EqualTo(1));
            Assert.That(values.Skip(1).First().Name, Is.EqualTo(GaugeSetName.LatestDay));
            Assert.That(values.Skip(1).First().GaugeValues.Count, Is.EqualTo(1));
            Assert.That(values.Skip(2).First().Name, Is.EqualTo(GaugeSetName.LatestMonth));
            Assert.That(values.Skip(2).First().GaugeValues.Count, Is.EqualTo(1));
            Assert.That(values.Last().Name, Is.EqualTo(GaugeSetName.LatestYear));
            Assert.That(values.Last().GaugeValues.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetLatestGroupsByLabelObisCodeAndDeviceId()
        {
            // Arrange
            var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            var target = CreateTarget();
            var rd1 = new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            var rd2 = new Db.LiveReading { LabelId = 1, DeviceId = 20, Timestamp = dt };
            var rd3 = new Db.LiveReading { LabelId = 2, DeviceId = 20, Timestamp = dt };
            var rd4 = new Db.LiveReading { LabelId = 2, DeviceId = 20, Timestamp = dt };
            DbContext.InsertReadings(rd1, rd2, rd3, rd4);
            byte electrActiveEnergyA14 = 1;
            byte electrActiveEnergyA23 = 2;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14), (electrActiveEnergyA23, ObisCode.ElectrActiveEnergyA23));
            var rg1 = new Db.LiveRegister { ReadingId = rd1.Id, ObisId = electrActiveEnergyA14, Value = 1, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg2 = new Db.LiveRegister { ReadingId = rd2.Id, ObisId = electrActiveEnergyA14, Value = 2, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg3 = new Db.LiveRegister { ReadingId = rd3.Id, ObisId = electrActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg4 = new Db.LiveRegister { ReadingId = rd4.Id, ObisId = electrActiveEnergyA23, Value = 4, Scale = 1, Unit = (byte)Unit.WattHour };
            DbContext.InsertRegisters(rg1, rg2, rg3, rg4);

            // Act
            var values = target.GetLatest(dt);

            // Assert
            Assert.That(values.Count, Is.EqualTo(1));
            Assert.That(values.First().GaugeValues.Count, Is.EqualTo(4));
        }

        [Test]
        public void GetLatestFiltersOnTimestamp()
        {
            // Arrange
            var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            var target = CreateTarget();
            var rd = new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dt - TimeSpan.FromDays(2) };
            DbContext.InsertReadings(rd);
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            var rg = new Db.LiveRegister { ReadingId = rd.Id, ObisId = electrActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour };
            DbContext.InsertRegisters(rg);

            // Act
            var values = target.GetLatest(dt);

            // Assert
            Assert.That(values.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetLatestFiltersOnCumulatingObisCodes()
        {
            // Arrange
            var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            var target = CreateTarget();
            var rd = new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            DbContext.InsertReadings(rd);
            byte electrActualPowerP14 = 1;
            DbContext.InsertObisCodes((electrActualPowerP14, ObisCode.ElectrActualPowerP14));
            var rg = new Db.LiveRegister { ReadingId = rd.Id, ObisId = electrActualPowerP14, Value = 3, Scale = 1, Unit = (byte)Unit.Watt };
            DbContext.InsertRegisters(rg);

            // Act
            var values = target.GetLatest(dt);

            // Assert
            Assert.That(values.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetCustom()
        {
            // Arrange
            var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            var target = CreateTarget();
            var rd = new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            DbContext.InsertReadings(rd);
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            var rg = new Db.DayRegister { ReadingId = rd.Id, ObisId = electrActiveEnergyA14, Value = 2, Scale = 1, Unit = (byte)Unit.WattHour };
            DbContext.InsertRegisters(rg);

            // Act
            var values = target.GetCustom(dt.AddHours(1));

            // Assert
            Assert.That(values.Count, Is.EqualTo(1));
            Assert.That(values.First().Name, Is.EqualTo(GaugeSetName.Custom));
            Assert.That(values.First().GaugeValues.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetCustomGroupsByLabelObisCodeAndDeviceId()
        {
            // Arrange
            var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            var target = CreateTarget();
            var rd1 = new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            var rd2 = new Db.DayReading { LabelId = 1, DeviceId = 20, Timestamp = dt };
            var rd3 = new Db.DayReading { LabelId = 2, DeviceId = 20, Timestamp = dt };
            var rd4 = new Db.DayReading { LabelId = 2, DeviceId = 20, Timestamp = dt };
            DbContext.InsertReadings(rd1, rd2, rd3, rd4);
            byte electrActiveEnergyA14 = 1;
            byte electrActiveEnergyA23 = 2;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14), (electrActiveEnergyA23, ObisCode.ElectrActiveEnergyA23));
            var rg1 = new Db.DayRegister { ReadingId = rd1.Id, ObisId = electrActiveEnergyA14, Value = 1, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg2 = new Db.DayRegister { ReadingId = rd2.Id, ObisId = electrActiveEnergyA14, Value = 2, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg3 = new Db.DayRegister { ReadingId = rd3.Id, ObisId = electrActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg4 = new Db.DayRegister { ReadingId = rd4.Id, ObisId = electrActiveEnergyA23, Value = 4, Scale = 1, Unit = (byte)Unit.WattHour };
            DbContext.InsertRegisters(rg1, rg2, rg3, rg4);

            // Act
            var values = target.GetCustom(dt.AddHours(1));

            // Assert
            Assert.That(values.Count, Is.EqualTo(1));
            Assert.That(values.First().GaugeValues.Count, Is.EqualTo(4));
        }

        [Test]
        public void GetCustomFiltersOnTimestamp()
        {
            // Arrange
            var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            var target = CreateTarget();
            var rd1 = new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = dt - TimeSpan.FromDays(3) };
            var rd2 = new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            DbContext.InsertReadings(rd1, rd2);
            byte electrActiveEnergyA14 = 1;
            DbContext.InsertObisCodes((electrActiveEnergyA14, ObisCode.ElectrActiveEnergyA14));
            var rg1 = new Db.DayRegister { ReadingId = rd1.Id, ObisId = electrActiveEnergyA14, Value = 3, Scale = 1, Unit = (byte)Unit.WattHour };
            var rg2 = new Db.DayRegister { ReadingId = rd2.Id, ObisId = electrActiveEnergyA14, Value = 4, Scale = 1, Unit = (byte)Unit.WattHour };
            DbContext.InsertRegisters(rg1, rg2);

            // Act
            var values = target.GetCustom(dt);

            // Assert
            Assert.That(values.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetCustomFiltersOnCumulatingObisCodes()
        {
            // Arrange
            var dt = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            var target = CreateTarget();
            var rd = new Db.DayReading { LabelId = 1, DeviceId = 10, Timestamp = dt };
            DbContext.InsertReadings(rd);
            byte electrActualPowerP14 = 1;
            DbContext.InsertObisCodes((electrActualPowerP14, ObisCode.ElectrActualPowerP14));
            var rg = new Db.DayRegister { ReadingId = rd.Id, ObisId = electrActualPowerP14, Value = 3, Scale = 1, Unit = (byte)Unit.Watt };
            DbContext.InsertRegisters(rg);

            // Act
            var values = target.GetCustom(dt.AddHours(1));

            // Assert
            Assert.That(values.Count, Is.EqualTo(0));
        }

        private GaugeRepository CreateTarget()
        {
            return new GaugeRepository(DbContext);
        }

    }
}
