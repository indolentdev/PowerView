using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class GuageValueSetTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange
      var name = GaugeSetName.Latest;
      var values = new GaugeValue[] { new GaugeValue("l", "123", DateTime.UtcNow, ObisCode.ElectrActiveEnergyA14, new UnitValue(1, Unit.WattHour)) };

      // Act & Assert
      Assert.That(() => new GaugeValueSet((GaugeSetName)12345, values), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new GaugeValueSet(name, null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new GaugeValueSet(name, new GaugeValue[1]), Throws.TypeOf<ArgumentNullException>());
    }
    
    [Test]
    public void ConstructorAndProperties()
    {
      // Arrange
      var name = GaugeSetName.Latest;
      var values = new GaugeValue[] { new GaugeValue("l", "123", DateTime.UtcNow, ObisCode.ElectrActiveEnergyA14, new UnitValue(1, Unit.WattHour)) };

      // Act
      var target = new GaugeValueSet(name, values);

      // Assert
      Assert.That(target.Name, Is.EqualTo(name));
      Assert.That(target.GuageValues, Is.EqualTo(values));
    }
  }
}
