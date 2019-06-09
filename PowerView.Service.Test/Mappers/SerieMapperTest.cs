using NUnit.Framework;
using PowerView.Service.Mappers;

namespace PowerView.Service.Test.Mappers
{
  [TestFixture]
  public class SerieMapperTest
  {
    [Test]
    [TestCase("1.0.1.8.0.200")]
    [TestCase("1.0.2.8.0.200")]
    [TestCase("8.0.1.0.0.200")]
    [TestCase("6.0.1.0.0.200")]
    [TestCase("6.0.2.0.0.200")]
    [TestCase("1.210.1.8.0.200")]
    [TestCase("1.220.1.8.0.200")]
    public void MapToSerieTypeAreaspline(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var serieType = target.MapToSerieType(obisCode);

      // Assert
      Assert.That(serieType, Is.EqualTo("areaspline"));
    }

    [Test]
    [TestCase("1.0.1.8.0.100")]
    [TestCase("1.0.2.8.0.100")]
    [TestCase("1.0.1.7.0.255")]
    [TestCase("1.0.2.7.0.255")]
    [TestCase("1.2.3.4.5.6")]
    public void MapToSerieTypeSpline(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var serieType = target.MapToSerieType(obisCode);

      // Assert
      Assert.That(serieType, Is.EqualTo("spline"));
    }

    [Test]
    [TestCase("1.0.1.8.0.200")]
    [TestCase("1.0.2.8.0.200")]
    [TestCase("6.0.1.0.0.200")]
    [TestCase("1.210.1.8.0.200")]
    [TestCase("1.220.1.8.0.200")]
    public void MapToSerieYAxisEnergyInterim(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("energyInterim"));
    }

    [Test]
    [TestCase("1.0.1.8.0.100")]
    [TestCase("1.0.2.8.0.100")]
    [TestCase("6.0.1.0.0.100")]
    [TestCase("1.210.1.8.0.100")]
    [TestCase("1.220.1.8.0.100")]
    public void MapToSerieYAxisEnergyDelta(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("energyDelta"));
    }

    [Test]
    [TestCase("1.0.1.7.0.255")]
    [TestCase("1.0.2.7.0.255")]
    [TestCase("6.0.8.0.0.255")]
    [TestCase("1.0.21.7.0.255")]
    [TestCase("1.0.41.7.0.255")]
    [TestCase("1.0.61.7.0.255")]
    [TestCase("1.0.22.7.0.255")]
    [TestCase("1.0.42.7.0.255")]
    [TestCase("1.0.62.7.0.255")]
    public void MapToSerieYAxisPower(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("power"));
    }
      
    [Test]
    [TestCase("8.0.1.0.0.200")]
    public void MapToSerieYAxisVolumeInterim(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("volumeInterim"));
    }

    [Test]
    [TestCase("6.0.2.0.0.200")]
    public void MapToSerieYAxisVolumeInterimHidden(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("volumeInterimHiddenYAxis"));
    }

    [Test]
    [TestCase("8.0.1.0.0.100")]
    public void MapToSerieYAxisVolumeDelta(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("volumeDelta"));
    }

    [Test]
    [TestCase("6.0.2.0.0.100")]
    public void MapToSerieYAxisVolumeDeltaHidden(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("volumeDeltaHiddenYAxis"));
    }

    [Test]
    [TestCase("8.0.2.0.0.255")]
    public void MapToSerieYAxisFlow(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("flow"));
    }

    [Test]
    [TestCase("6.0.9.0.0.255")]
    public void MapToSerieYAxisFlowHidden(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("flowHiddenYAxis"));
    }

    [Test]
    [TestCase("15.0.223.0.0.255")]
    public void MapToSerieYAxisTemperature(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("temp"));
    }

    [Test]
    [TestCase("6.0.10.0.0.255")]
    [TestCase("6.0.11.0.0.255")]
    public void MapToSerieYAxisTemperatureHidden(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("tempHiddenYAxis"));
    }

    [Test]
    [TestCase("15.0.223.0.2.255")]
    public void MapToSerieYAxisRelativeHumidity(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("rh"));
    }

    [Test]
    [TestCase("0.1.96.3.10.255")]
    [TestCase("0.4.96.3.10.255")]
    [TestCase("0.8.96.3.10.255")]
    public void MapToSerieYAxisDisconnectControlOutputStatusHidden(string obisCode)
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var yAxis = target.MapToSerieYAxis(obisCode);

      // Assert
      Assert.That(yAxis, Is.EqualTo("dcOutputStatusHiddenYAxis"));
    }

    private static SerieMapper CreateTarget()
    {
      return new SerieMapper();
    }
  }
}

