using System;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Service.Mappers;

namespace PowerView.Service.Test.Mappers
{
  [TestFixture]
  public class VaueAndUnitMapperTest
  {
    [Test]
    public void MapValueNull()
    {
      // Arrange

      // Act
      var unitString = ValueAndUnitMapper.Map(null, Unit.Watt);

      // Assert
      Assert.That(unitString, Is.Null);
    }

    [Test]
    public void MapValueWattHour()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitMapper.Map(1234.56789d, Unit.WattHour);

      // Assert
      Assert.That(valueString, Is.EqualTo(1.235d));
    }

    [Test]
    public void MapValueWatt()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitMapper.Map(1234.56789d, Unit.Watt);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234.568d));
    }

    [Test]
    public void MapValueJoule()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitMapper.Map(12345678.9123d, Unit.Joule);

      // Assert
      Assert.That(valueString, Is.EqualTo(3.429).Within(.00000000000001));
    }

    [Test]
    public void MapValueJoulePrHour()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitMapper.Map(12345678.9123d, Unit.JoulePrHour);

      // Assert
      Assert.That(valueString, Is.EqualTo(3429.355d));
    }

    [Test]
    public void MapValueCubicMetre()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitMapper.Map(1234.56789d, Unit.CubicMetre);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234.568d));
    }

    [Test]
    public void MapValueCubicMetreWithReduceUnit()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitMapper.Map(0.123456789d, Unit.CubicMetre, true);

      // Assert
      Assert.That(valueString, Is.EqualTo(123.457d));
    }

    [Test]
    public void MapValueCubicMetrePrHour()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitMapper.Map(1234.5678d, Unit.CubicMetrePrHour);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234567.8d));
    }

    [Test]
    public void MapValueDegreeCelsius()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitMapper.Map(1234.56789d, Unit.DegreeCelsius);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234.568d));
    }

    [Test]
    public void MapValuePercentage()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitMapper.Map(1234.56789d, Unit.Percentage);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234.568d));
    }

    [Test]
    public void MapUnitUnknown()
    {
      // Arrange
      var unit = (Unit)54;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("Unknown"));
    }

    [Test]
    public void MapUnitWattHour()
    {
      // Arrange
      var unit = Unit.WattHour;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("kWh"));
    }

    [Test]
    public void MapUnitWatt()
    {
      // Arrange
      var unit = Unit.Watt;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("W"));
    }

    [Test]
    public void MapUnitJoule()
    {
      // Arrange
      var unit = Unit.Joule;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("kWh"));
    }

    [Test]
    public void MapUnitJoulePrHour()
    {
      // Arrange
      var unit = Unit.JoulePrHour;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("W"));
    }

    [Test]
    public void MapUnitCubicMetre()
    {
      // Arrange
      var unit = Unit.CubicMetre;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("m3"));
    }

    [Test]
    public void MapUnitCubicMetreWihUnitReduce()
    {
      // Arrange
      var unit = Unit.CubicMetre;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit, true);

      // Assert
      Assert.That(unitString, Is.EqualTo("l"));
    }

    [Test]
    public void MapUnitCubicMetrePrHour()
    {
      // Arrange
      var unit = Unit.CubicMetrePrHour;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("l/h"));
    }

    [Test]
    public void MapUnitDegreeCelsius()
    {
      // Arrange
      var unit = Unit.DegreeCelsius;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("C"));
    }

    [Test]
    public void MapUnitPercentage()
    {
      // Arrange
      var unit = Unit.Percentage;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("%"));
    }

    [Test]
    public void MapUnitNoUnit()
    {
      // Arrange
      var unit = Unit.NoUnit;

      // Act
      var unitString = ValueAndUnitMapper.Map(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo(string.Empty));
    }

    [Test]
    [TestCase(Unit.Watt, "W")]
    public void MapUnitString(Unit expectedUnit, string unitString)
    {
      // Arrange

      // Act
      var unit = ValueAndUnitMapper.Map(unitString);

      // Assert
      Assert.That(unit, Is.EqualTo(expectedUnit));
    }

    [Test]
    [TestCase(typeof(ArgumentNullException), null)]
    [TestCase(typeof(ArgumentNullException), "")]
    [TestCase(typeof(ArgumentOutOfRangeException), "whatnot")]
    public void MapUnitStringUnsupported(Type expectedException, string unitString)
    {
      // Arrange

      // Act & Assert
      Assert.That(() => ValueAndUnitMapper.Map(unitString), Throws.TypeOf(expectedException));
    }

  }
}

