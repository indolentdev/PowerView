using System;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class ObisCodeTest
  {
    [Test]
    public void ConstructorAndEnumerable()
    {
      // Arrange
      var bytes = new byte[] {1,2,3,4,5,6};

      // Act
      var target = new ObisCode(bytes);

      // Assert
      Assert.That(target, Is.EqualTo(bytes));
    }

    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new ObisCode(null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new ObisCode(new byte[] {1,2,3,4,5}), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorImplicitStringToObisCode()
    {
      // Arrange
      const string s = "1.2.3.4.5.6";

      // Act
      ObisCode target = s;

      // Assert
      Assert.That(target, Is.EqualTo(new byte[] {1,2,3,4,5,6}));
    }

    [Test]
    public void ConstructorImplicitObisCodeToLong()
    {
      // Arrange
      var target = new ObisCode(new byte[] { 1,2,3,4,5,6 });

      // Act
      long l = target;

      // Assert
      Assert.That(l, Is.EqualTo(1108152157446));
    }

    [Test]
    public void ConstructorImplicitLongToObisCode()
    {
      // Arrange
      const long l = 1108152157446L;

      // Act
      ObisCode target = l;

      // Assert
      Assert.That(target, Is.EqualTo(new byte[] {1,2,3,4,5,6}));
    }

    [Test]
    public void ToStringTest()
    {
      // Arrange
      var bytes = new byte[] {1,2,3,4,5,6};

      // Act
      var target = new ObisCode(bytes);

      // Assert
      Assert.That(target.ToString(), Is.EqualTo("1.2.3.4.5.6"));
    }

    [Test]
    public void EqualsAndHashCode()
    {
      // Arrange
      var t1 = new ObisCode(new byte[] { 1,2,3,4,5,6 });
      var t2 = new ObisCode(new byte[] { 1,2,3,4,5,6 });
      var t3 = new ObisCode(new byte[] { 1,1,1,2,2,2 });

      // Act & Assert
      Assert.That(t1, Is.EqualTo(t2));
      Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
      Assert.That(t1, Is.Not.EqualTo(t3));
      Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
    }

    [Test]
    public void Equeality()
    {
        // Arrange
        var t1 = new ObisCode(new byte[] { 1,2,3,4,5,6 });
        var t2 = new ObisCode(new byte[] { 1,2,3,4,5,6 });
        var t3 = new ObisCode(new byte[] { 1,1,1,2,2,2 });

        // Act & Assert
        Assert.That(t1 == t2, Is.True);
        Assert.That(t1 == t3, Is.False);
    }

    [Test]
    public void StaticObisCodesElectricity()
    {
      // Arrange

      // Act & Assert
      Assert.That(ObisCode.ActiveEnergyA14, Is.EqualTo(new byte[] {1,0,1,8,0,255}));
      Assert.That(ObisCode.ActiveEnergyA14Interim, Is.EqualTo(new byte[] {1,0,1,8,0,200}));
      Assert.That(ObisCode.ActiveEnergyA14Delta, Is.EqualTo(new byte[] {1,0,1,8,0,100}));
      Assert.That(ObisCode.ActualPowerP14, Is.EqualTo(new byte[] {1,0,1,7,0,255}));
      Assert.That(ObisCode.ActualPowerP14L1, Is.EqualTo(new byte[] { 1, 0, 21, 7, 0, 255 }));
      Assert.That(ObisCode.ActualPowerP14L2, Is.EqualTo(new byte[] { 1, 0, 41, 7, 0, 255 }));
      Assert.That(ObisCode.ActualPowerP14L3, Is.EqualTo(new byte[] { 1, 0, 61, 7, 0, 255 }));
      Assert.That(ObisCode.ActiveEnergyA23, Is.EqualTo(new byte[] {1,0,2,8,0,255}));
      Assert.That(ObisCode.ActiveEnergyA23Interim, Is.EqualTo(new byte[] {1,0,2,8,0,200}));
      Assert.That(ObisCode.ActiveEnergyA23Delta, Is.EqualTo(new byte[] {1,0,2,8,0,100}));
      Assert.That(ObisCode.ActualPowerP23, Is.EqualTo(new byte[] {1,0,2,7,0,255}));
      Assert.That(ObisCode.ActualPowerP23L1, Is.EqualTo(new byte[] { 1, 0, 22, 7, 0, 255 }));
      Assert.That(ObisCode.ActualPowerP23L2, Is.EqualTo(new byte[] { 1, 0, 42, 7, 0, 255 }));
      Assert.That(ObisCode.ActualPowerP23L3, Is.EqualTo(new byte[] { 1, 0, 62, 7, 0, 255 }));
    }

    [Test]
    public void StaticObisCodesWater()
    {
      // Arrange

      // Act & Assert
      Assert.That(ObisCode.ColdWaterVolume1, Is.EqualTo(new byte[] {8,0,1,0,0,255}));
      Assert.That(ObisCode.ColdWaterVolume1Interim, Is.EqualTo(new byte[] {8,0,1,0,0,200}));
      Assert.That(ObisCode.ColdWaterVolume1Delta, Is.EqualTo(new byte[] {8,0,1,0,0,100}));
      Assert.That(ObisCode.ColdWaterFlow1, Is.EqualTo(new byte[] {8,0,2,0,0,255}));
    }

    [Test]
    public void StaticObisCodesEnergy()
    {
      // Arrange

      // Act & Assert
      Assert.That(ObisCode.HeatEnergyEnergy1, Is.EqualTo(new byte[] {6,0,1,0,0,255}));
      Assert.That(ObisCode.HeatEnergyEnergy1Interim, Is.EqualTo(new byte[] {6,0,1,0,0,200}));
      Assert.That(ObisCode.HeatEnergyEnergy1Delta, Is.EqualTo(new byte[] {6,0,1,0,0,100}));
      Assert.That(ObisCode.HeatEnergyVolume1, Is.EqualTo(new byte[] {6,0,2,0,0,255}));
      Assert.That(ObisCode.HeatEnergyVolume1Interim, Is.EqualTo(new byte[] {6,0,2,0,0,200}));
      Assert.That(ObisCode.HeatEnergyVolume1Delta, Is.EqualTo(new byte[] {6,0,2,0,0,100}));
      Assert.That(ObisCode.HeatEnergyPower1, Is.EqualTo(new byte[] {6,0,8,0,0,255}));
      Assert.That(ObisCode.HeatEnergyFlow1, Is.EqualTo(new byte[] {6,0,9,0,0,255}));
      Assert.That(ObisCode.HeatEnergyFlowTemperature, Is.EqualTo(new byte[] {6,0,10,0,0,255}));
      Assert.That(ObisCode.HeatEnergyReturnTemperature, Is.EqualTo(new byte[] {6,0,11,0,0,255}));
    }

    [Test]
    public void StaticObisCodesRoomSensor()
    {
      // Arrange

      // Act & Assert
      Assert.That(ObisCode.RoomTemperature, Is.EqualTo(new byte[] {15,0,223,0,0,255}));
      Assert.That(ObisCode.RoomRelativeHumidity, Is.EqualTo(new byte[] {15,0,223,0,2,255}));
    }

    [Test]
    public void StaticObisTemplates()
    {
      // Arrange

      // Act & Assert
      Assert.That(ObisCode.ConsumedElectricity, Is.EqualTo(new byte[] {1,210,1,8,0,255}));
      Assert.That(ObisCode.ConsumedElectricityInterim, Is.EqualTo(new byte[] { 1, 210, 1, 8, 0, 200 }));
      Assert.That(ObisCode.ConsumedElectricityDelta, Is.EqualTo(new byte[] { 1, 210, 1, 8, 0, 100 }));
      Assert.That(ObisCode.ConsumedElectricityWithHeat, Is.EqualTo(new byte[] {1,220,1,8,0,255}));
      Assert.That(ObisCode.ConsumedElectricityWithHeatInterim, Is.EqualTo(new byte[] { 1, 220, 1, 8, 0, 200 }));
      Assert.That(ObisCode.ConsumedElectricityWithHeatDelta, Is.EqualTo(new byte[] { 1, 220, 1, 8, 0, 100 }));
    }

    [Test]
    [TestCase("1.0.1.7.0.255")]
    [TestCase("1.0.21.7.0.255")]
    [TestCase("1.0.41.7.0.255")]
    [TestCase("1.0.61.7.0.255")]
    [TestCase("1.0.1.8.0.255")]
    [TestCase("1.0.1.8.0.200")]
    [TestCase("1.0.1.8.0.100")]
    [TestCase("1.0.1.8.0.123")]
    public void IsElectricityImport(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsElectricityImport;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("1.0.2.7.0.255")]
    [TestCase("1.0.22.7.0.255")]
    [TestCase("1.0.42.7.0.255")]
    [TestCase("1.0.62.7.0.255")]
    [TestCase("1.0.2.8.0.255")]
    [TestCase("1.0.2.8.0.200")]
    [TestCase("1.0.2.8.0.100")]
    [TestCase("1.0.2.8.0.123")]
    public void IsElectricityExport(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsElectricityExport;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("1.0.1.8.0.255")]
    [TestCase("1.0.2.8.0.255")]
    [TestCase("1.100.1.8.0.255")]
    [TestCase("1.100.2.8.0.255")]
    [TestCase("1.200.1.8.0.255")]
    [TestCase("1.200.2.8.0.255")]
    public void IsElectricityCumulative(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsElectricityCumulative;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("8.0.1.0.0.255")]
    [TestCase("8.0.1.0.0.200")]
    [TestCase("8.0.1.0.0.100")]
    [TestCase("9.0.1.0.0.255")]
    [TestCase("8.0.2.0.0.255")]
    [TestCase("9.0.2.0.0.255")]
    public void IsWaterImport(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsWaterImport;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("8.0.1.0.0.255")]
    public void IsWaterCumulative(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsWaterCumulative;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("6.0.1.0.0.255")]
    [TestCase("6.0.1.0.0.200")]
    [TestCase("6.0.1.0.0.100")]
    [TestCase("6.0.2.0.0.255")]
    [TestCase("6.0.2.0.0.200")]
    [TestCase("6.0.2.0.0.100")]
    [TestCase("6.0.8.0.0.255")]
    [TestCase("6.0.9.0.0.255")]
    [TestCase("6.0.10.0.0.255")]
    [TestCase("6.0.11.0.0.255")]
    [TestCase("5.0.1.0.0.255")]
    [TestCase("5.0.2.0.0.255")]
    public void IsEnergyImport(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsEnergyImport;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("15.0.223.0.0.255")]
    public void IsNotEnergyImport(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsEnergyImport;

      // Assert
      Assert.That(result, Is.False);
    }

    [Test]
    [TestCase("6.0.1.0.0.255")]
    [TestCase("6.0.2.0.0.255")]
    [TestCase("5.0.1.0.0.255")]
    [TestCase("5.0.2.0.0.255")]
    public void IsEnergyCumulative(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsEnergyCumulative;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("1.0.1.8.0.255")]
    [TestCase("1.0.2.8.0.255")]
    [TestCase("8.0.1.0.0.255")]
    [TestCase("6.0.1.0.0.255")]
    [TestCase("6.0.2.0.0.255")]
    [TestCase("5.0.1.0.0.255")]
    [TestCase("5.0.2.0.0.255")]
    public void IsCumulative(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsCumulative;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("1.0.1.8.0.200")]
    [TestCase("1.0.2.8.0.200")]
    [TestCase("1.100.1.8.0.200")]
    [TestCase("1.100.2.8.0.200")]
    [TestCase("1.200.1.8.0.200")]
    [TestCase("1.200.2.8.0.200")]
    public void IsInterim(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsInterim;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("1.0.1.8.0.100")]
    [TestCase("1.0.2.8.0.100")]
    [TestCase("1.100.1.8.0.100")]
    [TestCase("1.100.2.8.0.100")]
    [TestCase("1.200.1.8.0.100")]
    [TestCase("1.200.2.8.0.100")]
    public void IsDelta(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsDelta;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("0.0.96.3.10.255")]
    [TestCase("0.1.96.3.10.255")]
    [TestCase("0.9.96.3.10.255")]
    [TestCase("0.10.96.3.10.255")]
    [TestCase("0.99.96.3.10.255")]
    [TestCase("0.100.96.3.10.255")]
    [TestCase("0.255.96.3.10.255")]
    public void IsDisconnectControl(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.IsDisconnectControl;

      // Assert
      Assert.That(result, Is.True);
    }

    [Test]
    public void Templates()
    {
      // Arrange
      var expectedTemplates = new [] { ObisCode.ConsumedElectricity, ObisCode.ConsumedElectricityInterim, ObisCode.ConsumedElectricityDelta, 
        ObisCode.ConsumedElectricityWithHeat, ObisCode.ConsumedElectricityWithHeatInterim, ObisCode.ConsumedElectricityWithHeatDelta };

      // Act
      var actualTemplates = ObisCode.Templates;

      // Assert
      CollectionAssert.AreEquivalent(actualTemplates, expectedTemplates);
    }

    [Test]
    [TestCase("1.0.1.8.0.255")]
    [TestCase("1.0.1.8.0.200")]
    [TestCase("1.0.1.8.0.100")]
    [TestCase("1.0.1.8.0.123")]
    [TestCase("1.0.2.8.0.255")]
    [TestCase("1.0.2.8.0.200")]
    [TestCase("1.0.2.8.0.100")]
    [TestCase("1.0.2.8.0.123")]
    [TestCase("8.0.1.0.0.255")]
    public void ToInterim(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.ToInterim();

      // Assert
      Assert.That(result.ToString(), Is.EqualTo(obisCode.Substring(0,10)+"200"));
    }

    [Test]
    [TestCase("1.0.1.8.0.255")]
    [TestCase("1.0.1.8.0.200")]
    [TestCase("1.0.1.8.0.100")]
    [TestCase("1.0.1.8.0.123")]
    [TestCase("1.0.2.8.0.255")]
    [TestCase("1.0.2.8.0.200")]
    [TestCase("1.0.2.8.0.100")]
    [TestCase("1.0.2.8.0.123")]
    [TestCase("8.0.1.0.0.255")]
    public void ToDelta(string obisCode)
    {
      // Arrange
      ObisCode target = obisCode;

      // Act
      var result = target.ToDelta();

      // Assert
      Assert.That(result.ToString(), Is.EqualTo(obisCode.Substring(0,10)+"100"));
    }

    [Test]
    public void GetDefined()
    {
      // Arrange

      // Act
      var definedObisCodes = ObisCode.GetDefined();

      // Assert
      Assert.That(definedObisCodes.Count(), Is.GreaterThan(25));
    }
  }
}

