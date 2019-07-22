using System;
using System.Linq;
using NUnit.Framework;
using DapperExtensions;
using PowerView.Model.Expression;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class SeriesNameRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new SeriesNameRepository(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetSerieNames()
    {
      // Arrange
      var target = CreateTarget();
      const string label1 = "label1";
      const string label2 = "label2";
      const string label3 = "label3";
      ObisCode obisCode1 = "1.2.3.4.5.6";
      ObisCode obisCode2 = "6.5.4.3.2.1";
      ObisCode obisCode3 = "3.4.3.4.3.4";
      InsertLiveReading(label1, obisCode1);
      InsertLiveReading(label2, obisCode2);
      InsertDayReading(label2, obisCode2);
      InsertDayReading(label3, obisCode3);
      InsertMonthReading(label3, obisCode3);
      InsertMonthReading(label1, obisCode1);

      // Act
      var serieNames = target.GetSeriesNames(new LabelObisCodeTemplate[0]);

      // Assert
      Assert.That(serieNames.Count, Is.EqualTo(3));
      Assert.That(serieNames.Count(sc => sc.Label==label1 && sc.ObisCode==obisCode1), Is.EqualTo(1));
      Assert.That(serieNames.Count(sc => sc.Label==label2 && sc.ObisCode==obisCode2), Is.EqualTo(1));
      Assert.That(serieNames.Count(sc => sc.Label==label3 && sc.ObisCode==obisCode3), Is.EqualTo(1));
    }

    [Test]
    public void GetSerieNamesReplaceCumulativeObisCodes()
    {
      // Arrange
      var target = CreateTarget();
      const string label = "label";
      InsertDayReading(label, ObisCode.ElectrActiveEnergyA14);

      // Act
      var serieColors = target.GetSeriesNames(new LabelObisCodeTemplate[0]);

      // Assert
      Assert.That(serieColors.Count, Is.EqualTo(3));
      Assert.That(serieColors.Count(sc => sc.Label==label && sc.ObisCode==ObisCode.ElectrActiveEnergyA14Period), Is.EqualTo(1));
      Assert.That(serieColors.Count(sc => sc.Label==label && sc.ObisCode==ObisCode.ElectrActiveEnergyA14Delta), Is.EqualTo(1));
      Assert.That(serieColors.Count(sc => sc.Label==label && sc.ObisCode==ObisCode.ElectrActualPowerP14Average), Is.EqualTo(1));
    }

    [Test]
    public void GetSerieNamesGeneratesFromTemplates()
    {
      // Arrange
      var target = CreateTarget();
      const string label1 = "label1";
      const string label2 = "label2";
      ObisCode obisCode1 = "1.2.3.4.5.6";
      ObisCode obisCode2 = "6.5.4.3.2.1";
      InsertLiveReading(label1, obisCode1);
      var labelObisCodeTemplats = new[] {
        new LabelObisCodeTemplate(label2, new [] { new ObisCodeTemplate(obisCode2, new RegisterTemplateExpression(label1 + ":" + obisCode1)) }) };

      // Act
      var serieNames = target.GetSeriesNames(labelObisCodeTemplats);

      // Assert
      Assert.That(serieNames.Count, Is.EqualTo(2));
      Assert.That(serieNames.Count(sc => sc.Label==label2 && sc.ObisCode==obisCode2), Is.EqualTo(1));
    }

    private void InsertLiveReading(string label, params ObisCode[] obisCodes)
    {
      var liveReading = new Db.LiveReading { Label=label, SerialNumber="1", Timestamp=DateTime.UtcNow };
      DbContext.Connection.Insert(liveReading);

      foreach (var obisCode in obisCodes)
      {
        var LiveRegister = new Db.LiveRegister { ObisCode=obisCode, Value=2, Scale=0, Unit=(byte)Unit.Watt, ReadingId=liveReading.Id };
        DbContext.Connection.Insert(LiveRegister);
      }
    }

    private void InsertDayReading(string label, params ObisCode[] obisCodes)
    {
      var dayReading = new Db.DayReading { Label=label, SerialNumber="1", Timestamp=DateTime.UtcNow };
      DbContext.Connection.Insert(dayReading);

      foreach (var obisCode in obisCodes)
      {
        var dayRegister = new Db.DayRegister { ObisCode=obisCode, Value=2, Scale=0, Unit=(byte)Unit.Watt, ReadingId=dayReading.Id };
        DbContext.Connection.Insert(dayRegister);
      }
    }

    private void InsertMonthReading(string label, params ObisCode[] obisCodes)
    {
      var dayReading = new Db.MonthReading { Label=label, SerialNumber="1", Timestamp=DateTime.UtcNow };
      DbContext.Connection.Insert(dayReading);

      foreach (var obisCode in obisCodes)
      {
        var dayRegister = new Db.MonthRegister { ObisCode=obisCode, Value=2, Scale=0, Unit=(byte)Unit.Watt, ReadingId=dayReading.Id };
        DbContext.Connection.Insert(dayRegister);
      }
    }

    private SeriesNameRepository CreateTarget()
    {
      return new SeriesNameRepository(DbContext);
    }

  }
}
