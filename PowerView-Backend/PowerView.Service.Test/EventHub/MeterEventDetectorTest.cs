using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;
using NUnit.Framework;
using Moq;

namespace PowerView.Service.Test.EventHub
{
    [TestFixture]
    public class MeterEventDetectorTest
    {
        private Mock<IProfileRepository> profileRepository;
        private Mock<IMeterEventRepository> meterEventRepository;
        private ILocationContext locationContext;

        [SetUp]
        public void SetUp()
        {
            profileRepository = new Mock<IProfileRepository>();
            meterEventRepository = new Mock<IMeterEventRepository>();
            locationContext = TimeZoneHelper.GetDenmarkLocationContext();
        }

        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            var leakCharacteristicChecker = new LeakCharacteristicChecker(new NullLogger<LeakCharacteristicChecker>());

            // Act & Assert
            Assert.That(() => new MeterEventDetector(null, meterEventRepository.Object, locationContext, leakCharacteristicChecker), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new MeterEventDetector(profileRepository.Object, null, locationContext, leakCharacteristicChecker), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new MeterEventDetector(profileRepository.Object, meterEventRepository.Object, null, leakCharacteristicChecker), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new MeterEventDetector(profileRepository.Object, meterEventRepository.Object, locationContext, null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void DetectMeterEventsThrows()
        {
            // Arrange
            var target = CreateTarget();
            var dateTimeUnspecified = new DateTime(2017, 1, 7, 15, 17, 0, DateTimeKind.Unspecified);

            // Act & Assert
            Assert.That(() => target.DetectMeterEvents(DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => target.DetectMeterEvents(dateTimeUnspecified), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void DetectMeterEventsLeak()
        {
            // Arrange
            var midnightUtc = TimeZoneHelper.GetDenmarkFixedMidnightAsUtc();
            var labelSeries = GetLeakProfile(midnightUtc);
            profileRepository.Setup(pr => pr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
              .Returns(new TimeRegisterValueLabelSeriesSet(midnightUtc, midnightUtc, new[] { labelSeries }));
            meterEventRepository.Setup(mer => mer.GetLatestMeterEventsByLabel()).Returns(new MeterEvent[0]);
            var time = midnightUtc.AddHours(5.123);

            // Act
            CreateTarget().DetectMeterEvents(time);

            // Assert
            meterEventRepository.Verify(mer => mer.AddMeterEvents(ContainsLeak(labelSeries.Label, time, true, midnightUtc, midnightUtc.AddHours(6))));
        }

        [Test]
        public void DetectMeterEventsChangedEvent()
        {
            // Arrange
            var midnightUtc = TimeZoneHelper.GetDenmarkFixedMidnightAsUtc();
            var labelSeries = GetLeakProfile(midnightUtc);
            profileRepository.Setup(pr => pr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
              .Returns(new TimeRegisterValueLabelSeriesSet(midnightUtc, midnightUtc, new[] { labelSeries }));
            var oldTime = midnightUtc.Subtract(TimeSpan.FromDays(4));
            var meterEvent = new MeterEvent(labelSeries.Label, oldTime, false, new LeakMeterEventAmplification(oldTime, oldTime, new UnitValue()));
            meterEventRepository.Setup(mer => mer.GetLatestMeterEventsByLabel()).Returns(new[] { meterEvent });

            // Act
            CreateTarget().DetectMeterEvents(midnightUtc);

            // Assert
            meterEventRepository.Verify(mer => mer.AddMeterEvents(ContainsLeak(labelSeries.Label, midnightUtc, true, midnightUtc, midnightUtc.AddHours(6))));
        }

        [Test]
        public void DetectMeterEventsRedundantEvent()
        {
            // Arrange
            var midnightUtc = TimeZoneHelper.GetDenmarkFixedMidnightAsUtc();
            var labelSeries = GetLeakProfile(midnightUtc);
            profileRepository.Setup(pr => pr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
              .Returns(new TimeRegisterValueLabelSeriesSet(midnightUtc, midnightUtc, new[] { labelSeries }));
            var oldTime = midnightUtc.Subtract(TimeSpan.FromDays(4));
            var meterEvent = new MeterEvent(labelSeries.Label, oldTime, true, new LeakMeterEventAmplification(oldTime, oldTime, new UnitValue()));
            meterEventRepository.Setup(mer => mer.GetLatestMeterEventsByLabel()).Returns(new[] { meterEvent });

            // Act
            CreateTarget().DetectMeterEvents(midnightUtc);

            // Assert
            meterEventRepository.Verify(mer => mer.AddMeterEvents(It.IsAny<IEnumerable<MeterEvent>>()), Times.Never);
        }

        public static IEnumerable<MeterEvent> ContainsLeak(string label, DateTime timestamp, bool flag, DateTime start, DateTime end)
        {
            return Match.Create<IEnumerable<MeterEvent>>(
              x => x.Any(me =>
                me.Label == label && me.DetectTimestamp == timestamp && me.Flag == flag &&
                me.Amplification is LeakMeterEventAmplification &&
                ((LeakMeterEventAmplification)me.Amplification).Start == start &&
                ((LeakMeterEventAmplification)me.Amplification).End == end
              ));
        }

        private static TimeRegisterValueLabelSeries GetLeakProfile(DateTime midnightAsUtc)
        {
            var baseTime = midnightAsUtc.AddMinutes(58);
            var timestamps = Enumerable.Range(-1, 25).Select(i => baseTime.AddHours(i)).ToArray();
            var values = timestamps.Select((dt, i) => new TimeRegisterValue("1", dt, i + 3, 1, Unit.CubicMetre)).ToArray();
            var dict = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { ObisCode.ColdWaterVolume1, values } };
            var labelSeries = new TimeRegisterValueLabelSeries("lbl", dict);
            return labelSeries;
        }

        private MeterEventDetector CreateTarget()
        {
            return new MeterEventDetector(profileRepository.Object, meterEventRepository.Object, locationContext, new LeakCharacteristicChecker(new NullLogger<LeakCharacteristicChecker>()));
        }
    }
}

