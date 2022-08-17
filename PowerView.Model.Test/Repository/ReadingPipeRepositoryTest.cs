using System;
using NUnit.Framework;
using Moq;
using PowerView.Model.Repository;
using System.Globalization;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class ReadingPipeRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
    
      // Act & Assert
      Assert.That(() => new ReadingPipeRepository(DbContext, null), Throws.TypeOf<ArgumentNullException>());
    }
      
    [Test]
    public void PipeLiveReadingsToDayReadingsOneLabelReading()
    {
      // Arrange
      var dt = GetDateTime(28, 19, 31, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(1);
      var liveReading = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(liveReading);
      var target = CreateTarget();

      // Act
      var pipedSomething = target.PipeLiveReadingsToDayReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.DayReading>(), Is.EqualTo(1));
      AssertReading<Db.DayReading>(liveReading.Label, liveReading.DeviceId, liveReading.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(1));
      AssertStreamPosition("DayReading", liveReading.Label, liveReading.Id);

      Assert.That(pipedSomething, Is.True);
    }
      
    [Test]
    public void PipeLiveReadingsToDayReadingsMultipleLabelReading()
    {
      // Arrange
      var dt = GetDateTime(28, 19, 31, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(1);
      var liveReadingA = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt };
      var liveReadingB = new Db.LiveReading { Label="B", DeviceId="1", Timestamp=dt };
      var liveReadingC = new Db.LiveReading { Label="C", DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(liveReadingA, liveReadingB, liveReadingC);
      var target = CreateTarget();

      // Act
      target.PipeLiveReadingsToDayReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.DayReading>(), Is.EqualTo(3));
      AssertReading<Db.DayReading>(liveReadingA.Label, liveReadingA.DeviceId, liveReadingA.Timestamp);
      AssertReading<Db.DayReading>(liveReadingB.Label, liveReadingB.DeviceId, liveReadingB.Timestamp);
      AssertReading<Db.DayReading>(liveReadingC.Label, liveReadingC.DeviceId, liveReadingC.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(3));
      AssertStreamPosition("DayReading", liveReadingA.Label, liveReadingA.Id);
      AssertStreamPosition("DayReading", liveReadingB.Label, liveReadingB.Id);
      AssertStreamPosition("DayReading", liveReadingC.Label, liveReadingC.Id);
    }

    [Test]
    public void PipeLiveReadingsToDayReadingsSkipInsufficientTimestampsForLastReading()
    {
      // Arrange
      var dt = GetDateTime(28, 19, 31, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(1);
      var liveReading1 = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt };
      var liveReading2 = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt.AddHours(12.47) };
      DbContext.InsertReadings(liveReading1, liveReading2); // liveReading2 has insufficient time
      var target = CreateTarget(2);  // liveReading2 is the last of the potential piped.

      // Act
      target.PipeLiveReadingsToDayReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.DayReading>(), Is.EqualTo(1));
      AssertReading<Db.DayReading>(liveReading1.Label, liveReading1.DeviceId, liveReading1.Timestamp);
    }

    [Test]
    public void PipeLiveReadingsToDayReadingsIncludeMeterExchange()
    {
      // Arrange
      var hour = TimeSpan.FromMinutes(60);
      var dt = GetDateTime(28, 19, 25, 0);
      var maximumDateTime = dt+TimeSpan.FromDays(1);
      var liveReading1 = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt };
      var liveReading2 = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt+hour };
      var liveReading3 = new Db.LiveReading { Label="A", DeviceId="2", Timestamp=dt+hour+hour };
      DbContext.InsertReadings(liveReading1, liveReading2, liveReading3);
      var target = CreateTarget();

      // Act
      target.PipeLiveReadingsToDayReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.DayReading>(), Is.EqualTo(2));
      AssertReading<Db.DayReading>(liveReading2.Label, liveReading2.DeviceId, liveReading2.Timestamp);
      AssertReading<Db.DayReading>(liveReading3.Label, liveReading3.DeviceId, liveReading3.Timestamp);
    }

    [Test]
    public void PipeLiveReadingsToDayReadingsIncludesRegisters()
    {
      // Arrange
      var dt = GetDateTime(28, 19, 31, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(1);
      var liveReading = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(liveReading);
      var liveRegister1 = new Db.LiveRegister { ObisCode=(ObisCode)"1.1.1.1.1.1", ReadingId=liveReading.Id };
      var liveRegister2 = new Db.LiveRegister { ObisCode=(ObisCode)"2.2.2.2.2.2", ReadingId=liveReading.Id };
      DbContext.InsertRegisters(liveRegister1, liveRegister2);
      var target = CreateTarget();

      // Act
      target.PipeLiveReadingsToDayReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.DayReading>(), Is.EqualTo(1));
      AssertReading<Db.DayReading>(liveReading.Label, liveReading.DeviceId, liveReading.Timestamp);

      Assert.That(DbContext.GetCount<Db.DayRegister>(), Is.EqualTo(2));
      AssertRegister<Db.DayRegister>(liveRegister1.ObisCode, liveRegister1.ReadingId);
      AssertRegister<Db.DayRegister>(liveRegister2.ObisCode, liveRegister2.ReadingId);
    }

    [Test]
    public void PipeLiveReadingsToDayReadingsReducesToDayResolution()
    {
      // Arrange
      const string label = "A";
      var dt = GetDateTime(28, 19, 28, 11);
      var maximumDateTime = dt+TimeSpan.FromDays(1);
      var liveReading0 = new Db.LiveReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromHours(36) };
      var liveReading1 = new Db.LiveReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromHours(24) };
      var liveReading2 = new Db.LiveReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromHours(12) };
      var liveReading3 = new Db.LiveReading { Label=label, DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(liveReading0, liveReading1, liveReading2, liveReading3);
      var target = CreateTarget();

      // Act
      target.PipeLiveReadingsToDayReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.DayReading>(), Is.EqualTo(2));
      AssertReading<Db.DayReading>(liveReading1.Label, liveReading1.DeviceId, liveReading1.Timestamp);
      AssertReading<Db.DayReading>(liveReading3.Label, liveReading3.DeviceId, liveReading3.Timestamp);
      AssertStreamPosition("DayReading", label, liveReading3.Id);
    }

    [Test]
    public void PipeLiveReadingsToDayReadingsExcludesMaximumTimeStampLiveReadings()
    {
      // Arrange
      const string label = "A";
      var dt = GetDateTime(28, 19, 0, 0);
      var liveReading1 = new Db.LiveReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromDays(2) };
      var liveReading2 = new Db.LiveReading { Label=label, DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(liveReading1, liveReading2);
      var target = CreateTarget();

      // Act
      target.PipeLiveReadingsToDayReadings(dt);

      // Assert
      Assert.That(DbContext.GetCount<Db.DayReading>(), Is.EqualTo(1));
      AssertReading<Db.DayReading>(liveReading1.Label, liveReading1.DeviceId, liveReading1.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(1));
      AssertStreamPosition("DayReading", label, liveReading1.Id);
    }

    [Test]
    public void PipeLiveReadingsToDayReadingsExcludesLastDayReadings()
    {
      // Arrange
      var dt = GetDateTime(28, 19, 0, 0);
      var maximumDateTime = dt+TimeSpan.FromDays(2);
      var liveReadingA1 = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt };
      var liveReadingA2 = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(1) };
      var liveReadingB1 = new Db.LiveReading { Label="B", DeviceId="1", Timestamp=dt };
      var liveReadingB2 = new Db.LiveReading { Label="B", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(1) };
      DbContext.InsertReadings(liveReadingA1, liveReadingA2, liveReadingB1, liveReadingB2);
      var dayReadingA = new Db.DayReading { Label = "A", DeviceId="1", Timestamp = dt-TimeSpan.FromHours(4) };
      var dayReadingB = new Db.DayReading { Label = "B", DeviceId="1", Timestamp = dt-TimeSpan.FromHours(4) };
      DbContext.InsertReadings(dayReadingA, dayReadingB);
      var target = CreateTarget();

      // Act
      target.PipeLiveReadingsToDayReadings(maximumDateTime);

      // Assert
      AssertReading<Db.DayReading>(liveReadingA2.Label, liveReadingA2.DeviceId, liveReadingA2.Timestamp);
      AssertReading<Db.DayReading>(liveReadingB2.Label, liveReadingB2.DeviceId, liveReadingB2.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(2));
      AssertStreamPosition("DayReading", "A", liveReadingA2.Id);
      AssertStreamPosition("DayReading", "B", liveReadingB2.Id);
    }

    [Test]
    public void PipeLiveReadingsToDayReadingsContinueExistingStreamPosition()
    {
      // Arrange
      var streamPosition = new Db.StreamPosition { StreamName="DayReading", Label="A", Position=2 };
      InsertStreamPositions(streamPosition);
      var dt = GetDateTime(28, 19, 0, 0);
      var maximumDateTime = dt+TimeSpan.FromDays(4);
      var liveReading1 = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(1) };
      var liveReading2 = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(2) };
      var liveReading3 = new Db.LiveReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(3) };
      DbContext.InsertReadings(liveReading1, liveReading2, liveReading3);
      var target = CreateTarget();

      // Act
      target.PipeLiveReadingsToDayReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.DayReading>(), Is.EqualTo(1));
      AssertReading<Db.DayReading>(liveReading3.Label, liveReading3.DeviceId, liveReading3.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(1));
      AssertStreamPosition("DayReading", "A", liveReading3.Id);
    }

    [Test]
    public void PipeDayReadingsToMonthReadingsOneLabelReading()
    {
      // Arrange
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(32);
      var dayReading = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(dayReading);
      var target = CreateTarget();

      // Act
      var pipedSomething = target.PipeDayReadingsToMonthReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(1));
      AssertReading<Db.MonthReading>(dayReading.Label, dayReading.DeviceId, dayReading.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(1));
      AssertStreamPosition("MonthReading", dayReading.Label, dayReading.Id);

      Assert.That(pipedSomething, Is.True);
    }

    [Test]
    public void PipeDayReadingsToMonthReadingsMultipleLabelReading()
    {
      // Arrange
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(32);
      var dayReadingA = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt };
      var dayReadingB = new Db.DayReading { Label="B", DeviceId="1", Timestamp=dt };
      var dayReadingC = new Db.DayReading { Label="C", DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(dayReadingA, dayReadingB, dayReadingC);
      var target = CreateTarget();

      // Act
      target.PipeDayReadingsToMonthReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(3));
      AssertReading<Db.MonthReading>(dayReadingA.Label, dayReadingA.DeviceId, dayReadingA.Timestamp);
      AssertReading<Db.MonthReading>(dayReadingB.Label, dayReadingB.DeviceId, dayReadingB.Timestamp);
      AssertReading<Db.MonthReading>(dayReadingC.Label, dayReadingC.DeviceId, dayReadingC.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(3));
      AssertStreamPosition("MonthReading", dayReadingA.Label, dayReadingA.Id);
      AssertStreamPosition("MonthReading", dayReadingB.Label, dayReadingB.Id);
      AssertStreamPosition("MonthReading", dayReadingC.Label, dayReadingC.Id);
    }

    [Test]
    public void PipeDayReadingsToMonthReadingsSkipInsufficientTimestampsForLastReading()
    {
      // Arrange
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(1);
      var dayReading1 = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt };
      var dayReading2 = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt.AddDays(10.52) };
      DbContext.InsertReadings(dayReading1, dayReading2); // dayReading2 has insufficient time
      var target = CreateTarget(2);  // dayReading2 is the last of the potential piped.

      // Act
      target.PipeDayReadingsToMonthReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(1));
      AssertReading<Db.MonthReading>(dayReading1.Label, dayReading1.DeviceId, dayReading1.Timestamp);
    }

    [Test]
    public void PipeDayReadingsToMonthReadingsIncludeMeterExchange()
    {
      // Arrange
      var hour = TimeSpan.FromHours(1);
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(32);
      var dayReading1 = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt-hour-hour };
      var dayReading2 = new Db.DayReading { Label="A", DeviceId="2", Timestamp=dt-hour };
      var dayReading3 = new Db.DayReading { Label="A", DeviceId="2", Timestamp=dt };
      DbContext.InsertReadings(dayReading1, dayReading2, dayReading3);
      var target = CreateTarget();

      // Act
      target.PipeDayReadingsToMonthReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(3));
      AssertReading<Db.MonthReading>(dayReading1.Label, dayReading1.DeviceId, dayReading1.Timestamp);
      AssertReading<Db.MonthReading>(dayReading2.Label, dayReading2.DeviceId, dayReading2.Timestamp);
    }

    [Test]
    public void PipeDayReadingsToMonthReadingsIncludesRegisters()
    {
      // Arrange
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(32);
      var dayReading = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(dayReading);
      var dayRegister1 = new Db.DayRegister { ObisCode=(ObisCode)"1.1.1.1.1.1", ReadingId=dayReading.Id };
      var dayRegister2 = new Db.DayRegister { ObisCode=(ObisCode)"2.2.2.2.2.2", ReadingId=dayReading.Id };
      DbContext.InsertRegisters(dayRegister1, dayRegister2);
      var target = CreateTarget();

      // Act
      target.PipeDayReadingsToMonthReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(1));
      AssertReading<Db.MonthReading>(dayReading.Label, dayReading.DeviceId, dayReading.Timestamp);

      Assert.That(DbContext.GetCount<Db.MonthRegister>(), Is.EqualTo(2));
      AssertRegister<Db.MonthRegister>(dayRegister1.ObisCode, dayRegister1.ReadingId);
      AssertRegister<Db.MonthRegister>(dayRegister2.ObisCode, dayRegister2.ReadingId);
    }

    [Test]
    public void PipeDayReadingsToMonthReadingsReducesToMonthResolution()
    {
      // Arrange
      const string label = "A";
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(32);
      var dayReading0 = new Db.DayReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromDays(48) };
      var dayReading1 = new Db.DayReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromDays(32) };
      var dayReading2 = new Db.DayReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromDays(16) };
      var dayReading3 = new Db.DayReading { Label=label, DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(dayReading0, dayReading1, dayReading2, dayReading3);
      var target = CreateTarget();

      // Act
      target.PipeDayReadingsToMonthReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(2));
      AssertReading<Db.MonthReading>(dayReading1.Label, dayReading1.DeviceId, dayReading1.Timestamp);
      AssertReading<Db.MonthReading>(dayReading3.Label, dayReading3.DeviceId, dayReading3.Timestamp);
      AssertStreamPosition("MonthReading", label, dayReading3.Id);
    }

    [Test]
    public void PipeDayReadingsToMonthReadingsExcludesMaximumDateTimeReadings()
    {
      // Arrange
      const string label = "A";
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var dayReading1 = new Db.DayReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromDays(62) };
      var dayReading2 = new Db.DayReading { Label=label, DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(dayReading1, dayReading2);
      var target = CreateTarget();

      // Act
      target.PipeDayReadingsToMonthReadings(dt);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(1));
      AssertReading<Db.MonthReading>(dayReading1.Label, dayReading1.DeviceId, dayReading1.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(1));
      AssertStreamPosition("MonthReading", label, dayReading1.Id);
    }

    [Test]
    public void PipeDayReadingsToMonthReadingsExcludesLastMonthReadings()
    {
      // Arrange
      var dt = GetDateTime(11, 30, 19, 0, 0);
      var maximumDateTime = dt+TimeSpan.FromDays(63);
      var dayReadingA1 = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt };
      var dayReadingA2 = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(31) };
      var dayReadingB1 = new Db.DayReading { Label="B", DeviceId="1", Timestamp=dt };
      var dayReadingB2 = new Db.DayReading { Label="B", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(31) };
      DbContext.InsertReadings(dayReadingA1, dayReadingA2, dayReadingB1, dayReadingB2);
      var dayReadingA = new Db.DayReading { Label = "A", DeviceId="1", Timestamp = dt-TimeSpan.FromDays(4) };
      var dayReadingB = new Db.DayReading { Label = "B", DeviceId="1", Timestamp = dt-TimeSpan.FromDays(4) };
      DbContext.InsertReadings(dayReadingA, dayReadingB);
      var target = CreateTarget();

      // Act
      target.PipeDayReadingsToMonthReadings(maximumDateTime);

      // Assert
      AssertReading<Db.MonthReading>(dayReadingA2.Label, dayReadingA2.DeviceId, dayReadingA2.Timestamp);
      AssertReading<Db.MonthReading>(dayReadingB2.Label, dayReadingB2.DeviceId, dayReadingB2.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(2));
      AssertStreamPosition("MonthReading", "A", dayReadingA2.Id);
      AssertStreamPosition("MonthReading", "B", dayReadingB2.Id);
    }

    [Test]
    public void PipeDayReadingsToMonthReadingsContinueExistingStreamPosition()
    {
      // Arrange
      var streamPosition = new Db.StreamPosition { StreamName="MonthReading", Label="A", Position=2 };
      InsertStreamPositions(streamPosition);
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(124);
      var dayReading1 = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(30) };
      var dayReading2 = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(61) };
      var dayReading3 = new Db.DayReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(92) };
      DbContext.InsertReadings(dayReading1, dayReading2, dayReading3);
      var target = CreateTarget();

      // Act
      target.PipeDayReadingsToMonthReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(1));
      AssertReading<Db.MonthReading>(dayReading3.Label, dayReading3.DeviceId, dayReading3.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(1));
      AssertStreamPosition("MonthReading", "A", dayReading3.Id);
    }

    [Test]
    public void PipeMonthReadingsToYearReadingsOneLabelReading()
    {
      // Arrange
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(70); // some time the following year
      var monthReading = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(monthReading);
      var target = CreateTarget();

      // Act
      target.PipeMonthReadingsToYearReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(1));
      AssertReading<Db.YearReading>(monthReading.Label, monthReading.DeviceId, monthReading.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(1));
      AssertStreamPosition("YearReading", monthReading.Label, monthReading.Id);
    }

    [Test]
    public void PipeMonthReadingsToYearReadingsMultipleLabelReading()
    {
      // Arrange
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(70); // some time the following year
      var monthReadingA = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt };
      var monthReadingB = new Db.MonthReading { Label="B", DeviceId="1", Timestamp=dt };
      var monthReadingC = new Db.MonthReading { Label="C", DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(monthReadingA, monthReadingB, monthReadingC);
      var target = CreateTarget();

      // Act
      target.PipeMonthReadingsToYearReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.MonthReading>(), Is.EqualTo(3));
      AssertReading<Db.YearReading>(monthReadingA.Label, monthReadingA.DeviceId, monthReadingA.Timestamp);
      AssertReading<Db.YearReading>(monthReadingB.Label, monthReadingB.DeviceId, monthReadingB.Timestamp);
      AssertReading<Db.YearReading>(monthReadingC.Label, monthReadingC.DeviceId, monthReadingC.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(3));
      AssertStreamPosition("YearReading", monthReadingA.Label, monthReadingA.Id);
      AssertStreamPosition("YearReading", monthReadingB.Label, monthReadingB.Id);
      AssertStreamPosition("YearReading", monthReadingC.Label, monthReadingC.Id);
    }

    [Test]
    public void PipeMonthReadingsToYearReadingsSkipInsufficientTimestampsForLastReading()
    {
      // Arrange
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(365+180); // some time the following year
      var monthReading1 = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt };
      var monthReading2 = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt.AddDays(70) };
      DbContext.InsertReadings(monthReading1, monthReading2); // monthReading2 has insufficient time
      var target = CreateTarget(2);  // monthReading2 is the last of the potential piped.

      // Act
      target.PipeMonthReadingsToYearReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.YearReading>(), Is.EqualTo(1));
      AssertReading<Db.YearReading>(monthReading1.Label, monthReading1.DeviceId, monthReading1.Timestamp);
    }

    [Test]
    public void PipeMonthReadingsToYearReadingsIncludeMeterExchange()
    {
      // Arrange
      var hour = TimeSpan.FromHours(1);
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(70); // some time the following year
      var monthReading1 = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt-hour-hour };
      var monthReading2 = new Db.MonthReading { Label="A", DeviceId="2", Timestamp=dt-hour };
      var monthReading3 = new Db.MonthReading { Label="A", DeviceId="2", Timestamp=dt };
      DbContext.InsertReadings(monthReading1, monthReading2, monthReading3);
      var target = CreateTarget();

      // Act
      target.PipeMonthReadingsToYearReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.YearReading>(), Is.EqualTo(3));
      AssertReading<Db.YearReading>(monthReading1.Label, monthReading1.DeviceId, monthReading1.Timestamp);
      AssertReading<Db.YearReading>(monthReading2.Label, monthReading2.DeviceId, monthReading2.Timestamp);
    }

    [Test]
    public void PipeMonthReadingsToYearReadingsIncludesRegisters()
    {
      // Arrange
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(70); // some time the following year
      var monthReading = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(monthReading);
      var monthRegister1 = new Db.MonthRegister { ObisCode=(ObisCode)"1.1.1.1.1.1", ReadingId=monthReading.Id };
      var monthRegister2 = new Db.MonthRegister { ObisCode=(ObisCode)"2.2.2.2.2.2", ReadingId=monthReading.Id };
      DbContext.InsertRegisters(monthRegister1, monthRegister2);
      var target = CreateTarget();

      // Act
      target.PipeMonthReadingsToYearReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.YearReading>(), Is.EqualTo(1));
      AssertReading<Db.YearReading>(monthReading.Label, monthReading.DeviceId, monthReading.Timestamp);

      Assert.That(DbContext.GetCount<Db.YearRegister>(), Is.EqualTo(2));
      AssertRegister<Db.YearRegister>(monthRegister1.ObisCode, monthRegister1.ReadingId);
      AssertRegister<Db.YearRegister>(monthRegister2.ObisCode, monthRegister2.ReadingId);
    }

    [Test]
    public void PipeMonthReadingsToYearReadingsReducesToYearResolution()
    {
      // Arrange
      const string label = "A";
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(70); // some time the following year
      var monthReading0 = new Db.MonthReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromDays(545) };
      var monthReading1 = new Db.MonthReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromDays(365) };
      var monthReading2 = new Db.MonthReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromDays(180) };
      var monthReading3 = new Db.MonthReading { Label=label, DeviceId="1", Timestamp=dt };
      DbContext.InsertReadings(monthReading0, monthReading1, monthReading2, monthReading3);
      var target = CreateTarget();

      // Act
      target.PipeMonthReadingsToYearReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.YearReading>(), Is.EqualTo(2));
      AssertReading<Db.YearReading>(monthReading1.Label, monthReading1.DeviceId, monthReading1.Timestamp);
      AssertReading<Db.YearReading>(monthReading3.Label, monthReading3.DeviceId, monthReading3.Timestamp);
      AssertStreamPosition("YearReading", label, monthReading3.Id);
    }

    [Test]
    public void PipeMonthReadingsToYearReadingsExcludesMaximumDateTimeReadings()
    {
      // Arrange
      const string label = "A";
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var monthReading1 = new Db.MonthReading { Label=label, DeviceId="1", Timestamp=dt-TimeSpan.FromDays(365) };
      var monthReading2 = new Db.MonthReading { Label=label, DeviceId="1", Timestamp=dt.AddDays(50) };
      DbContext.InsertReadings(monthReading1, monthReading2);
      var target = CreateTarget();

      // Act
      target.PipeMonthReadingsToYearReadings(dt);

      // Assert
      Assert.That(DbContext.GetCount<Db.YearReading>(), Is.EqualTo(1));
      AssertReading<Db.YearReading>(monthReading1.Label, monthReading1.DeviceId, monthReading1.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(1));
      AssertStreamPosition("YearReading", label, monthReading1.Id);
    }

    [Test]
    public void PipeMonthReadingsToYearReadingsExcludesLastMonthReadings()
    {
      // Arrange
      var dt = GetDateTime(11, 30, 19, 0, 0);
      var maximumDateTime = dt+TimeSpan.FromDays(2*365);
      var monthReadingA1 = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt };
      var monthReadingA2 = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(365) };
      var monthReadingB1 = new Db.MonthReading { Label="B", DeviceId="1", Timestamp=dt };
      var monthReadingB2 = new Db.MonthReading { Label="B", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(365) };
      DbContext.InsertReadings(monthReadingA1, monthReadingA2, monthReadingB1, monthReadingB2);
      var monthReadingA = new Db.DayReading { Label = "A", DeviceId="1", Timestamp = dt-TimeSpan.FromDays(47) };
      var monthReadingB = new Db.DayReading { Label = "B", DeviceId="1", Timestamp = dt-TimeSpan.FromDays(47) };
      DbContext.InsertReadings(monthReadingA, monthReadingB);
      var target = CreateTarget();

      // Act
      target.PipeMonthReadingsToYearReadings(maximumDateTime);

      // Assert
      AssertReading<Db.YearReading>(monthReadingA2.Label, monthReadingA2.DeviceId, monthReadingA2.Timestamp);
      AssertReading<Db.YearReading>(monthReadingB2.Label, monthReadingB2.DeviceId, monthReadingB2.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(2));
      AssertStreamPosition("YearReading", "A", monthReadingA2.Id);
      AssertStreamPosition("YearReading", "B", monthReadingB2.Id);
    }

    [Test]
    public void PipeMonthReadingsToYearReadingsContinueExistingStreamPosition()
    {
      // Arrange
      var streamPosition = new Db.StreamPosition { StreamName="YearReading", Label="A", Position=2 };
      InsertStreamPositions(streamPosition);
      var dt = GetDateTime(10, 31, 22, 51, 43);
      var maximumDateTime = dt+TimeSpan.FromDays(4*365);
      var monthReading1 = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(365) };
      var monthReading2 = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(2*365) };
      var monthReading3 = new Db.MonthReading { Label="A", DeviceId="1", Timestamp=dt+TimeSpan.FromDays(3*365) };
      DbContext.InsertReadings(monthReading1, monthReading2, monthReading3);
      var target = CreateTarget();

      // Act
      target.PipeMonthReadingsToYearReadings(maximumDateTime);

      // Assert
      Assert.That(DbContext.GetCount<Db.YearReading>(), Is.EqualTo(1));
      AssertReading<Db.MonthReading>(monthReading3.Label, monthReading3.DeviceId, monthReading3.Timestamp);

      Assert.That(DbContext.GetCount<Db.StreamPosition>(), Is.EqualTo(1));
      AssertStreamPosition("YearReading", "A", monthReading3.Id);
    }

    private void AssertReading<TReading>(string label, string deviceId, DateTime timestamp)
      where TReading : class, IDbReading
    {
      var tableName = typeof(TReading).Name;
      var sql = "SELECT * FROM {0} WHERE Label=@label AND DeviceId=@deviceId AND Timestamp=@timestamp;";
      sql = string.Format(CultureInfo.InvariantCulture, sql, tableName);
      var result = DbContext.QueryTransaction<TReading>(sql, new { label, deviceId, timestamp });
      Assert.That(result.Count, Is.EqualTo(1));
    }

    private void AssertRegister<TRegister>(long obisCode, long readingId)
      where TRegister : class, IDbRegister
    {
      var tableName = typeof(TRegister).Name;
      var sql = "SELECT * FROM {0} WHERE ObisCode=@obisCode AND ReadingId=@readingId;";
      sql = string.Format(CultureInfo.InvariantCulture, sql, tableName);
      var result = DbContext.QueryTransaction<TRegister>(sql, new { obisCode, readingId });
      Assert.That(result.Count, Is.EqualTo(1));
    }

    private void AssertStreamPosition(string streamName, string label, long position)
    {
      var sql = "SELECT * FROM StreamPosition WHERE StreamName=@streamName AND Label=@label AND Position=@position;";
      var result = DbContext.QueryTransaction<Db.StreamPosition>(sql, new { streamName, label, position });
      Assert.That(result.Count, Is.EqualTo(1));
    }

    private ReadingPipeRepository CreateTarget(int readingsPerLabel = 5)
    {
      return new ReadingPipeRepository(DbContext, TimeZoneHelper.GetDenmarkLocationContext(), readingsPerLabel);
    }

    private static DateTime GetDateTime(int month, int day, int hour, int minute, int second)
    {
      return new DateTime(DateTime.Now.Year-1, month, day, hour, minute, second, DateTimeKind.Utc);
    }

    private static DateTime GetDateTime(int day, int hour, int minute, int second)
    {
      return GetDateTime(DateTime.Now.Month, day, hour, minute, second);
    }

    private void InsertStreamPositions(params Db.StreamPosition[] streamPositions)
    {
      DbContext.ExecuteTransaction("INSERT INTO StreamPosition (StreamName,Label,Position) VALUES (@StreamName,@Label,@Position);", streamPositions);
    }

  }
}
