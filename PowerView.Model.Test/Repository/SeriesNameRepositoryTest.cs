using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class SeriesNameRepositoryTest : DbTestFixtureWithSchema
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new SeriesNameRepository(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetSerieNames()
        {
            // Arrange
            var target = CreateTarget();
            ObisCode obisCode1 = "1.2.3.4.5.6";
            ObisCode obisCode2 = "6.5.4.3.2.1";
            ObisCode obisCode3 = "3.4.3.4.3.4";
            var labels1 = Insert<Db.LiveReading, Db.LiveRegister>(1, obisCode1);
            var labels2 = Insert<Db.LiveReading, Db.LiveRegister>(2, obisCode2);
            Insert<Db.DayReading, Db.DayRegister>(2, obisCode2);
            var labels3 = Insert<Db.DayReading, Db.DayRegister>(3, obisCode3);
            Insert<Db.MonthReading, Db.MonthRegister>(3, obisCode3);
            Insert<Db.MonthReading, Db.MonthRegister>(1, obisCode1);

            // Act
            var serieNames = target.GetSeriesNames(TimeZoneInfo.Local);

            // Assert
            Assert.That(serieNames.Count, Is.EqualTo(3));
            Assert.That(serieNames.Count(sc => sc.Label == labels1.First() && sc.ObisCode == obisCode1), Is.EqualTo(1));
            Assert.That(serieNames.Count(sc => sc.Label == labels2.First() && sc.ObisCode == obisCode2), Is.EqualTo(1));
            Assert.That(serieNames.Count(sc => sc.Label == labels3.First() && sc.ObisCode == obisCode3), Is.EqualTo(1));
        }

        [Test]
        public void GetSerieNamesReplaceCumulativeObisCodes()
        {
            // Arrange
            var target = CreateTarget();
            var labels = Insert<Db.DayReading, Db.DayRegister>(1, ObisCode.ElectrActiveEnergyA14);

            // Act
            var serieColors = target.GetSeriesNames(TimeZoneInfo.Local);

            // Assert
            Assert.That(serieColors.Count, Is.EqualTo(3));
            Assert.That(serieColors.Count(sc => sc.Label == labels.First() && sc.ObisCode == ObisCode.ElectrActiveEnergyA14Period), Is.EqualTo(1));
            Assert.That(serieColors.Count(sc => sc.Label == labels.First() && sc.ObisCode == ObisCode.ElectrActiveEnergyA14Delta), Is.EqualTo(1));
            Assert.That(serieColors.Count(sc => sc.Label == labels.First() && sc.ObisCode == ObisCode.ElectrActualPowerP14Average), Is.EqualTo(1));
        }

        [Test]
        public void GetStoredSerieNames()
        {
            // Arrange
            var target = CreateTarget();
            ObisCode obisCode1 = "1.2.3.4.5.6";
            ObisCode obisCode2 = "6.5.4.3.2.1";
            ObisCode obisCode3 = "3.4.3.4.3.4";
            var labels1 = Insert<Db.LiveReading, Db.LiveRegister>(1, obisCode1);
            var labels2 = Insert<Db.LiveReading, Db.LiveRegister>(2, obisCode2);
            Insert<Db.DayReading, Db.DayRegister>(2, obisCode2);
            var labels3 = Insert<Db.DayReading, Db.DayRegister>(3, obisCode3);
            Insert<Db.MonthReading, Db.MonthRegister>(3, obisCode3);
            Insert<Db.MonthReading, Db.MonthRegister>(1, obisCode1);

            // Act
            var serieNames = target.GetSeriesNames(TimeZoneInfo.Local);

            // Assert
            Assert.That(serieNames.Count, Is.EqualTo(3));
            Assert.That(serieNames.Count(sc => sc.Label == labels1.First() && sc.ObisCode == obisCode1), Is.EqualTo(1));
            Assert.That(serieNames.Count(sc => sc.Label == labels2.First() && sc.ObisCode == obisCode2), Is.EqualTo(1));
            Assert.That(serieNames.Count(sc => sc.Label == labels3.First() && sc.ObisCode == obisCode3), Is.EqualTo(1));
        }

        private IList<string> Insert<TReading, TRegister>(byte labelId, params ObisCode[] obisCodes)
          where TReading : IDbReading, new()
          where TRegister : IDbRegister, new()
        {
            var reading = new TReading { LabelId = labelId, DeviceId = 10, Timestamp = DateTime.UtcNow };
            (var labels, var _) = DbContext.InsertReadings(reading);

            var obisCodesDict = DbContext.InsertObisCodes(obisCodes).ToDictionary(x => x.ObisCode, x => x.Id);
            var registers = obisCodes
              .Select(oc => (IDbRegister)new TRegister { ObisId = obisCodesDict[oc], Value = 2, Scale = 0, Unit = (byte)Unit.Watt, ReadingId = reading.Id })
              .ToArray();
            DbContext.InsertRegisters(registers);

            return labels;
        }

        private SeriesNameRepository CreateTarget()
        {
            return new SeriesNameRepository(DbContext);
        }

    }
}
