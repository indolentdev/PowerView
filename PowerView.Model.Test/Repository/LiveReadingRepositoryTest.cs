using System;
using NUnit.Framework;
using DapperExtensions;
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
      Assert.That(DbContext.Connection.Count<Db.LiveReading>(null), Is.EqualTo(0));
      Assert.That(DbContext.Connection.Count<Db.LiveRegister>(null), Is.EqualTo(0));
    }

    [Test]
    public void AddOneLiveReadingWithOneRegister()
    {
      // Arrange
      var liveReading = new LiveReading("TheLabel", "1", DateTime.UtcNow, new [] { new RegisterValue("1.2.3.4.5.6", 10, -1, Unit.WattHour) });
      var target = CreateTarget();

      // Act
      target.Add(new[] { liveReading });

      // Assert
      Assert.That(DbContext.Connection.Count<Db.LiveReading>(null), Is.EqualTo(1));
      Assert.That(DbContext.Connection.Count<Db.LiveRegister>(null), Is.EqualTo(1));
    }

    [Test]
    public void AddTwoLiveReadingsWithTwoRegisters()
    {
      // Arrange
      var liveReading = new LiveReading("TheLabel", "1", DateTime.UtcNow, new [] { new RegisterValue("1.1.1.1.1.1", 10, 1, Unit.WattHour), new RegisterValue("11.11.11.11.11.11", 1010, 1, Unit.Watt) });
      var liveReading2 = new LiveReading("TheLabel2", "2", DateTime.UtcNow, new [] { new RegisterValue("2.2.2.2.2.2", 20, 2, Unit.WattHour), new RegisterValue("22.22.22.22.22.22", 2020, 2, Unit.Watt) });
      var target = CreateTarget();

      // Act
      target.Add(new [] { liveReading, liveReading2 });

      // Assert
      Assert.That(DbContext.Connection.Count<Db.LiveReading>(null), Is.EqualTo(2));
      Assert.That(DbContext.Connection.Count<Db.LiveRegister>(null), Is.EqualTo(4));
    }

    private LiveReadingRepository CreateTarget()
    {
      return new LiveReadingRepository(DbContext);
    }

  }
}
