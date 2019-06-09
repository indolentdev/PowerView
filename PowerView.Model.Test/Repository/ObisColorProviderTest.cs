﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  public class ObisColorProviderTest
  {
    [Test]
    public void All()
    {
      // Arrange

      // Act
      var obisCodes = GetObisCodes().ToArray();

      // Assert
      Assert.That(obisCodes.Length-ObisCode.Templates.Count, Is.EqualTo(30), "Remember to extend this test fixture when adding obis code definitions");
    }

    [Test]
    public void Black([Values("1.0.21.7.0.255", "1.0.41.7.0.255", "1.0.61.7.0.255", "1.0.22.7.0.255", "1.0.42.7.0.255", "1.0.62.7.0.255",
      "1.0.1.8.0.255", "1.0.2.8.0.255", "8.0.1.0.0.255", "6.0.1.0.0.255", 
      "6.0.2.0.0.255")] string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(obisCode);

      // Assert
      Assert.That(color, Is.EqualTo("#000000"));
    }

    [Test]
    public void ActiveEnergyA14Interim()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.ActiveEnergyA14Interim);

      // Assert
      Assert.That(color, Is.EqualTo("#FFF5CC"));
    }

    [Test]
    public void ActiveEnergyA14Delta()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.ActiveEnergyA14Delta);

      // Assert
      Assert.That(color, Is.EqualTo("#FFCC00"));
    }

    [Test]
    public void ActualPowerP14()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.ActualPowerP14);

      // Assert
      Assert.That(color, Is.EqualTo("#FFCC00"));
    }

    [Test]
    public void ActiveEnergyA23Interim()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.ActiveEnergyA23Interim);

      // Assert
      Assert.That(color, Is.EqualTo("#FFFFCC"));
    }

    [Test]
    public void ActiveEnergyA23Delta()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.ActiveEnergyA23Delta);

      // Assert
      Assert.That(color, Is.EqualTo("#FFFF00"));
    }

    [Test]
    public void ActualPowerP23()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.ActualPowerP23);

      // Assert
      Assert.That(color, Is.EqualTo("#FFFF00"));
    }

    [Test]
    public void ColdWaterVolume1Interim()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.ColdWaterVolume1Interim);

      // Assert
      Assert.That(color, Is.EqualTo("#99CCFF"));
    }

    [Test]
    public void ColdWaterVolume1Delta()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.ColdWaterVolume1Delta);

      // Assert
      Assert.That(color, Is.EqualTo("#330066"));
    }

    [Test]
    public void ColdWaterFlow1()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.ColdWaterFlow1);

      // Assert
      Assert.That(color, Is.EqualTo("#330066"));
    }

    [Test]
    public void HeatEnergyEnergy1Interim()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.HeatEnergyEnergy1Interim);

      // Assert
      Assert.That(color, Is.EqualTo("#FF6699"));
    }

    [Test]
    public void HeatEnergyEnergy1Delta()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.HeatEnergyEnergy1Delta);

      // Assert
      Assert.That(color, Is.EqualTo("#CC3300"));
    }

    [Test]
    public void HeatEnergyVolume1Interim()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.HeatEnergyVolume1Interim);

      // Assert
      Assert.That(color, Is.EqualTo("#D6F5F5"));
    }

    [Test]
    public void HeatEnergyVolume1Delta()
    {
      // Arrange
      var target = CreateTarget();  

      // Act
      var color = target.GetColor(ObisCode.HeatEnergyVolume1Delta);

      // Assert
      Assert.That(color, Is.EqualTo("#33CCCC"));
    }

    [Test]
    public void HeatEnergyPower1()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.HeatEnergyPower1);

      // Assert
      Assert.That(color, Is.EqualTo("#CC3300"));
    }

    [Test]
    public void HeatEnergyFlow1()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.HeatEnergyFlow1);

      // Assert
      Assert.That(color, Is.EqualTo("#33CCCC"));
    }

    [Test]
    public void HeatEnergyFlowTemperature()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.HeatEnergyFlowTemperature);

      // Assert
      Assert.That(color, Is.EqualTo("#E60000"));
    }

    [Test]
    public void HeatEnergyReturnTemperature()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.HeatEnergyReturnTemperature);

      // Assert
      Assert.That(color, Is.EqualTo("#3333CC"));
    }

    [Test]
    public void RoomTemperature()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.RoomTemperature);

      // Assert
      Assert.That(color, Is.EqualTo("#FF99FF"));
    }

    [Test]
    public void RoomRelativeHumidity()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var color = target.GetColor(ObisCode.RoomRelativeHumidity);

      // Assert
      Assert.That(color, Is.EqualTo("#E600E6"));
    }

    private static ObisColorProvider CreateTarget()
    {
      return new ObisColorProvider();
    }

    private static IEnumerable<ObisCode> GetObisCodes()
    {
      var obisCodeType = typeof(ObisCode);
      var obisCodeProperties = obisCodeType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField)
        .Where(pi => pi.FieldType == typeof(ObisCode));
      foreach (var pi in obisCodeProperties)
      {
        yield return (ObisCode)pi.GetValue(null);
      }
    }
  }
}

