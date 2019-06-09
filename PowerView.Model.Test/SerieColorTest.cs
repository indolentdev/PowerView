using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class SerieColorTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      const string label = "label";
      ObisCode obisCode = "1.2.3.4.5.6";
      const string color = "#123456";

      // Act & Assert
      Assert.That(() => new SerieColor(null, obisCode, color), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new SerieColor(string.Empty, obisCode, color), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new SerieColor(label, obisCode, null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new SerieColor(label, obisCode, string.Empty), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new SerieColor(label, obisCode, "#12"), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new SerieColor(label, obisCode, "1234567"), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      const string label = "label";
      ObisCode obisCode = "1.2.3.4.5.6";
      const string color = "#123456";

      // Act
      var target = new SerieColor(label, obisCode, color);

      // Assert
      Assert.That(target.Label, Is.EqualTo(label));
      Assert.That(target.ObisCode, Is.EqualTo(obisCode));
      Assert.That(target.Color, Is.EqualTo(color));
    }
  }
}
