using System;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;
using PowerView.Model.Test.Repository;

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
      var sn1 = new SeriesName("label1", ObisCode.ColdWaterFlow1);
      var sn2 = new SeriesName("label2", ObisCode.ElectrActualPowerP14);
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
      var sn1 = new SeriesName("label1", ObisCode.ColdWaterFlow1);

      var dbDisconnectRule = new Db.DisconnectRule { Label = sn1.Label, ObisCode = sn1.ObisCode, EvaluationLabel = sn1.Label, EvaluationObisCode = sn1.ObisCode,
        DurationSeconds = 4, DisconnectToConnectValue = 4, ConnectToDisconnectValue = 3, Unit = 4 };
      InsertDisconnectRule(dbDisconnectRule);

      var sn2 = new SeriesName("label2", ObisCode.ElectrActualPowerP14);
      var disconnectRule = new DisconnectRule(sn1, sn2, TimeSpan.FromMinutes(30), 1500, 200, Unit.Watt);
      var target = CreateTarget();

      // Act
      target.AddDisconnectRule(disconnectRule);

      // Assert
      AssertDisconnectRuleExists(disconnectRule);
    }

    private void AssertDisconnectRuleExists(DisconnectRule disconnectRule, bool not = false)
    {
      var disconnectRulesDb = DbContext.QueryTransaction<Db.DisconnectRule>("", @"
        SELECT * FROM DisconnectRule 
        WHERE Label=@Label AND ObisCode=@ObisCode AND EvaluationLabel=@EvaluationLabel AND EvaluationObisCode=@EvaluationObisCode AND
        DurationSeconds=@TotalSeconds AND DisconnectToConnectValue=@DisconnectToConnectValue AND ConnectToDisconnectValue=@ConnectToDisconnectValue AND Unit=@Unit;",
        new { disconnectRule.Name.Label, ObisCode = (long)disconnectRule.Name.ObisCode, EvaluationLabel = disconnectRule.EvaluationName.Label,
          EvaluationObisCode = (long)disconnectRule.EvaluationName.ObisCode, TotalSeconds = (int)disconnectRule.Duration.TotalSeconds,
          disconnectRule.DisconnectToConnectValue, disconnectRule.ConnectToDisconnectValue, Unit = (byte)disconnectRule.Unit });
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
      var sn = new SeriesName("label", "0.1.96.3.10.255");
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

      InsertDisconnectRules(dbDr1, dbDr2, dbDr3);
      var sn = new SeriesName("lbl1", "0.1.96.3.10.255");
      var target = CreateTarget();

      // Act
      target.DeleteDisconnectRule(sn);

      // Assert
      var disconnectRulesDb = DbContext.QueryTransaction<Db.DisconnectRule>("", 
        "SELECT * FROM DisconnectRule WHERE Label=@Label AND ObisCode=@ObisCode;", new { sn.Label, ObisCode = (long)sn.ObisCode });
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

      InsertDisconnectRules(dbDr1, dbDr2);

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
      DbContext.InsertReadings(reading1, reading2, reading3);
      var register1 = new Db.LiveRegister { ObisCode = ObisCode.ElectrActualPowerP23L2, Unit = (byte)Unit.Watt, ReadingId = reading1.Id };
      var register2 = new Db.LiveRegister { ObisCode = ObisCode.ElectrActualPowerP23L2, Unit = (byte)Unit.Joule, ReadingId = reading2.Id };
      var register3 = new Db.LiveRegister { ObisCode = ObisCode.ElectrActualPowerP23L2, Unit = (byte)Unit.Percentage, ReadingId = reading3.Id };
      DbContext.InsertRegisters(register1, register2, register3);
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();

      // Act
      var res = target.GetLatestSerieNames(dateTime);

      // Assert
      Assert.That(res.Count, Is.EqualTo(1));
      Assert.That(res, Contains.Key(new SeriesName("lbl", ObisCode.ElectrActualPowerP23L2)));
      Assert.That(res.First().Value, Is.EqualTo(Unit.Watt));
    }

    [Test]
    public void GetLatestSerieNamesFromDay()
    {
      // Arrange
      var reading1 = new Db.DayReading { Label = "lbl", SerialNumber = "SN", Timestamp = DateTime.UtcNow - TimeSpan.FromDays(4) };
      var reading2 = new Db.DayReading { Label = "lbl-other", SerialNumber = "SN-other", Timestamp = DateTime.UtcNow - TimeSpan.FromDays(8) };
      var reading3 = new Db.DayReading { Label = "lbl", SerialNumber = "SN", Timestamp = DateTime.UtcNow - TimeSpan.FromDays(6) };
      DbContext.InsertReadings(reading1, reading2, reading3);
      var register1 = new Db.DayRegister { ObisCode = ObisCode.ElectrActualPowerP23L2, Unit = (byte)Unit.Watt, ReadingId = reading1.Id };
      var register2 = new Db.DayRegister { ObisCode = ObisCode.ElectrActualPowerP23L2, Unit = (byte)Unit.Joule, ReadingId = reading2.Id };
      var register3 = new Db.DayRegister { ObisCode = ObisCode.ElectrActualPowerP23L2, Unit = (byte)Unit.Percentage, ReadingId = reading3.Id };
      DbContext.InsertRegisters(register1, register2, register3);
      var dateTime = DateTime.UtcNow;
      var target = CreateTarget();

      // Act
      var res = target.GetLatestSerieNames(dateTime);

      // Assert
      Assert.That(res.Count, Is.EqualTo(1));
      Assert.That(res, Contains.Key(new SeriesName("lbl", ObisCode.ElectrActualPowerP23L2)));
      Assert.That(res.First().Value, Is.EqualTo(Unit.Watt));
    }

    private static void AssertDisconnectRule(Db.DisconnectRule dbDisconnectRule, IDisconnectRule disconnectRule)
    {
      Assert.That(disconnectRule.Name, Is.EqualTo(new SeriesName(dbDisconnectRule.Label, dbDisconnectRule.ObisCode)));
      Assert.That(disconnectRule.EvaluationName, Is.EqualTo(new SeriesName(dbDisconnectRule.EvaluationLabel, dbDisconnectRule.EvaluationObisCode)));
      Assert.That(disconnectRule.Duration, Is.EqualTo(TimeSpan.FromSeconds(dbDisconnectRule.DurationSeconds)));
      Assert.That(disconnectRule.DisconnectToConnectValue, Is.EqualTo(dbDisconnectRule.DisconnectToConnectValue));
      Assert.That(disconnectRule.ConnectToDisconnectValue, Is.EqualTo(dbDisconnectRule.ConnectToDisconnectValue));
      Assert.That(disconnectRule.Unit, Is.EqualTo((Unit)dbDisconnectRule.Unit));
    }

    private DisconnectRuleRepository CreateTarget()
    {
      return new DisconnectRuleRepository(DbContext);
    }

    private void InsertDisconnectRule(Db.DisconnectRule disconnectRule)
    {
      InsertDisconnectRules(new Db.DisconnectRule[] { disconnectRule });
    }

    private void InsertDisconnectRules(params Db.DisconnectRule[] disconnectRules)
    {
      DbContext.ExecuteTransaction("",
        "INSERT INTO DisconnectRule (Label,ObisCode,EvaluationLabel,EvaluationObisCode,DurationSeconds,DisconnectToConnectValue,ConnectToDisconnectValue,Unit) VALUES (@Label,@ObisCode,@EvaluationLabel,@EvaluationObisCode,@DurationSeconds,@DisconnectToConnectValue,@ConnectToDisconnectValue,@Unit);",
        disconnectRules);
    }

  }
}
