using System;
using System.Linq;
using NUnit.Framework;
using DapperExtensions;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class MeterEventRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new MeterEventRepository(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetLatestMeterEventsByLabel()
    {
      // Arrange
      var target = CreateTarget();
      var now = new DateTime(2016, 12, 30, 23, 33, 0, DateTimeKind.Utc);
      var amp = new LeakMeterEventAmplification(now, now, new UnitValue(1.1, Unit.CubicMetre));
      InsertMeterEvent("Lbl1", now, true, amp);
      InsertMeterEvent("Lbl1", now.AddDays(1), false, amp);
      InsertMeterEvent("Lbl2", now.AddDays(2), true, amp);
      var amp3 = new LeakMeterEventAmplification(now, now, new UnitValue(3.3, Unit.CubicMetre));
      InsertMeterEvent("Lbl2", now.AddDays(3), false, amp3);
      var amp4 = new LeakMeterEventAmplification(now, now, new UnitValue(4.4, Unit.CubicMetre));
      InsertMeterEvent("Lbl1", now.AddDays(4), true, amp4);

      // Act
      var meterEvents = target.GetLatestMeterEventsByLabel();

      // Assert
      Assert.That(meterEvents, Has.Count.EqualTo(2));
      AssertMeterEvent("Lbl2", now.AddDays(3), false, amp3, meterEvents.First());
      AssertMeterEvent("Lbl1", now.AddDays(4), true, amp4, meterEvents.Last());
    }

    [Test]
    public void GetMeterEvents()
    {
      // Arrange
      var target = CreateTarget();
      var now = new DateTime(2016, 12, 30, 23, 33, 0, DateTimeKind.Utc);
      var amp = new LeakMeterEventAmplification(now, now, new UnitValue(1.1, Unit.CubicMetre));
      InsertMeterEvent("Lbl1", now, true, amp);
      InsertMeterEvent("Lbl1", now.AddDays(1), false, amp);
      InsertMeterEvent("Lbl2", now.AddDays(2), true, amp);
      var amp3 = new LeakMeterEventAmplification(now, now, new UnitValue(3.3, Unit.CubicMetre));
      InsertMeterEvent("Lbl2", now.AddDays(3), false, amp3);
      var amp4 = new LeakMeterEventAmplification(now, now, new UnitValue(4.4, Unit.CubicMetre));
      InsertMeterEvent("Lbl1", now.AddDays(4), true, amp4);
      InsertMeterEvent("Lbl1", now.AddDays(5), false, amp);
      InsertMeterEvent("Lbl1", now.AddDays(6), true, amp4);
      InsertMeterEvent("Lbl2", now.AddDays(7), true, amp);
      InsertMeterEvent("Lbl2", now.AddDays(8), false, amp);
      InsertMeterEvent("Lbl1", now.AddDays(9), false, amp);
      InsertMeterEvent("Lbl1", now.AddDays(10), true, amp4);

      // Act & Assert
      var meterEvents = target.GetMeterEvents(0, 1);
      Assert.That(meterEvents.TotalCount, Is.EqualTo(11));
      Assert.That(meterEvents.Result.Count, Is.EqualTo(1));
      Assert.That(meterEvents.Result.First().DetectTimestamp, Is.EqualTo(now.AddDays(10)));

      meterEvents = target.GetMeterEvents(1, 2);
      Assert.That(meterEvents.TotalCount, Is.EqualTo(11));
      Assert.That(meterEvents.Result.Count, Is.EqualTo(2));
      Assert.That(meterEvents.Result.First().DetectTimestamp, Is.EqualTo(now.AddDays(9)));
      Assert.That(meterEvents.Result.Last().DetectTimestamp, Is.EqualTo(now.AddDays(8)));
    }

    [Test]
    public void AddMeterEventsThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddMeterEvents(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void AddMeterEvents()
    {
      // Arrange
      var target = CreateTarget();
      var now = DateTime.UtcNow;
      var amplification1 = new LeakMeterEventAmplification(now, now, new UnitValue(1, Unit.Watt));
      var meterEvent1 = new MeterEvent("Lable1", DateTime.UtcNow.AddHours(1), false, amplification1);
      var amplification2 = new LeakMeterEventAmplification(now, now, new UnitValue(2, Unit.Watt));
      var meterEvent2 = new MeterEvent("Lable2", DateTime.UtcNow, true, amplification2);

      // Act
      target.AddMeterEvents(new[] { meterEvent1, meterEvent2 });

      // Assert
      AssertMeterEventExists(meterEvent1);
      AssertMeterEventExists(meterEvent2);
    }

    [Test]
    public void GetMaxFlaggedMeterEventIdWhenNoMeterEvents()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var maxMeterEventId = target.GetMaxFlaggedMeterEventId();

      // Assert
      Assert.That(maxMeterEventId, Is.Null);
    }

    [Test]
    public void GetMaxFlaggedMeterEventId()
    {
      // Arrange
      var target = CreateTarget();
      var now = new DateTime(2016, 12, 30, 23, 33, 0, DateTimeKind.Utc);
      var amp = new LeakMeterEventAmplification(now, now, new UnitValue(1.1, Unit.CubicMetre));
      InsertMeterEvent("Lbl1", now, false, amp);
      var dbMeterEvent = InsertMeterEvent("Lbl1", now.AddDays(1), true, amp);
      InsertMeterEvent("Lbl1", now.AddDays(2), false, amp);

      // Act
      var maxMeterEventId = target.GetMaxFlaggedMeterEventId();

      // Assert
      Assert.That(maxMeterEventId, Is.EqualTo(dbMeterEvent.Id));
    }

    private void AssertMeterEventExists(MeterEvent meterEvent, bool not = false)
    {
      var pLabel = Predicates.Field<Db.MeterEvent>(me => me.Label, Operator.Eq, meterEvent.Label);
      var pMeterEventId = Predicates.Field<Db.MeterEvent>(me => me.MeterEventType, Operator.Eq, meterEvent.Amplification.GetMeterEventType());
      var pDetectTimestamp = Predicates.Field<Db.MeterEvent>(me => me.DetectTimestamp, Operator.Eq, meterEvent.DetectTimestamp);
      var pValue = Predicates.Field<Db.MeterEvent>(me => me.Flag, Operator.Eq, meterEvent.Flag);
      var pAmplification = Predicates.Field<Db.MeterEvent>(me => me.Amplification, Operator.Eq, MeterEventAmplificationSerializer.Serialize(meterEvent.Amplification));

      var predicate = Predicates.Group(GroupOperator.And, pLabel, pMeterEventId, pDetectTimestamp, pValue, pAmplification);
      Assert.That(DbContext.Connection.Count<Db.MeterEvent>(predicate), Is.EqualTo(not ? 0 : 1));
    }

    private Db.MeterEvent InsertMeterEvent(string label, DateTime detectTimestamp, bool flag, IMeterEventAmplification amplification)
    {
      var ampString = MeterEventAmplificationSerializer.Serialize(amplification);
      var dbMeterEvent = new Db.MeterEvent
      {
        Label = label, MeterEventType = amplification.GetMeterEventType(),
        DetectTimestamp = detectTimestamp, Flag = flag, Amplification = ampString
      };

      DbContext.Connection.Insert(dbMeterEvent);

      return dbMeterEvent;
    }

    private static void AssertMeterEvent(string label, DateTime detectTimestamp, bool value, IMeterEventAmplification amplification, MeterEvent actual)
    {
      Assert.That(actual.Label, Is.EqualTo(label));
      Assert.That(actual.DetectTimestamp, Is.EqualTo(detectTimestamp));
      Assert.That(actual.Flag, Is.EqualTo(value));
      Assert.That(actual.Amplification, Is.TypeOf(amplification.GetType()));
      Assert.That(((LeakMeterEventAmplification)actual.Amplification).UnitValue.Value, Is.EqualTo(((LeakMeterEventAmplification)amplification).UnitValue.Value));
    }

    private MeterEventRepository CreateTarget()
    {
      return new MeterEventRepository(DbContext);
    }

  }
}
