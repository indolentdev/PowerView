using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class CrudeDataRepositoryTest : DbTestFixtureWithSchema
    {
        private ILocationContext LocationContext = TimeZoneHelper.GetDenmarkLocationContext();

        [Test]
        [TestCase(null, "2022-10-18T13:21:10Z", 0, 1, typeof(ArgumentNullException))]
        [TestCase("Label", "2022-10-18T13:21:10", 0, 1, typeof(ArgumentOutOfRangeException))]
        [TestCase("Label", "2022-10-18T13:21:10Z", -1, 1, typeof(ArgumentOutOfRangeException))]
        [TestCase("Label", "2022-10-18T13:21:10Z", 0, 0, typeof(ArgumentOutOfRangeException))]
        [TestCase("Label", "2022-10-18T13:21:10Z", 0, 10001, typeof(ArgumentOutOfRangeException))]
        public void GetCrudeDataThrows(string label, string dateTimeString, int skip, int take, Type exceptionType)
        {
            // Arrange
            var dateTime = DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var target = CreateTarget();

            // Act & Asssert
            Assert.That(() => target.GetCrudeData(label, dateTime, skip, take), Throws.TypeOf(exceptionType));
        }

        [Test]
        public void GetCrudeData()
        {
            // Arrange
            var dtA = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            (var labels, var deviceIds) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtA },
                new Db.LiveRegister { ObisId = 111, Value = 101, Scale = 1, Unit = 1 });

            var dtB = dtA.AddSeconds(44);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtB }, 
                new Db.LiveRegister { ObisId = 111, Value = 201, Scale = 1, Unit = 1 }, new Db.LiveRegister { ObisId = 112, Value = 202, Scale = 1, Unit = 1 });
            DbContext.Insert(new Db.LiveReading { LabelId = 99, DeviceId = 99, Timestamp = dtB },
                new Db.LiveRegister { ObisId = 99, Value = 99, Scale = 1, Unit = 1 });

            var dtC = dtB.AddSeconds(28);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtC },
                new Db.LiveRegister { ObisId = 111, Value = 301, Scale = 1, Unit = 1 });

            var dtD = dtC.AddSeconds(33);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtD },
                new Db.LiveRegister { ObisId = 111, Value = 401, Scale = 1, Unit = 1 }, new Db.LiveRegister { ObisId = 112, Value = 402, Scale = 1, Unit = 1 });

            var target = CreateTarget();

            // Act & Assert
            var values1 = target.GetCrudeData(labels.First(), dtB, 0, 1 );
            Assert.That(values1.TotalCount, Is.EqualTo(5));

            Assert.That(values1.Result.Count, Is.EqualTo(1));
            Assert.That(values1.Result.First().DateTime, Is.EqualTo(dtB));
            Assert.That(values1.Result.First().ObisCode, Is.EqualTo(new ObisCode(new byte[] {111, 111, 111, 111, 111, 111})));
            Assert.That(values1.Result.First().Value, Is.EqualTo(201));
            Assert.That(values1.Result.First().Scale, Is.EqualTo(1));
            Assert.That(values1.Result.First().Unit, Is.EqualTo(Unit.WattHour));
            Assert.That(values1.Result.First().DeviceId, Is.EqualTo(deviceIds.First()));

            var values2 = target.GetCrudeData(labels.First(), dtB, 1, 2);
            Assert.That(values2.TotalCount, Is.EqualTo(5));
            Assert.That(values2.Result.Count, Is.EqualTo(2));
            Assert.That(values2.Result.First().ObisCode, Is.EqualTo(new ObisCode(new byte[] { 112, 112, 112, 112, 112, 112 })));
            Assert.That(values2.Result.First().Value, Is.EqualTo(202));
            Assert.That(values2.Result.Last().ObisCode, Is.EqualTo(new ObisCode(new byte[] { 111, 111, 111, 111, 111, 111 })));
            Assert.That(values2.Result.Last().Value, Is.EqualTo(301));
        }

        [Test]
        [TestCase(null, "2022-10-18T13:21:10Z", typeof(ArgumentNullException))]
        [TestCase("Label", "2022-10-18T13:21:10", typeof(ArgumentOutOfRangeException))]
        public void GetCrudeDataByThrows(string label, string dateTimeString, Type exceptionType)
        {
            // Arrange
            var dateTime = DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var target = CreateTarget();

            // Act & Asssert
            Assert.That(() => target.GetCrudeDataBy(label, dateTime), Throws.TypeOf(exceptionType));
        }

        [Test]
        public void GetCrudeDataBy()
        {
            // Arrange
            var dtA = new DateTime(2017, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            (var labels, var deviceIds) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtA },
                new Db.LiveRegister { ObisId = 111, Value = 101, Scale = 1, Unit = 1 });

            var dtB = dtA.AddSeconds(44);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtB },
                new Db.LiveRegister { ObisId = 111, Value = 201, Scale = 1, Unit = 1 }, new Db.LiveRegister { ObisId = 112, Value = 202, Scale = 1, Unit = 1 });
            DbContext.Insert(new Db.LiveReading { LabelId = 99, DeviceId = 99, Timestamp = dtB },
                new Db.LiveRegister { ObisId = 99, Value = 99, Scale = 1, Unit = 1 });

            var dtC = dtB.AddSeconds(28);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtC },
                new Db.LiveRegister { ObisId = 111, Value = 301, Scale = 1, Unit = 1 });

            var target = CreateTarget();

            // Act & Assert
            var values = target.GetCrudeDataBy(labels.First(), dtB);

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(values.First().DateTime, Is.EqualTo(dtB));
            Assert.That(values.First().Scale, Is.EqualTo(1));
            Assert.That(values.First().Unit, Is.EqualTo(Unit.WattHour));
            Assert.That(values.First().DeviceId, Is.EqualTo(deviceIds.First()));

            Assert.That(values.First().ObisCode, Is.EqualTo(new ObisCode(new byte[] { 111, 111, 111, 111, 111, 111 })));
            Assert.That(values.First().Value, Is.EqualTo(201));
            Assert.That(values.Last().ObisCode, Is.EqualTo(new ObisCode(new byte[] { 112, 112, 112, 112, 112, 112 })));
            Assert.That(values.Last().Value, Is.EqualTo(202));
        }

        [Test]
        public void GetMissingDaysThrows()
        {
            // Arrange
            var target = CreateTarget();

            // Act & Asssert
            Assert.That(() => target.GetMissingDays(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetMissingDays()
        {
            // Arrange
            var dtA = new DateTime(2017, 2, 17, 8, 0, 0, DateTimeKind.Utc);
            (var labels, var deviceIds) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtA },
                new Db.LiveRegister { ObisId = 111, Value = 101, Scale = 1, Unit = 1 });

            var dtB1 = dtA.AddHours(24 + 24 + 1);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtB1 },
                new Db.LiveRegister { ObisId = 111, Value = 211, Scale = 1, Unit = 1 });
            var dtB2 = dtA.AddHours(24 + 24 + 4);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtB2 },
                new Db.LiveRegister { ObisId = 111, Value = 221, Scale = 1, Unit = 1 });

            var dtC = dtA.AddHours(24 + 24 + 24 + 24 + 24 + 3);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtC },
                new Db.LiveRegister { ObisId = 111, Value = 301, Scale = 1, Unit = 1 });

            var target = CreateTarget();

            // Act
            var missingDays = target.GetMissingDays(labels.First());

            // Assert
            Assert.That(missingDays.Count, Is.EqualTo(3));
            Assert.That(missingDays, Contains.Item(new MissingDate(LocationContext.ConvertTimeToUtc(new DateTime(2017, 2, 18, 23, 59, 59)), dtA, dtB1)));
            Assert.That(missingDays, Contains.Item(new MissingDate(LocationContext.ConvertTimeToUtc(new DateTime(2017, 2, 20, 23, 59, 59)), dtB2, dtC)));
            Assert.That(missingDays, Contains.Item(new MissingDate(LocationContext.ConvertTimeToUtc(new DateTime(2017, 2, 21, 23, 59, 59)), dtB2, dtC)));
        }

        [Test]
        public void GetMissingDaysOneReading()
        {
            // Arrange
            var dtA = new DateTime(2017, 2, 17, 8, 0, 0, DateTimeKind.Utc);
            (var labels, var deviceIds) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtA },
                new Db.LiveRegister { ObisId = 111, Value = 101, Scale = 1, Unit = 1 });

            var target = CreateTarget();

            // Act
            var missingDays = target.GetMissingDays(labels.First());

            // Assert
            Assert.That(missingDays, Is.Empty);
        }

        [Test]
        public void GetMissingDaysTwoReadingsSameDay()
        {
            // Arrange
            var dtA = new DateTime(2017, 2, 17, 8, 0, 0, DateTimeKind.Utc);
            (var labels, var deviceIds) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtA },
                new Db.LiveRegister { ObisId = 111, Value = 101, Scale = 1, Unit = 1 });

            var dtB = dtA.AddHours(2);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtB },
                new Db.LiveRegister { ObisId = 111, Value = 201, Scale = 1, Unit = 1 });

            var target = CreateTarget();

            // Act
            var missingDays = target.GetMissingDays(labels.First());

            // Assert
            Assert.That(missingDays, Is.Empty);
        }

        [Test]
        public void GetMissingDaysTworReadingsDifferentDeviceIds()
        {
            // Arrange
            var dtA = new DateTime(2017, 2, 17, 8, 0, 0, DateTimeKind.Utc);
            (var labels, var deviceIds) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dtA },
                new Db.LiveRegister { ObisId = 111, Value = 101, Scale = 1, Unit = 1 });

            var dtB = dtA.AddHours(24 + 25);
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 20, Timestamp = dtB },
                new Db.LiveRegister { ObisId = 111, Value = 201, Scale = 1, Unit = 1 });

            var target = CreateTarget();

            // Act
            var missingDays = target.GetMissingDays(labels.First());

            // Assert
            Assert.That(missingDays, Is.Empty);
        }

        private CrudeDataRepository CreateTarget()
        {
            return new CrudeDataRepository(DbContext, LocationContext);
        }

    }
}
