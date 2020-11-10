using System;
using Autofac.Features.OwnedInstances;
using NUnit.Framework;
using Moq;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;

namespace PowerView.Service.Test.EventHub
{
  [TestFixture]
  public class ReadingPiperTest
  {
    private Mock<IIntervalTrigger> dayTrigger;
    private Mock<IIntervalTrigger> monthTrigger;
    private Mock<IIntervalTrigger> yearTrigger;
    private Mock<IFactory> factory;
    private Mock<IReadingPipeRepository> readingPipeRepository;

    [SetUp]
    public void SetUp()
    {
      dayTrigger = new Mock<IIntervalTrigger>();
      monthTrigger = new Mock<IIntervalTrigger>();
      yearTrigger = new Mock<IIntervalTrigger>();
      factory = new Mock<IFactory>();
      readingPipeRepository = new Mock<IReadingPipeRepository>();

      factory.Setup(f => f.Create<IReadingPipeRepository>()).Returns(() => new Owned<IReadingPipeRepository>(readingPipeRepository.Object, new System.IO.MemoryStream()));
    }

    [Test]
    public void Constructor()
    {
      // Arrange

      // Act
      var target = CreateTarget();

      // Assert
      dayTrigger.Verify(dt => dt.Setup(new TimeSpan(0, 45, 0), TimeSpan.FromDays(1)));
      monthTrigger.Verify(mt => mt.Setup(new TimeSpan(0, 45, 0), TimeSpan.FromDays(1)));
      yearTrigger.Verify(yt => yt.Setup(new TimeSpan(0, 45, 0), TimeSpan.FromDays(1)));
    }

    [Test]
    public void PipeLiveReadings()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();
      dayTrigger.Setup(dt => dt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);

      // Act
      target.PipeLiveReadings(dateTime);

      // Assert
      dayTrigger.Verify(dt => dt.IsTriggerTime(dateTime));
      factory.Verify(f => f.Create<IReadingPipeRepository>());
      readingPipeRepository.Verify(rpr => rpr.PipeLiveReadingsToDayReadings(dateTime));
      dayTrigger.Verify(dt => dt.Advance(dateTime));
    }

    [Test]
    public void PipeLiveReadingsNoTrigger()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();
      dayTrigger.Setup(dt => dt.IsTriggerTime(It.IsAny<DateTime>())).Returns(false);

      // Act
      target.PipeLiveReadings(dateTime);

      // Assert
      dayTrigger.Verify(dt => dt.IsTriggerTime(dateTime));
      factory.Verify(f => f.Create<IReadingPipeRepository>(), Times.Never);
      dayTrigger.Verify(dt => dt.Advance(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void PipeLiveReadingsCanRepeat()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();
      dayTrigger.Setup(dt => dt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      readingPipeRepository.Setup(rpr => rpr.PipeLiveReadingsToDayReadings(It.IsAny<DateTime>())).Returns(true);

      // Act
      target.PipeLiveReadings(dateTime);

      // Assert
      readingPipeRepository.Verify(rpr => rpr.PipeLiveReadingsToDayReadings(dateTime), Times.Exactly(50)) ;
    }

    [Test]
    public void PipeDayReadingsWithoutPriorPipeReadingsToHead()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();

      // Act
      target.PipeDayReadings(dateTime);

      // Assert
      monthTrigger.Verify(mt => mt.IsTriggerTime(It.IsAny<DateTime>()), Times.Never );
      factory.Verify(f => f.Create<IReadingPipeRepository>(), Times.Never);
    }

    [Test]
    public void PipeDayReadings()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();
      dayTrigger.Setup(dt => dt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      target.PipeLiveReadings(dateTime);
      monthTrigger.Setup(mt => mt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);

      // Act
      target.PipeDayReadings(dateTime);

      // Assert
      monthTrigger.Verify(mt => mt.IsTriggerTime(dateTime));
      readingPipeRepository.Verify(rpr => rpr.PipeDayReadingsToMonthReadings(dateTime));
      monthTrigger.Verify(mt => mt.Advance(dateTime));
    }

    [Test]
    public void PipeDayReadingsNoTrigger()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();
      dayTrigger.Setup(dt => dt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      target.PipeLiveReadings(dateTime);
      monthTrigger.Setup(mt => mt.IsTriggerTime(It.IsAny<DateTime>())).Returns(false);

      // Act
      target.PipeDayReadings(dateTime);

      // Assert
      monthTrigger.Verify(mt => mt.IsTriggerTime(dateTime));
      readingPipeRepository.Verify(rpr => rpr.PipeDayReadingsToMonthReadings(It.IsAny<DateTime>()), Times.Never);
      monthTrigger.Verify(mt => mt.Advance(dateTime), Times.Never);
    }

    [Test]
    public void PipeDayReadingsCanRepeat()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();
      dayTrigger.Setup(dt => dt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      target.PipeLiveReadings(dateTime);
      monthTrigger.Setup(mt => mt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      readingPipeRepository.Setup(rpr => rpr.PipeDayReadingsToMonthReadings(It.IsAny<DateTime>())).Returns(true);

      // Act
      target.PipeDayReadings(dateTime);

      // Assert
      readingPipeRepository.Verify(rpr => rpr.PipeDayReadingsToMonthReadings(dateTime), Times.Exactly(13));
    }

    [Test]
    public void PipeMonthReadingsWithoutPriorPipeReadingsToHead()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();

      // Act
      target.PipeMonthReadings(dateTime.AddMonths(1));

      // Assert
      yearTrigger.Verify(yt => yt.IsTriggerTime(It.IsAny<DateTime>()), Times.Never);
      factory.Verify(f => f.Create<IReadingPipeRepository>(), Times.Never);
    }

    [Test]
    public void PipeMonthReadings()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();
      dayTrigger.Setup(dt => dt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      target.PipeLiveReadings(dateTime);
      monthTrigger.Setup(mt => mt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      target.PipeDayReadings(dateTime);
      yearTrigger.Setup(yt => yt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);

      // Act
      target.PipeMonthReadings(dateTime);

      // Assert
      yearTrigger.Verify(yt => yt.IsTriggerTime(dateTime));
      readingPipeRepository.Verify(rpr => rpr.PipeMonthReadingsToYearReadings(dateTime));
      yearTrigger.Verify(yt => yt.Advance(dateTime));
    }

    [Test]
    public void PipeMonthReadingsNoTrigger()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();
      dayTrigger.Setup(dt => dt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      target.PipeLiveReadings(dateTime);
      monthTrigger.Setup(mt => mt.IsTriggerTime(It.IsAny<DateTime>())).Returns(true);
      target.PipeDayReadings(dateTime);
      yearTrigger.Setup(yt => yt.IsTriggerTime(It.IsAny<DateTime>())).Returns(false);

      // Act
      target.PipeMonthReadings(dateTime);

      // Assert
      yearTrigger.Verify(yt => yt.IsTriggerTime(dateTime));
      readingPipeRepository.Verify(rpr => rpr.PipeMonthReadingsToYearReadings(It.IsAny<DateTime>()), Times.Never);
      yearTrigger.Verify(yt => yt.Advance(It.IsAny<DateTime>()), Times.Never);
    }

    private ReadingPiper CreateTarget()
    {
      return new ReadingPiper(dayTrigger.Object, monthTrigger.Object, yearTrigger.Object, factory.Object);
    }
  }
}
