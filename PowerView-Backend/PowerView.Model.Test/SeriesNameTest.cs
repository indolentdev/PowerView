using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class SeriesNameTest
    {
        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            const string label = "TheLabel";
            var obisCode = ObisCode.ColdWaterFlow1;

            // Act
            var target = new SeriesName(label, obisCode);

            // Assert
            Assert.That(target.Label, Is.EqualTo(label));
            Assert.That(target.ObisCode, Is.EqualTo(obisCode));
        }

        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new SeriesName(null, new ObisCode()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new SeriesName(string.Empty, new ObisCode()), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ToStringTest()
        {
            // Arrange
            const string label = "TheLabel";
            var obisCode = ObisCode.ColdWaterFlow1;
            var target = new SeriesName(label, obisCode);

            // Act
            var s = target.ToString();

            // Assert
            Assert.That(s, Is.EqualTo("[SeriesName: Label=TheLabel, ObisCode=" + obisCode + "]"));
        }


        [Test]
        public void EqualsAndHashCode()
        {
            // Arrange
            var t1 = new SeriesName("MyLabel", ObisCode.ColdWaterVolume1);
            var t2 = new SeriesName("MyLabel", ObisCode.ColdWaterVolume1);
            var t3 = new SeriesName("OtherLabel", ObisCode.ColdWaterVolume1);
            var t4 = new SeriesName("MyLabel", ObisCode.ElectrActiveEnergyA14);

            // Act & Assert
            Assert.That(t1, Is.EqualTo(t2));
            Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t3));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t4));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));

            Assert.That(t1.Equals((object)t2), Is.True);
            Assert.That(t1.Equals((ISeriesName)t2), Is.True);
            Assert.That(t1.Equals((object)t3), Is.False);
            Assert.That(t1.Equals((ISeriesName)t3), Is.False);
        }

    }
}

