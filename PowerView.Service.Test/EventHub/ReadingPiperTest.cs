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
    private Mock<IFactory> factory;
    private Mock<IReadingPipeRepository> readingPipeRepository;

    [SetUp]
    public void SetUp()
    {
      factory = new Mock<IFactory>();
      readingPipeRepository = new Mock<IReadingPipeRepository>();

      factory.Setup(f => f.Create<IReadingPipeRepository>()).Returns(() => new Owned<IReadingPipeRepository>(readingPipeRepository.Object, new System.IO.MemoryStream()));
    }

    [Test]
    public void PipeLiveReadings()
    {
      // Arrange
      var dateTime = new DateTime(2016, 6, 19, 14, 52, 31, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime - TimeSpan.FromDays(1));

      // Act
      target.PipeLiveReadings(dateTime);

      // Assert
      factory.Verify(f => f.Create<IReadingPipeRepository>());
      readingPipeRepository.Verify(rpr => rpr.PipeLiveReadingsToDayReadings(dateTime.ToUniversalTime()));
    }

    [Test]
    public void PipeLiveReadingsCanRepeat()
    {
      // Arrange
      var dateTime = new DateTime(2016, 6, 19, 14, 52, 31, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime - TimeSpan.FromDays(1));
      readingPipeRepository.Setup(rpr => rpr.PipeLiveReadingsToDayReadings(It.IsAny<DateTime>())).Returns(true);

      // Act
      target.PipeLiveReadings(dateTime);

      // Assert
      readingPipeRepository.Verify(rpr => rpr.PipeLiveReadingsToDayReadings(dateTime.ToUniversalTime()), Times.Exactly(10)) ;
    }

    [Test]
    public void PipeLiveReadingsSuccessiveBeforeInterval()
    {
      // Arrange
      var now = DateTime.Now;
      var target = CreateTarget(now);
      var dateTime = new DateTime(now.Year, now.Month, now.Day, 0, 44, 0, 0, now.Kind);

      // Act
      target.PipeLiveReadings(dateTime+TimeSpan.FromDays(1));
      target.PipeLiveReadings(dateTime+TimeSpan.FromDays(2));

      // Assert
      factory.Verify(f => f.Create<IReadingPipeRepository>(), Times.Once);
      readingPipeRepository.Verify(rpr => rpr.PipeLiveReadingsToDayReadings(It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public void PipeLiveReadingsSuccessiveAfterInterval()
    {
      // Arrange
      var now = DateTime.Now;
      var target = CreateTarget(now);
      var dateTime = new DateTime(now.Year, now.Month, now.Day, 0, 46, 0, 0, now.Kind);

      // Act
      target.PipeLiveReadings(dateTime+TimeSpan.FromDays(1));
      target.PipeLiveReadings(dateTime+TimeSpan.FromDays(2));

      // Assert
      factory.Verify(f => f.Create<IReadingPipeRepository>(), Times.Exactly(2));
      readingPipeRepository.Verify(rpr => rpr.PipeLiveReadingsToDayReadings(It.IsAny<DateTime>()), Times.Exactly(2));
    }

    [Test]
    public void PipeDayReadingsWithoutPriorPipeReadingsToHead()
    {
      // Arrange
      var dateTime = new DateTime(2016, 6, 19, 14, 52, 31, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime);

      // Act
      target.PipeDayReadings(dateTime.AddMonths(1));

      // Assert
      factory.Verify(f => f.Create<IReadingPipeRepository>(), Times.Never);
    }

    [Test]
    public void PipeDayReadings()
    {
      // Arrange
      var dateTime = new DateTime(2016, 6, 19, 14, 52, 31, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime);
      target.PipeLiveReadings(dateTime + TimeSpan.FromDays(1));

      // Act
      target.PipeDayReadings(dateTime.AddMonths(1));

      // Assert
      readingPipeRepository.Verify(rpr => rpr.PipeDayReadingsToMonthReadings(dateTime.AddMonths(1).ToUniversalTime()));
    }

    [Test]
    public void PipeDayReadingsCanRepeat()
    {
      // Arrange
      var dateTime = new DateTime(2016, 6, 19, 14, 52, 31, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime);
      target.PipeLiveReadings(dateTime + TimeSpan.FromDays(1));
      readingPipeRepository.Setup(rpr => rpr.PipeDayReadingsToMonthReadings(It.IsAny<DateTime>())).Returns(true);

      // Act
      target.PipeDayReadings(dateTime.AddMonths(1));

      // Assert
      readingPipeRepository.Verify(rpr => rpr.PipeDayReadingsToMonthReadings(dateTime.AddMonths(1).ToUniversalTime()), Times.Exactly(2));
    }

    [Test]
    public void PipeDayReadingsBeforeInterval()
    {
      const DateTimeKind dtk = DateTimeKind.Local;
      var timePairs = new [] 
      { 
        new { Construct=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk), Invoke=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk) }, 
        new { Construct=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk), Invoke=new DateTime(2016, 6, 19, 23, 59, 59, 0, dtk) }, 
      };

      foreach (var item in timePairs)
      {
        // Arrange
        var target = CreateTarget(item.Construct);
        target.PipeLiveReadings(item.Construct + TimeSpan.FromDays(1));

        // Act
        target.PipeDayReadings(item.Invoke);

        // Assert
        readingPipeRepository.Verify(rpr => rpr.PipeDayReadingsToMonthReadings(It.IsAny<DateTime>()), Times.Never);
      }
    }

    [Test]
    public void PipeDayReadingsAfterInterval()
    {
      const DateTimeKind dtk = DateTimeKind.Local;
      var timePairs = new [] 
      { 
        new { Construct=new DateTime(2016, 6, 30, 23, 59, 59, 0, dtk), Invoke=new DateTime(2016, 7, 1, 0, 46, 00, 0, dtk) }, 
        new { Construct=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk), Invoke=new DateTime(2016, 7, 1, 0, 46, 00, 0, dtk) }, 
        new { Construct=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk), Invoke=new DateTime(2016, 8, 1, 0, 46, 00, 0, dtk) }, 
        new { Construct=new DateTime(2016, 7, 1, 0, 0, 1, 0, dtk), Invoke=new DateTime(2016, 7, 1, 0, 44, 59, 0, dtk) },  // Ideally this wouldn't fire..
      };

      foreach (var item in timePairs)
      {
        // Arrange
        var target = CreateTarget(item.Construct);
        target.PipeLiveReadings(item.Construct + TimeSpan.FromDays(1));

        // Act
        target.PipeDayReadings(item.Invoke);

        // Assert
        readingPipeRepository.Verify(rpr => rpr.PipeDayReadingsToMonthReadings(It.IsAny<DateTime>()));
      }
    }

    [Test]
    public void PipeMonthReadingsWithoutPriorPipeReadingsToHead()
    {
      // Arrange
      var dateTime = new DateTime(2016, 6, 19, 14, 52, 31, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime);

      // Act
      target.PipeMonthReadings(dateTime.AddMonths(1));

      // Assert
      factory.Verify(f => f.Create<IReadingPipeRepository>(), Times.Never);
    }

    [Test]
    public void PipeMonthReadings()
    {
      // Arrange
      var dateTime = new DateTime(2016, 6, 19, 14, 52, 31, 0, DateTimeKind.Local);
      var target = CreateTarget(dateTime);
      target.PipeLiveReadings(dateTime + TimeSpan.FromDays(1));
      target.PipeDayReadings(dateTime.AddMonths(1));

      // Act
      target.PipeMonthReadings(dateTime.AddMonths(1));

      // Assert
      readingPipeRepository.Verify(rpr => rpr.PipeMonthReadingsToYearReadings(dateTime.AddMonths(1).ToUniversalTime()));
    }

    [Test]
    public void PipeMonthReadingsBeforeInterval()
    {
      const DateTimeKind dtk = DateTimeKind.Local;
      var timePairs = new[]
      {
        new { Construct=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk), Invoke=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk) },
        new { Construct=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk), Invoke=new DateTime(2016, 6, 19, 23, 59, 59, 0, dtk) },
      };

      foreach (var item in timePairs)
      {
        // Arrange
        var target = CreateTarget(item.Construct);
        target.PipeLiveReadings(item.Construct + TimeSpan.FromDays(1));
        target.PipeDayReadings(item.Construct.AddMonths(1));

        // Act
        target.PipeMonthReadings(item.Invoke);

        // Assert
        readingPipeRepository.Verify(rpr => rpr.PipeMonthReadingsToYearReadings(It.IsAny<DateTime>()), Times.Never);
      }
    }

    [Test]
    public void PipeMonthReadingsAfterInterval()
    {
      const DateTimeKind dtk = DateTimeKind.Local;
      var timePairs = new[]
      {
        new { Construct=new DateTime(2016, 6, 30, 23, 59, 59, 0, dtk), Invoke=new DateTime(2016, 7, 1, 0, 46, 00, 0, dtk) },
        new { Construct=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk), Invoke=new DateTime(2016, 7, 1, 0, 46, 00, 0, dtk) },
        new { Construct=new DateTime(2016, 6, 19, 14, 52, 31, 0, dtk), Invoke=new DateTime(2016, 8, 1, 0, 46, 00, 0, dtk) },
        new { Construct=new DateTime(2016, 7, 1, 0, 0, 1, 0, dtk), Invoke=new DateTime(2016, 7, 1, 0, 44, 59, 0, dtk) },  // Ideally this wouldn't fire..
      };

      foreach (var item in timePairs)
      {
        // Arrange
        var target = CreateTarget(item.Construct);
        target.PipeLiveReadings(item.Construct + TimeSpan.FromDays(1));
        target.PipeDayReadings(item.Construct.AddMonths(1));

        // Act
        target.PipeMonthReadings(item.Invoke);

        // Assert
        readingPipeRepository.Verify(rpr => rpr.PipeMonthReadingsToYearReadings(It.IsAny<DateTime>()));
      }
    }

    private ReadingPiper CreateTarget(DateTime dateTime)
    {
      return new ReadingPiper(factory.Object, dateTime);
    }
  }
}
