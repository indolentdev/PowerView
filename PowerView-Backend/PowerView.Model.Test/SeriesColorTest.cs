using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class SeriesColorTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            var seriesName = new SeriesName("label", "1.2.3.4.5.6");
            const string color = "#123456";

            // Act & Assert
            Assert.That(() => new SeriesColor(null, color), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new SeriesColor(seriesName, null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new SeriesColor(seriesName, string.Empty), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new SeriesColor(seriesName, "#12"), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new SeriesColor(seriesName, "1234567"), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            var seriesName = new SeriesName("label", "1.2.3.4.5.6");
            const string color = "#123456";

            // Act
            var target = new SeriesColor(seriesName, color);

            // Assert
            Assert.That(target.SeriesName, Is.EqualTo(seriesName));
            Assert.That(target.Color, Is.EqualTo(color));
        }
    }
}
