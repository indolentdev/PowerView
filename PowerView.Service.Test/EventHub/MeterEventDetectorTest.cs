using System;
using System.Collections.Generic;
using System.Linq;
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
    private Mock<ILocationProvider> locationProvider;

    [SetUp]
    public void SetUp()
    {
      profileRepository = new Mock<IProfileRepository>();
      meterEventRepository = new Mock<IMeterEventRepository>();
      locationProvider = new Mock<ILocationProvider>();
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var timeConverter = new Mock<ITimeConverter>();

      // Act & Assert
      Assert.That(() => new MeterEventDetector(null, profileRepository.Object, meterEventRepository.Object, locationProvider.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new MeterEventDetector(timeConverter.Object, null, meterEventRepository.Object, locationProvider.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new MeterEventDetector(timeConverter.Object, profileRepository.Object, null, locationProvider.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new MeterEventDetector(timeConverter.Object, profileRepository.Object, meterEventRepository.Object, null), Throws.TypeOf<ArgumentNullException>());
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
      var time = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labelSeries = GetLeakProfile(time);
      profileRepository.Setup(pr => pr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(time, time, new [] { labelSeries }));
      meterEventRepository.Setup(mer => mer.GetLatestMeterEventsByLabel()).Returns(new MeterEvent[0]);

      // Act
      CreateTarget().DetectMeterEvents(time);

      // Assert
      meterEventRepository.Verify(mer => mer.AddMeterEvents(ContainsLeak(labelSeries.Label, time, true, time, time.AddHours(6))));
    }

    [Test]
    public void DetectMeterEventsChangedEvent()
    {
      // Arrange
      var time = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labelSeries = GetLeakProfile(time);
      profileRepository.Setup(pr => pr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(time, time, new[] { labelSeries }));
      var oldTime = time.Subtract(TimeSpan.FromDays(4));
      var meterEvent = new MeterEvent(labelSeries.Label, oldTime, false, new LeakMeterEventAmplification(oldTime, oldTime, new UnitValue()));
      meterEventRepository.Setup(mer => mer.GetLatestMeterEventsByLabel()).Returns(new [] { meterEvent });

      // Act
      CreateTarget().DetectMeterEvents(time);

      // Assert
      meterEventRepository.Verify(mer => mer.AddMeterEvents(ContainsLeak(labelSeries.Label, time, true, time, time.AddHours(6))));
    }

    [Test]
    public void DetectMeterEventsRedundantEvent()
    {
      // Arrange
      var time = TimeZoneHelper.GetDenmarkTodayAsUtc();
      var labelSeries = GetLeakProfile(time);
      profileRepository.Setup(pr => pr.GetDayProfileSet(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        .Returns(new LabelSeriesSet<TimeRegisterValue>(time, time, new[] { labelSeries }));
      var oldTime = time.Subtract(TimeSpan.FromDays(4));
      var meterEvent = new MeterEvent(labelSeries.Label, oldTime, true, new LeakMeterEventAmplification(oldTime, oldTime, new UnitValue()));
      meterEventRepository.Setup(mer => mer.GetLatestMeterEventsByLabel()).Returns(new [] { meterEvent });

      // Act
      CreateTarget().DetectMeterEvents(time);

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

    private static LabelSeries<TimeRegisterValue> GetLeakProfile(DateTime time)
    {
      var baseTime = TimeZoneHelper.GetDenmarkTodayAsUtc().AddMinutes(58);
      var timestamps = Enumerable.Range(-1, 25).Select(i => baseTime.AddHours(i)).ToArray(); 
      var values = timestamps.Select((dt, i) => new TimeRegisterValue("1", dt, i+3, 1, Unit.CubicMetre)).ToArray();
      var dict = new Dictionary<ObisCode, IEnumerable<TimeRegisterValue>> { { ObisCode.ColdWaterVolume1, values } };
      var labelSeries = new LabelSeries<TimeRegisterValue>("lbl", dict);
      return labelSeries;
    }

    private MeterEventDetector CreateTarget()
    {
      var tzi = TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
      locationProvider.Setup(x => x.GetTimeZone()).Returns(tzi);
      var timeConverter = new TimeConverter(locationProvider.Object);
      return new MeterEventDetector(timeConverter, profileRepository.Object, meterEventRepository.Object, locationProvider.Object);
    }
  }
}

