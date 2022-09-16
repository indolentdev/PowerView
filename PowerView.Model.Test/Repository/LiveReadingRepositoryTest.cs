using System;
using System.Linq;
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
            Assert.That(() => target.Add(new LiveReading[] { null }), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void AddEmpty()
        {
            // Arrange
            var target = CreateTarget();

            // Act
            target.Add(new LiveReading[0]);

            // Assert
        }

        [Test]
        public void AddOneLiveReadingWithOneRegister()
        {
            // Arrange
            var liveReading = new LiveReading("TheLabel", "1", DateTime.UtcNow, new[] { new RegisterValue("1.2.3.4.5.6", 10, -1, Unit.WattHour) });
            var target = CreateTarget();

            // Act
            target.Add(new[] { liveReading });

            // Assert
            AssertLiveReading(liveReading);
        }

        [Test]
        public void AddTwoLiveReadingsWithTwoRegisters()
        {
            // Arrange
            var liveReading = new LiveReading("TheLabel", "1", DateTime.UtcNow, new[] { new RegisterValue("1.1.1.1.1.1", 10, 1, Unit.WattHour), new RegisterValue("11.11.11.11.11.11", 1010, 1, Unit.Watt) });
            var liveReading2 = new LiveReading("TheLabel2", "2", DateTime.UtcNow, new[] { new RegisterValue("2.2.2.2.2.2", 20, 2, Unit.WattHour), new RegisterValue("22.22.22.22.22.22", 2020, 2, Unit.Watt) });
            var target = CreateTarget();

            // Act
            target.Add(new[] { liveReading, liveReading2 });

            // Assert
            AssertLiveReading(liveReading);
            AssertLiveReading(liveReading2);
        }

        private LiveReadingRepository CreateTarget()
        {
            return new LiveReadingRepository(DbContext);
        }

        private void AssertLiveReading(LiveReading liveReading)
        {
            var rd = DbContext.QueryTransaction<long>("SELECT lr.Id FROM LiveReading AS lr JOIN Label AS lbl ON lr.LabelId=lbl.Id JOIN Device AS dev ON lr.DeviceId=dev.Id WHERE lbl.LabelName = @Label AND dev.DeviceName = @DeviceId AND lr.Timestamp = @Timestamp;",
                new { Label = liveReading.Label, DeviceId = liveReading.DeviceId, Timestamp = (UnixTime)liveReading.Timestamp });
            Assert.That(rd.Count, Is.EqualTo(1));

            var reg = DbContext.QueryTransaction<dynamic>("SELECT * FROM LiveRegister WHERE ReadingId = @ReadingId;", new { ReadingId = rd.First() });
            var registerValues = liveReading.GetRegisterValues();
            Assert.That(reg.Count, Is.EqualTo(registerValues.Count));
            var expectedRegisters = registerValues.Select(x => new { ObisCode = (long)x.ObisCode, x.Scale, Unit = (byte)x.Unit, x.Value }).ToArray();
            var actualRegisters = reg.Select(x => new { ObisCode = (long)x.ObisCode, Scale = (short)x.Scale, Unit = (byte)x.Unit, Value = (int)x.Value }).ToArray();
            Assert.That(actualRegisters, Is.EquivalentTo(expectedRegisters));
        }

    }
}
