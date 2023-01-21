using System;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class LiveReadingRepositoryTest : DbTestFixtureWithSchema
    {
        [Test]
        public void AddThrows()
        {
            // Arrange
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.Add(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.Add(new Reading[] { null }), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void AddEmpty()
        {
            // Arrange
            var target = CreateTarget();

            // Act
            target.Add(new Reading[0]);

            // Assert
            var count = DbContext.QueryTransaction<int>("SELECT count(*) FROM LiveReading;").First();
            Assert.That(count, Is.Zero);
        }

        [Test]
        public void AddCreatesLabel()
        {
            // Arrange
            var reading = new Reading("ALabel1", "DID1", DateTime.UtcNow, new[] { new RegisterValue("7.7.7.7.7.7", 10, -1, Unit.WattHour) });
            var target = CreateTarget();

            // Act
            target.Add(new[] { reading });

            // Assert
            var labels = DbContext.QueryTransaction<(string LabelName, UnixTime Timestamp)>("SELECT LabelName, Timestamp FROM Label;");
            Assert.That(labels.Count, Is.EqualTo(1));
            Assert.That(labels[0].LabelName, Is.EqualTo(reading.Label));
            Assert.That(labels[0].Timestamp.ToUnixTimeSeconds(), Is.EqualTo(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).Within(2));
        }

        [Test]
        public void AddCreatesDevice()
        {
            // Arrange
            var reading = new Reading("ALabel2", "DID2", DateTime.UtcNow, new[] { new RegisterValue("8.8.8.8.8.8", 10, -1, Unit.WattHour) });
            var target = CreateTarget();

            // Act
            target.Add(new[] { reading });

            // Assert
            var devices = DbContext.QueryTransaction<(string DeviceName, UnixTime Timestamp)>("SELECT DeviceName, Timestamp FROM Device;");
            Assert.That(devices.Count, Is.EqualTo(1));
            Assert.That(devices[0].DeviceName, Is.EqualTo(reading.DeviceId));
            Assert.That(devices[0].Timestamp.ToUnixTimeSeconds(), Is.EqualTo(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).Within(2));
        }

        [Test]
        public void AddCreatesObis()
        {
            // Arrange
            var reading = new Reading("ALabel3", "DID3", DateTime.UtcNow, new[] { new RegisterValue("9.9.9.9.9.9", 10, -1, Unit.WattHour) });
            var target = CreateTarget();

            // Act
            target.Add(new[] { reading });

            // Assert
            var obis = DbContext.QueryTransaction<long>("SELECT ObisCode FROM Obis;");
            Assert.That(obis.Count, Is.EqualTo(1));
            Assert.That(obis[0], Is.EqualTo((long)reading.GetRegisterValues().First().ObisCode));
        }

        [Test]
        public void AddOneReadingWithOneRegister()
        {
            // Arrange
            var reading = new Reading("TheLabel", "1", DateTime.UtcNow, new[] { new RegisterValue("1.2.3.4.5.6", 10, -1, Unit.WattHour) });
            var target = CreateTarget();

            // Act
            target.Add(new[] { reading });

            // Assert
            AssertLiveReading(reading);
        }

        [Test]
        public void AddTwoReadingsWithTwoRegisters()
        {
            // Arrange
            var reading = new Reading("TheLabel", "1", DateTime.UtcNow, new[] { new RegisterValue("1.1.1.1.1.1", 10, 1, Unit.WattHour), new RegisterValue("11.11.11.11.11.11", 1010, 1, Unit.Watt) });
            var reading2 = new Reading("TheLabel2", "2", DateTime.UtcNow, new[] { new RegisterValue("2.2.2.2.2.2", 20, 2, Unit.WattHour), new RegisterValue("22.22.22.22.22.22", 2020, 2, Unit.Watt) });
            var target = CreateTarget();

            // Act
            target.Add(new[] { reading, reading2 });

            // Assert
            AssertLiveReading(reading);
            AssertLiveReading(reading2);
        }

        [Test]
        public void AddOneReadingWithOneRegisterTwoTimes()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var readingA = new Reading("TheLabel", "1", timestamp, new[] { new RegisterValue("1.1.1.1.1.1", 10, 1, Unit.WattHour) });
            var readingB = new Reading("TheLabel", "1", timestamp, new[] { new RegisterValue("2.2.2.2.2.2", 20, 2, Unit.WattHour) });
            var target = CreateTarget();

            // Act
            target.Add(new[] { readingA });
            target.Add(new[] { readingB });

            // Assert
            var expectedReading = new Reading(readingA.Label, readingA.DeviceId, readingA.Timestamp, readingA.GetRegisterValues().Concat(readingB.GetRegisterValues()));
            AssertLiveReading(expectedReading);
        }

        [Test]
        public void AddOneReadingWithOneRegisterDuplicateLabelTimestampAndObisCode()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var readingA = new Reading("TheLabel", "1", timestamp, new[] { new RegisterValue("1.1.1.1.1.1", 10, 1, Unit.WattHour) });
            var readingB = new Reading("TheLabel", "2", timestamp, new[] { new RegisterValue("1.1.1.1.1.1", 20, 2, Unit.Watt, RegisterValueTag.Manual) });
            var target = CreateTarget();

            // Act
            target.Add(new[] { readingA });
            target.Add(new[] { readingB });

            // Assert
            AssertLiveReading(readingA);
        }

        [Test]
        public void GetObisCodesThrows()
        {
            // Arrange
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.GetObisCodes(null, DateTime.UtcNow), Throws.ArgumentNullException);
            Assert.That(() => target.GetObisCodes("lbl", DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetObisCodesEmpty()
        {
            // Arrange
            var target = CreateTarget();

            // Act
            var obisCodes = target.GetObisCodes("theLabel", DateTime.UtcNow);

            // Assert
            Assert.That(obisCodes, Is.Empty);
        }

        [Test]
        public void GetObisCodes()
        {
            // Arrange
            var utcNow = DateTime.UtcNow;
            var dt = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, DateTimeKind.Utc);
            (var labels, var deviceIds) = DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dt.AddMinutes(-1) },
                new[] { new Db.LiveRegister { ObisId = 110, Value = 101, Scale = 1, Unit = 1 }, new Db.LiveRegister { ObisId = 112, Value = 102, Scale = 1, Unit = 1 } });
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dt },
                new[] { new Db.LiveRegister { ObisId = 111, Value = 201, Scale = 1, Unit = 1 }, new Db.LiveRegister { ObisId = 112, Value = 202, Scale = 1, Unit = 1 } });
            DbContext.Insert(new Db.LiveReading { LabelId = 2, DeviceId = 20, Timestamp = dt },
                new[] { new Db.LiveRegister { ObisId = 111, Value = 301, Scale = 1, Unit = 1 }, new Db.LiveRegister { ObisId = 113, Value = 302, Scale = 1, Unit = 1 } });
            DbContext.Insert(new Db.LiveReading { LabelId = 1, DeviceId = 10, Timestamp = dt.AddMinutes(5) },
                 new[] { new Db.LiveRegister { ObisId = 111, Value = 212, Scale = 1, Unit = 1 }, new Db.LiveRegister { ObisId = 114, Value = 212, Scale = 1, Unit = 1 } });
            var target = CreateTarget();

            // Act
            var obisCodes = target.GetObisCodes(labels.First(), dt);

            // Assert
            Assert.That(obisCodes, Is.EqualTo(new [] { (ObisCode)"111.111.111.111.111.111", (ObisCode)"114.114.114.114.114.114", (ObisCode)"112.112.112.112.112.112" }));
        }

        private LiveReadingRepository CreateTarget()
        {
            return new LiveReadingRepository(new NullLogger<LiveReadingRepository>(), DbContext);
        }

        private void AssertLiveReading(Reading reading)
        {
            var rd = DbContext.QueryTransaction<long>("SELECT lr.Id FROM LiveReading AS lr JOIN Label AS lbl ON lr.LabelId=lbl.Id JOIN Device AS dev ON lr.DeviceId=dev.Id WHERE lbl.LabelName = @Label AND dev.DeviceName = @DeviceId AND lr.Timestamp = @Timestamp;",
                new { Label = reading.Label, DeviceId = reading.DeviceId, Timestamp = (UnixTime)reading.Timestamp });
            Assert.That(rd.Count, Is.EqualTo(1));

            var reg = DbContext.QueryTransaction<dynamic>("SELECT o.ObisCode,reg.Scale,reg.Unit,Reg.Value FROM LiveRegister reg JOIN Obis o ON reg.ObisId=o.Id WHERE ReadingId = @ReadingId;", new { ReadingId = rd.First() });
            var registerValues = reading.GetRegisterValues();            
            Assert.That(reg.Count, Is.EqualTo(registerValues.Count));
            var expectedRegisters = registerValues.Select(x => new { ObisCode = (long)x.ObisCode, x.Scale, Unit = (byte)x.Unit, x.Value }).ToArray();
            var actualRegisters = reg.Select(x => new { ObisCode = (long)x.ObisCode, Scale = (short)x.Scale, Unit = (byte)x.Unit, Value = (int)x.Value }).ToArray();
            Assert.That(actualRegisters, Is.EquivalentTo(expectedRegisters));

            var regTag = DbContext.QueryTransaction<dynamic>("SELECT o.ObisCode,t.Tags FROM LiveRegisterTag t JOIN Obis o ON t.ObisId=o.Id WHERE t.ReadingId = @ReadingId;", new { ReadingId = rd.First() });
            var registerValueTags = registerValues.Where(x => x.Tag != RegisterValueTag.None).ToList();
            Assert.That(regTag.Count, Is.EqualTo(registerValueTags.Count));
            var expectedRegisterTags = registerValueTags.Select(x => new { ObisCode = (long)x.ObisCode, Tags = (byte)x.Tag }).ToArray();
            var actualRegisterTags = regTag.Select(x => new { ObisCode = (long)x.ObisCode, Tags = (byte)x.Tags }).ToArray();
            Assert.That(actualRegisterTags, Is.EquivalentTo(expectedRegisterTags));
        }

    }
}
