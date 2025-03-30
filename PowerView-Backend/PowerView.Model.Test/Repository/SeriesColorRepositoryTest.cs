using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class SeriesColorRepositoryTest : DbTestFixtureWithSchema
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
            Assert.That(() => new SeriesColorRepository(null, obisColorProvider.Object), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new SeriesColorRepository(DbContext, null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetColorCachedThrows()
        {
            // Arrange
            var target = CreateTarget();
            ObisCode obisCode = "1.2.3.4.5.6";

            // Act & Assert
            Assert.That(() => target.GetColorCached(null, obisCode), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.GetColorCached(string.Empty, obisCode), Throws.TypeOf<ArgumentOutOfRangeException>());
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
            var sc = new Db.SerieColor { Label = label, ObisCode = obisCode, Color = colorBase };
            InsertSerieColors(sc);

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
            var sc1 = new Db.SerieColor { Label = label1, ObisCode = obisCode1, Color = "#111111" };
            var sc2 = new Db.SerieColor { Label = label2, ObisCode = obisCode2, Color = "#222222" };
            var sc3 = new Db.SerieColor { Label = label1, ObisCode = obisCode3, Color = "#333333" };
            InsertSerieColors(sc1, sc2, sc3);

            // Act
            var seriesColors = target.GetSeriesColors();

            // Assert
            Assert.That(seriesColors.Count, Is.EqualTo(3));
            Assert.That(seriesColors.Count(sc => sc.SeriesName.Label == label1 && sc.SeriesName.ObisCode == obisCode1 && sc.Color == "#111111"), Is.EqualTo(1));
            Assert.That(seriesColors.Count(sc => sc.SeriesName.Label == label2 && sc.SeriesName.ObisCode == obisCode2 && sc.Color == "#222222"), Is.EqualTo(1));
            Assert.That(seriesColors.Count(sc => sc.SeriesName.Label == label1 && sc.SeriesName.ObisCode == obisCode3 && sc.Color == "#333333"), Is.EqualTo(1));
        }

        [Test]
        public void SetSerieColorsThrows()
        {
            // Arrange
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.SetSeriesColors(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void SetSeriesColorsInserts()
        {
            // Arrange
            var target = CreateTarget();
            var serieColor = new SeriesColor(new SeriesName("label", "1.2.3.4.5.6"), "#123456");

            // Act
            target.SetSeriesColors(new[] { serieColor });

            // Assert
            AssertSeriesColorExists(serieColor);
        }

        [Test]
        public void SetSerieColorsUpdatesExisting()
        {
            // Arrange
            var target = CreateTarget();
            var dbSerieColor = new Db.SerieColor { Label = "label", ObisCode = (ObisCode)"1.2.3.4.5.6", Color = "#111111" };
            var seriesColor = new SeriesColor(new SeriesName(dbSerieColor.Label, dbSerieColor.ObisCode), "#222222");

            // Act
            target.SetSeriesColors(new[] { seriesColor });

            // Assert
            AssertSeriesColorExists(seriesColor);
        }

        [Test]
        public void SetSerieColorsDeletesWhenColorMatchesObisCodeProvider()
        {
            // Arrange
            var target = CreateTarget();
            var dbSerieColor = new Db.SerieColor { Label = "label", ObisCode = (ObisCode)"1.2.3.4.5.6", Color = "#111111" };
            InsertSerieColors(dbSerieColor);
            var seriesColor = new SeriesColor(new SeriesName(dbSerieColor.Label, dbSerieColor.ObisCode), "#222222");
            obisColorProvider.Setup(ocp => ocp.GetColor(seriesColor.SeriesName.ObisCode)).Returns(seriesColor.Color);

            // Act
            target.SetSeriesColors(new[] { seriesColor });

            // Assert
            AssertSeriesColorExists(seriesColor, not: true);
        }

        private void AssertSeriesColorExists(SeriesColor serieColor, bool not = false)
        {
            var serieColors = DbContext.QueryTransaction<Db.SerieColor>("SELECT * FROM SerieColor WHERE Label=@Label AND ObisCode=@ObisCode AND Color=@Color;",
              new { serieColor.SeriesName.Label, ObisCode = (long)serieColor.SeriesName.ObisCode, serieColor.Color });
            Assert.That(serieColors.Count, Is.EqualTo(not ? 0 : 1));
        }

        private SeriesColorRepository CreateTarget()
        {
            return new SeriesColorRepository(DbContext, obisColorProvider.Object);
        }

        private void InsertSerieColors(params Db.SerieColor[] serieColors)
        {
            DbContext.ExecuteTransaction("INSERT INTO SerieColor (Label,ObisCode,Color) VALUES (@Label,@ObisCode,@Color);", serieColors);
        }

    }
}
