using System;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class LabelRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new LabelRepository(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetSerieNames()
    {
      // Arrange
      var target = CreateTarget();
      const string label1 = "label1";
      const string label2 = "label2";
      const string label3 = "label3";
      Insert<Db.LiveReading>(label1);
      Insert<Db.LiveReading>(label2);
      Insert<Db.DayReading>(label2);
      Insert<Db.DayReading>(label3);
      Insert<Db.MonthReading>(label3);
      Insert<Db.MonthReading>(label1);

      // Act
      var labels = target.GetLabels();

      // Assert
      Assert.That(labels, Is.EqualTo(new[] { label1, label2, label3 }));
    }

    private void Insert<TReading>(string label)
      where TReading  : IDbReading,  new()
    {
      var reading = new TReading { Label=label, SerialNumber="1", Timestamp=DateTime.UtcNow };
      DbContext.InsertReadings(reading);
    }

    private LabelRepository CreateTarget()
    {
      return new LabelRepository(DbContext);
    }

  }
}
