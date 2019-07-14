﻿using System;
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

    [SetUp]
    public void SetUp()
    {
      profileRepository = new Mock<IProfileRepository>();
      meterEventRepository = new Mock<IMeterEventRepository>();
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var timeConverter = new Mock<ITimeConverter>();

      // Act & Assert
      Assert.That(() => new MeterEventDetector(null, profileRepository.Object, meterEventRepository.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new MeterEventDetector(timeConverter.Object, null, meterEventRepository.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new MeterEventDetector(timeConverter.Object, profileRepository.Object, null), Throws.TypeOf<ArgumentNullException>());
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
      var time = new DateTime(2017, 1, 7, 10, 0, 22, DateTimeKind.Utc);
      var labelProfile = GetLeakProfile(time);
      profileRepository.Setup(pr => pr.GetDayProfileSet(It.IsAny<DateTime>())).Returns(new LabelProfileSet(time, new [] { labelProfile }));
      meterEventRepository.Setup(mer => mer.GetLatestMeterEventsByLabel()).Returns(new MeterEvent[0]);

      // Act
      CreateTarget().DetectMeterEvents(time);

      // Assert
      var start = new DateTime(2017, 1, 6, 23, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2017, 1, 7, 5, 0, 0, DateTimeKind.Utc);
      meterEventRepository.Verify(mer => mer.AddMeterEvents(ContainsLeak(labelProfile.Label, time, true, start, end)));
    }

    [Test]
    public void DetectMeterEventsChangedEvent()
    {
      // Arrange
      var time = new DateTime(2017, 1, 7, 10, 0, 22, DateTimeKind.Utc);
      var labelProfile = GetLeakProfile(time);
      profileRepository.Setup(pr => pr.GetDayProfileSet(It.IsAny<DateTime>())).Returns(new LabelProfileSet(time, new [] { labelProfile }));
      var oldTime = time.Subtract(TimeSpan.FromDays(4));
      var meterEvent = new MeterEvent(labelProfile.Label, oldTime, false, new LeakMeterEventAmplification(oldTime, oldTime, new UnitValue()));
      meterEventRepository.Setup(mer => mer.GetLatestMeterEventsByLabel()).Returns(new [] { meterEvent });

      // Act
      CreateTarget().DetectMeterEvents(time);

      // Assert
      var start = new DateTime(2017, 1, 6, 23, 0, 0, DateTimeKind.Utc);
      var end = new DateTime(2017, 1, 7, 5, 0, 0, DateTimeKind.Utc);
      meterEventRepository.Verify(mer => mer.AddMeterEvents(ContainsLeak(labelProfile.Label, time, true, start, end)));
    }

    [Test]
    public void DetectMeterEventsRedundantEvent()
    {
      // Arrange
      var time = new DateTime(2017, 1, 7, 10, 0, 22, DateTimeKind.Utc);
      var labelProfile = GetLeakProfile(time);
      profileRepository.Setup(pr => pr.GetDayProfileSet(It.IsAny<DateTime>())).Returns(new LabelProfileSet(time, new [] { labelProfile }));
      var oldTime = time.Subtract(TimeSpan.FromDays(4));
      var meterEvent = new MeterEvent(labelProfile.Label, oldTime, true, new LeakMeterEventAmplification(oldTime, oldTime, new UnitValue()));
      meterEventRepository.Setup(mer => mer.GetLatestMeterEventsByLabel()).Returns(new [] { meterEvent });

      // Act
      CreateTarget().DetectMeterEvents(time);

      // Assert
      meterEventRepository.Verify(mer => mer.AddMeterEvents(It.IsAny<IEnumerable<MeterEvent>>()), Times.Never);
    }

    public static IEnumerable<MeterEvent> ContainsLeak(string label, DateTime timestamp, bool flag, DateTime start, DateTime end) 
    { 
      return Match.Create<IEnumerable<MeterEvent>>(
        x => x.Where(me => 
          me.Label == label && me.DetectTimestamp == timestamp && me.Flag == flag &&
          me.Amplification is LeakMeterEventAmplification &&
          ((LeakMeterEventAmplification)me.Amplification).Start == start &&
          ((LeakMeterEventAmplification)me.Amplification).End == end
        ).Any() );
    }

    private static LabelProfile GetLeakProfile(DateTime time)
    {
      var timestamps = Enumerable.Range(0, 23).Select(i => new DateTime(time.Year, time.Month, time.Day, i, 58, 0, DateTimeKind.Utc)).ToArray(); 
      var values = timestamps.Select(dt => new TimeRegisterValue("1", dt, 1, 1, Unit.CubicMetre)).ToArray();
      var dict = new Dictionary<ObisCode, ICollection<TimeRegisterValue>> { { ObisCode.ColdWaterVolume1Delta, values } };
      var labelProfile = new LabelProfile("lbl", timestamps.First(), dict);
      return labelProfile;
    }

    private MeterEventDetector CreateTarget()
    {
      var tzi = TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
      var lp = new Mock<ILocationProvider>();
      lp.Setup(x => x.GetTimeZone()).Returns(tzi);
      var timeConverter = new TimeConverter(lp.Object);
      return new MeterEventDetector(timeConverter, profileRepository.Object, meterEventRepository.Object);
    }
  }
}
