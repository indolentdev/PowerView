using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using DapperExtensions;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class SerieColorRepositoryTest : DbTestFixtureWithSchema
  {
    private Mock<IObisColorProvider> obisColorProvider;
    
    [SetUp]
    public override void SetUp()
    {
      base.SetUp();

      obisColorProvider = new Mock<IObisColorProvider>();
    }
    
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new SerieColorRepository(null, obisColorProvider.Object), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new SerieColorRepository(DbContext, null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetColorCachedThrows()
    {
      // Arrange
      var target = CreateTarget();
      ObisCode obisCode = "1.2.3.4.5.6";

      // Act & Assert
      Assert.That(() => target.GetColorCached(null, obisCode), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.GetColorCached(string.Empty, obisCode), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetColorCachedFromProvider()
    {
      // Arrange
      var target = CreateTarget();
      const string label = "label";
      ObisCode obisCode = "1.2.3.4.5.6";
      const string colorBase = "#123456";
      obisColorProvider.Setup(ocp => ocp.GetColor(It.IsAny<ObisCode>())).Returns(colorBase);

      // Act
      var color = target.GetColorCached(label, obisCode);

      // Assert
      Assert.That(color, Is.EqualTo(colorBase));
      obisColorProvider.Verify(ocp => ocp.GetColor(It.Is<ObisCode>(oc => oc == obisCode)));
    }

    [Test]
    public void GetColorCachedFromDb()
    {
      // Arrange
      var target = CreateTarget();
      const string label = "label";
      ObisCode obisCode = "1.2.3.4.5.6";
      const string colorBase = "#123456";
      DbContext.Connection.Insert(new Db.SerieColor { Label = label, ObisCode = obisCode, Color = colorBase });

      // Act
      var color = target.GetColorCached(label, obisCode);

      // Assert
      Assert.That(color, Is.EqualTo(colorBase));
      obisColorProvider.Verify(ocp => ocp.GetColor(It.IsAny<ObisCode>()), Times.Never);
    }

    [Test]
    public void GetSerieColors()
    {
      // Arrange
      var target = CreateTarget();
      const string label1 = "label";
      const string label2 = "label";
      ObisCode obisCode1 = "1.2.3.4.5.6";
      ObisCode obisCode2 = "6.5.4.3.2.1";
      ObisCode obisCode3 = "3.4.3.4.3.4";
      DbContext.Connection.Insert(new Db.SerieColor{ Label=label1, ObisCode=obisCode1, Color="#111111" });
      DbContext.Connection.Insert(new Db.SerieColor{ Label=label2, ObisCode=obisCode2, Color="#222222" });
      DbContext.Connection.Insert(new Db.SerieColor{ Label=label1, ObisCode=obisCode3, Color="#333333" });

      // Act
      var serieColors = target.GetSerieColors();

      // Assert
      Assert.That(serieColors.Count, Is.EqualTo(3));
      Assert.That(serieColors.Count(sc => sc.Label==label1 && sc.ObisCode==obisCode1 && sc.Color=="#111111"), Is.EqualTo(1));
      Assert.That(serieColors.Count(sc => sc.Label==label2 && sc.ObisCode==obisCode2 && sc.Color=="#222222"), Is.EqualTo(1));
      Assert.That(serieColors.Count(sc => sc.Label==label1 && sc.ObisCode==obisCode3 && sc.Color=="#333333"), Is.EqualTo(1));
    }

    [Test]
    public void SetSerieColorsThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.SetSerieColors(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void SetSerieColorsInserts()
    {
      // Arrange
      var target = CreateTarget();
      var serieColor = new SerieColor("label", "1.2.3.4.5.6", "#123456");

      // Act
      target.SetSerieColors(new [] { serieColor });

      // Assert
      AssertSerieColorExists(serieColor);
    }

    [Test]
    public void SetSerieColorsUpdatesExisting()
    {
      // Arrange
      var target = CreateTarget();
      var dbSerieColor = new Db.SerieColor { Label = "label", ObisCode = (ObisCode)"1.2.3.4.5.6", Color="#111111" };
      var serieColor = new SerieColor(dbSerieColor.Label, dbSerieColor.ObisCode, "#222222");

      // Act
      target.SetSerieColors(new [] { serieColor });

      // Assert
      AssertSerieColorExists(serieColor);
    }
      
    [Test]
    public void SetSerieColorsDeletesWhenColorMatchesObisCodeProvider()
    {
      // Arrange
      var target = CreateTarget();
      var dbSerieColor = new Db.SerieColor { Label = "label", ObisCode = (ObisCode)"1.2.3.4.5.6", Color="#111111" };
      DbContext.Connection.Insert(dbSerieColor);
      var serieColor = new SerieColor(dbSerieColor.Label, dbSerieColor.ObisCode, "#222222");
      obisColorProvider.Setup(ocp => ocp.GetColor(serieColor.ObisCode)).Returns(serieColor.Color);

      // Act
      target.SetSerieColors(new [] { serieColor });

      // Assert
      AssertSerieColorExists(serieColor, not:true);
    }

    private void AssertSerieColorExists(SerieColor serieColor, bool not = false)
    {
      var predicateLabel = Predicates.Field<Db.SerieColor>(sc => sc.Label, Operator.Eq, serieColor.Label);
      var predicateObisCode = Predicates.Field<Db.SerieColor>(sc => sc.ObisCode, Operator.Eq, (long)serieColor.ObisCode);
      var predicateColor = Predicates.Field<Db.SerieColor>(sc => sc.Color, Operator.Eq, serieColor.Color);
      var predicate = Predicates.Group(GroupOperator.And, predicateLabel, predicateObisCode, predicateColor);
      Assert.That(DbContext.Connection.Count<Db.SerieColor>(predicate), Is.EqualTo(not ? 0 : 1));
    }

    private SerieColorRepository CreateTarget()
    {
      return new SerieColorRepository(DbContext, obisColorProvider.Object);
    }

  }
}
