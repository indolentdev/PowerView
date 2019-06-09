using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class SerieNameTest
  {
    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      const string label = "TheLabel";
      var obisCode = ObisCode.ColdWaterFlow1;

      // Act
      var target = new SerieName(label, obisCode);

      // Assert
      Assert.That(target.Label, Is.EqualTo(label));
      Assert.That(target.ObisCode, Is.EqualTo(obisCode));
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new SerieName(null, new ObisCode()), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new SerieName(string.Empty, new ObisCode()), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void EqualsAndHashCode()
    {
      // Arrange
      var t1 = new SerieName("MyLabel", ObisCode.ColdWaterVolume1);
      var t2 = new SerieName("MyLabel", ObisCode.ColdWaterVolume1);
      var t3 = new SerieName("OtherLabel", ObisCode.ColdWaterVolume1);
      var t4 = new SerieName("MyLabel", ObisCode.ActiveEnergyA14);

      // Act & Assert
      Assert.That(t1, Is.EqualTo(t2));
      Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t3));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t4));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));

      Assert.That(t1.Equals((object)t2), Is.True);
      Assert.That(t1.Equals((ISerieName)t2), Is.True);
      Assert.That(t1.Equals((object)t3), Is.False);
      Assert.That(t1.Equals((ISerieName)t3), Is.False);
    }

  }
}

