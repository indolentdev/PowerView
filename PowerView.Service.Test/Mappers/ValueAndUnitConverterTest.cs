using System;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Service.Mappers;

namespace PowerView.Service.Test.Mappers
{
  [TestFixture]
  public class VaueAndUnitConverterTest
  {
    [Test]
    public void ConvertValueNull()
    {
      // Arrange

      // Act
      var unitString = ValueAndUnitConverter.Convert(null, Unit.Watt);

      // Assert
      Assert.That(unitString, Is.Null);
    }

    [Test]
    public void ConvertValueWattHour()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitConverter.Convert(1234.56789d, Unit.WattHour);

      // Assert
      Assert.That(valueString, Is.EqualTo(1.235d));
    }

    [Test]
    public void ConvertValueWatt()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitConverter.Convert(1234.56789d, Unit.Watt);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234.568d));
    }

    [Test]
    public void ConvertValueJoule()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitConverter.Convert(12345678.9123d, Unit.Joule);

      // Assert
      Assert.That(valueString, Is.EqualTo(3.429).Within(.00000000000001));
    }

    [Test]
    public void ConvertValueJoulePrHour()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitConverter.Convert(12345678.9123d, Unit.JoulePrHour);

      // Assert
      Assert.That(valueString, Is.EqualTo(3429.355d));
    }

    [Test]
    public void ConvertValueCubicMetre()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitConverter.Convert(1234.56789d, Unit.CubicMetre);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234.568d));
    }

    [Test]
    public void ConvertValueCubicMetreWithReduceUnit()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitConverter.Convert(0.123456789d, Unit.CubicMetre, true);

      // Assert
      Assert.That(valueString, Is.EqualTo(123.457d));
    }

    [Test]
    public void ConvertValueCubicMetrePrHour()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitConverter.Convert(1234.5678d, Unit.CubicMetrePrHour);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234567.8d));
    }

    [Test]
    public void ConvertValueDegreeCelsius()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitConverter.Convert(1234.56789d, Unit.DegreeCelsius);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234.568d));
    }

    [Test]
    public void ConvertValuePercentage()
    {
      // Arrange

      // Act
      var valueString = ValueAndUnitConverter.Convert(1234.56789d, Unit.Percentage);

      // Assert
      Assert.That(valueString, Is.EqualTo(1234.568d));
    }

    [Test]
    public void ConvertUnitUnknown()
    {
      // Arrange
      var unit = (Unit)54;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("Unknown"));
    }

    [Test]
    public void ConvertUnitWattHour()
    {
      // Arrange
      var unit = Unit.WattHour;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("kWh"));
    }

    [Test]
    public void ConvertUnitWatt()
    {
      // Arrange
      var unit = Unit.Watt;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("W"));
    }

    [Test]
    public void ConvertUnitJoule()
    {
      // Arrange
      var unit = Unit.Joule;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("kWh"));
    }

    [Test]
    public void ConvertUnitJoulePrHour()
    {
      // Arrange
      var unit = Unit.JoulePrHour;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("W"));
    }

    [Test]
    public void ConvertUnitCubicMetre()
    {
      // Arrange
      var unit = Unit.CubicMetre;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("m3"));
    }

    [Test]
    public void ConvertUnitCubicMetreWihUnitReduce()
    {
      // Arrange
      var unit = Unit.CubicMetre;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit, true);

      // Assert
      Assert.That(unitString, Is.EqualTo("l"));
    }

    [Test]
    public void ConvertUnitCubicMetrePrHour()
    {
      // Arrange
      var unit = Unit.CubicMetrePrHour;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("l/h"));
    }

    [Test]
    public void ConvertUnitDegreeCelsius()
    {
      // Arrange
      var unit = Unit.DegreeCelsius;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("C"));
    }

    [Test]
    public void ConvertUnitPercentage()
    {
      // Arrange
      var unit = Unit.Percentage;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo("%"));
    }

    [Test]
    public void ConvertUnitNoUnit()
    {
      // Arrange
      var unit = Unit.NoUnit;

      // Act
      var unitString = ValueAndUnitConverter.Convert(unit);

      // Assert
      Assert.That(unitString, Is.EqualTo(string.Empty));
    }

  }
}

