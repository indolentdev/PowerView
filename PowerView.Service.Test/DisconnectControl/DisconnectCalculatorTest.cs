using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.DisconnectControl;

namespace PowerView.Service.Test.DisconnectControl
{
  [TestFixture]
  public class DisconnectCalculatorTest
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new DisconnectCalculator(null), Throws.ArgumentNullException);
    }

    [Test]
    public void SynchronizeAndCalculate()
    {
      // Arrange
      var time = DateTime.UtcNow;
      var disconnectCache = new Mock<IDisconnectCache>();
      var liveReadings = new List<Reading>();
      var disconnectRuleRepository = new Mock<IDisconnectRuleRepository>();
      var disconnectRule = new DisconnectRule(new SeriesName("lbl", ObisCode.ColdWaterFlow1), new SeriesName("other", ObisCode.ElectrActualPowerP23L1),
                                             TimeSpan.FromMinutes(30), 1500, 300, Unit.Watt);
      disconnectRuleRepository.Setup(x => x.GetDisconnectRules()).Returns(new [] { disconnectRule });
      var target = new DisconnectCalculator(disconnectRuleRepository.Object);


      // Act
      target.SynchronizeAndCalculate(time, disconnectCache.Object, liveReadings);

      // Assert
      disconnectRuleRepository.Verify(x => x.GetDisconnectRules());
      disconnectCache.Verify(x => x.SynchronizeRules(It.Is<ICollection<IDisconnectRule>>(c => c.Count == 1 && c.Contains(disconnectRule))));
      disconnectCache.Verify(x => x.Add(liveReadings));
      disconnectCache.Verify(x => x.Calculate(time));
    }

  }
}
