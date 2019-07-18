using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class GaugeValueTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      const string label = "lbl";
      const string sn = "123";
      DateTime dt = DateTime.UtcNow;
      ObisCode oc = ObisCode.ElectrActiveEnergyA14;
      var unitValue = new UnitValue(1, Unit.CubicMetre);

      // Act & Assert
      Assert.That(() => new GaugeValue(string.Empty, sn, dt, oc, unitValue), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new GaugeValue(null, sn, dt, oc, unitValue), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new GaugeValue(label, string.Empty, dt, oc, unitValue), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new GaugeValue(label, null, dt, oc, unitValue), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new GaugeValue(label, sn, DateTime.Now, oc, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      const string label = "lbl";
      const string sn = "123";
      DateTime dt = DateTime.UtcNow;
      ObisCode oc = ObisCode.ElectrActiveEnergyA14;
      var unitValue = new UnitValue(1, Unit.CubicMetre);

      // Act
      var target = new GaugeValue(label, sn, dt, oc, unitValue);

      // Assert
      Assert.That(target.Label, Is.EqualTo(label));
      Assert.That(target.SerialNumber, Is.EqualTo(sn));
      Assert.That(target.DateTime, Is.EqualTo(dt));
      Assert.That(target.ObisCode, Is.EqualTo(oc));
      Assert.That(target.UnitValue, Is.EqualTo(unitValue));
    }

  }
}
