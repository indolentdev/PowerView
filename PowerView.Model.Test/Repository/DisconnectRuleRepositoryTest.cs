using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DapperExtensions;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class DisconnectRuleRepositoryTest : DbTestFixtureWithSchema
  {
    [SetUp]
    public override void SetUp()
    {
      base.SetUp();
    }
    
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new DisconnectRuleRepository(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void AddDisconnectRuleThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddDisconnectRule(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void AddDisconnectRuleInserts()
    {
      // Arrange
      var sn1 = new SerieName("label1", ObisCode.ColdWaterFlow1);
      var sn2 = new SerieName("label2", ObisCode.ActualPowerP14);
      var disconnectRule = new DisconnectRule(sn1, sn2, TimeSpan.FromMinutes(30), 1500, 200, Unit.Watt);
      var target = CreateTarget();

      // Act
      target.AddDisconnectRule(disconnectRule);

      // Assert
      AssertDisconnectRuleExists(disconnectRule);
    }

    [Test]
    public void AddDisconnectRuleUpdates()
    {
      // Arrange
      var sn1 = new SerieName("label1", ObisCode.ColdWaterFlow1);

      var dbDisconnectRule = new Db.DisconnectRule { Label = sn1.Label, ObisCode = sn1.ObisCode, EvaluationLabel = sn1.Label, EvaluationObisCode = sn1.ObisCode,
        DurationSeconds = 4, DisconnectToConnectValue = 4, ConnectToDisconnectValue = 3, Unit = 4 };
      DbContext.InsertTransaction("testdata", dbDisconnectRule);

      var sn2 = new SerieName("label2", ObisCode.ActualPowerP14);
      var disconnectRule = new DisconnectRule(sn1, sn2, TimeSpan.FromMinutes(30), 1500, 200, Unit.Watt);
      var target = CreateTarget();

      // Act
      target.AddDisconnectRule(disconnectRule);

      // Assert
      AssertDisconnectRuleExists(disconnectRule);
    }

    private void AssertDisconnectRuleExists(DisconnectRule disconnectRule, bool not = false)
    {
      var predicateLabel = Predicates.Field<Db.DisconnectRule>(x => x.Label, Operator.Eq, disconnectRule.Name.Label);
      var predicateObisCode = Predicates.Field<Db.DisconnectRule>(x => x.ObisCode, Operator.Eq, (long)disconnectRule.Name.ObisCode);
      var predicateEvalLabel = Predicates.Field<Db.DisconnectRule>(x => x.EvaluationLabel, Operator.Eq, disconnectRule.EvaluationName.Label);
      var predicateEvalObisCode = Predicates.Field<Db.DisconnectRule>(x => x.EvaluationObisCode, Operator.Eq, (long)disconnectRule.EvaluationName.ObisCode);
      var predicateDuration = Predicates.Field<Db.DisconnectRule>(x => x.DurationSeconds, Operator.Eq, (int)disconnectRule.Duration.TotalSeconds);
      var predicateDtcValue = Predicates.Field<Db.DisconnectRule>(x => x.DisconnectToConnectValue, Operator.Eq, disconnectRule.DisconnectToConnectValue);
      var predicateCtdValue = Predicates.Field<Db.DisconnectRule>(x => x.ConnectToDisconnectValue, Operator.Eq, disconnectRule.ConnectToDisconnectValue);
      var predicateUnit = Predicates.Field<Db.DisconnectRule>(x => x.Unit, Operator.Eq, (byte)disconnectRule.Unit);

      var predicate = Predicates.Group(GroupOperator.And, predicateObisCode, predicateLabel, predicateEvalObisCode, predicateEvalLabel,
                                       predicateDuration, predicateDtcValue, predicateCtdValue, predicateUnit);
      var disconnectRulesDb = DbContext.GetPage<Db.DisconnectRule>("", 0, 10, predicate);
      Assert.That(disconnectRulesDb.Count, Is.EqualTo(not ? 0 : 1));
    }

    [Test]
    public void DeleteDisconnectRuleThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.DeleteDisconnectRule(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void DeleteDisconnectRuleAbsent()
    {
      // Arrange
      var sn = new SerieName("label", "0.1.96.3.10.255");
      var target = CreateTarget();

      // Act
      target.DeleteDisconnectRule(sn);

      // Assert
      // No exeptions
    }

    [Test]
    public void DeleteDisconnectRule()
    {
      // Arrange
      var dbDr1 = new Db.DisconnectRule { Label = "lbl1", ObisCode = (ObisCode)"0.1.96.3.10.255", EvaluationLabel = "lbl11", EvaluationObisCode = 11,
        DurationSeconds = 1111, DisconnectToConnectValue = 11, ConnectToDisconnectValue = 1, Unit = 1 };
      var dbDr2 = new Db.DisconnectRule { Label = "lbl1", ObisCode = 2, EvaluationLabel = "lbl22", EvaluationObisCode = 22,
        DurationSeconds = 2222, DisconnectToConnectValue = 22, ConnectToDisconnectValue = 2, Unit = 2 };
      var dbDr3 = new Db.DisconnectRule { Label = "lbl2", ObisCode = (ObisCode)"0.1.96.3.10.255", EvaluationLabel = "lbl33", EvaluationObisCode = 33,
        DurationSeconds = 3333, DisconnectToConnectValue = 33, ConnectToDisconnectValue = 3, Unit = 3 };

      DbContext.InsertTransaction("", (IEnumerable<Db.DisconnectRule>)new[] { dbDr1, dbDr2 });
      var sn = new SerieName("lbl1", "0.1.96.3.10.255");
      var target = CreateTarget();

      // Act
      target.DeleteDisconnectRule(sn);

      // Assert
      var predicateLabel = Predicates.Field<Db.DisconnectRule>(x => x.Label, Operator.Eq, sn.Label);
      var predicateObisCode = Predicates.Field<Db.DisconnectRule>(x => x.ObisCode, Operator.Eq, (long)sn.ObisCode);
      var predicate = Predicates.Group(GroupOperator.And, predicateObisCode, predicateLabel);
      var disconnectRulesDb = DbContext.GetPage<Db.DisconnectRule>("", 0, 10, predicate);
      Assert.That(disconnectRulesDb.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetDisconnectRules()
    {
      // Arrange
      var dbDr1 = new Db.DisconnectRule { Label = "lbl1", ObisCode = 1, EvaluationLabel = "lbl11", EvaluationObisCode = 11,
        DurationSeconds = 1111, DisconnectToConnectValue = 11, ConnectToDisconnectValue = 1, Unit = 1 };
      var dbDr2 = new Db.DisconnectRule { Label = "lbl2", ObisCode = 2, EvaluationLabel = "lbl22", EvaluationObisCode = 22,
        DurationSeconds = 2222, DisconnectToConnectValue = 22, ConnectToDisconnectValue = 2, Unit = 2 };

      DbContext.InsertTransaction("", (IEnumerable<Db.DisconnectRule>)new[] { dbDr1, dbDr2 });

      var target = CreateTarget();

      // Act
      var disconnectRules = target.GetDisconnectRules();

      // Assert
      Assert.That(disconnectRules.Count, Is.EqualTo(2));
      AssertDisconnectRule(dbDr1, disconnectRules.First());
      AssertDisconnectRule(dbDr2, disconnectRules.Last());
    }

    [Test]
    public void GetLatestSerieNamesThrows()
    {
      // Arrange
      var dateTime = DateTime.Now;
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.GetLatestSerieNames(dateTime), Throws.TypeOf<ArgumentOutOfRangeException>());
    }


    [Test]
    public void GetLatestSerieNamesFromLive()
    {
      // Arrange
      var reading1 = new Db.LiveReading { Label = "lbl", SerialNumber = "SN", Timestamp = DateTime.UtcNow };
      var reading2 = new Db.LiveReading { Label = "lbl-other", SerialNumber = "SN-other", Timestamp = DateTime.UtcNow - TimeSpan.FromDays(3)};
      var reading3 = new Db.LiveReading { Label = "lbl", SerialNumber = "SN", Timestamp = DateTime.UtcNow - TimeSpan.FromDays(1)};
      Insert(new [] { reading1, reading2, reading3 });
      var register1 = new Db.LiveRegister { ObisCode = ObisCode.ActualPowerP23L2, Unit = (byte)Unit.Watt, ReadingId = reading1.Id };
      var register2 = new Db.LiveRegister { ObisCode = ObisCode.ActualPowerP23L2, Unit = (byte)Unit.Joule, ReadingId = reading2.Id };
      var register3 = new Db.LiveRegister { ObisCode = ObisCode.ActualPowerP23L2, Unit = (byte)Unit.Percentage, ReadingId = reading3.Id };
      Insert(new[] { register1, register2, register3 });
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();

      // Act
      var res = target.GetLatestSerieNames(dateTime);

      // Assert
      Assert.That(res.Count, Is.EqualTo(1));
      Assert.That(res, Contains.Key(new SerieName("lbl", ObisCode.ActualPowerP23L2)));
      Assert.That(res.First().Value, Is.EqualTo(Unit.Watt));
    }

    [Test]
    public void GetLatestSerieNamesFromDay()
    {
      // Arrange
      var reading1 = new Db.DayReading { Label = "lbl", SerialNumber = "SN", Timestamp = DateTime.UtcNow - TimeSpan.FromDays(4) };
      var reading2 = new Db.DayReading { Label = "lbl-other", SerialNumber = "SN-other", Timestamp = DateTime.UtcNow - TimeSpan.FromDays(8) };
      var reading3 = new Db.DayReading { Label = "lbl", SerialNumber = "SN", Timestamp = DateTime.UtcNow - TimeSpan.FromDays(6) };
      Insert(new[] { reading1, reading2, reading3 });
      var register1 = new Db.DayRegister { ObisCode = ObisCode.ActualPowerP23L2, Unit = (byte)Unit.Watt, ReadingId = reading1.Id };
      var register2 = new Db.DayRegister { ObisCode = ObisCode.ActualPowerP23L2, Unit = (byte)Unit.Joule, ReadingId = reading2.Id };
      var register3 = new Db.DayRegister { ObisCode = ObisCode.ActualPowerP23L2, Unit = (byte)Unit.Percentage, ReadingId = reading3.Id };
      Insert(new[] { register1, register2, register3 });
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();

      // Act
      var res = target.GetLatestSerieNames(dateTime);

      // Assert
      Assert.That(res.Count, Is.EqualTo(1));
      Assert.That(res, Contains.Key(new SerieName("lbl", ObisCode.ActualPowerP23L2)));
      Assert.That(res.First().Value, Is.EqualTo(Unit.Watt));
    }

    private static void AssertDisconnectRule(Db.DisconnectRule dbDisconnectRule, IDisconnectRule disconnectRule)
    {
      Assert.That(disconnectRule.Name, Is.EqualTo(new SerieName(dbDisconnectRule.Label, dbDisconnectRule.ObisCode)));
      Assert.That(disconnectRule.EvaluationName, Is.EqualTo(new SerieName(dbDisconnectRule.EvaluationLabel, dbDisconnectRule.EvaluationObisCode)));
      Assert.That(disconnectRule.Duration, Is.EqualTo(TimeSpan.FromSeconds(dbDisconnectRule.DurationSeconds)));
      Assert.That(disconnectRule.DisconnectToConnectValue, Is.EqualTo(dbDisconnectRule.DisconnectToConnectValue));
      Assert.That(disconnectRule.ConnectToDisconnectValue, Is.EqualTo(dbDisconnectRule.ConnectToDisconnectValue));
      Assert.That(disconnectRule.Unit, Is.EqualTo((Unit)dbDisconnectRule.Unit));
    }

    private DisconnectRuleRepository CreateTarget()
    {
      return new DisconnectRuleRepository(DbContext);
    }

    private void Insert<TEntity>(IEnumerable<TEntity> items) where TEntity : class, IDbEntity
    {
      foreach (var item in items)
      {
        DbContext.InsertTransaction(string.Empty, item);
      }
    }

  }
}
