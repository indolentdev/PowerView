using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class DisconnectCacheTest
  {
    [Test]
    public void SynchronizeRulesAdd()
    {
      // Arrange
      var target = CreateTarget();
      var rule = GetRule();
      var factory = new Mock<Func<IDisconnectRule, IDisconnectCacheItem>>();

      // Act
      target.SynchronizeRules(new[] { rule }, factory.Object);

      // Assert
      Assert.That(target.Count, Is.EqualTo(1));
      factory.Verify(x => x(rule));
    }

    [Test]
    public void SynchronizeRulesAddSame()
    {
      // Arrange
      var target = CreateTarget();
      var rule = GetRule();
      target.SynchronizeRules(new[] { rule }, GetFactory());
      var factory = new Mock<Func<IDisconnectRule, IDisconnectCacheItem>>();

      // Act
      target.SynchronizeRules(new[] { rule }, factory.Object);

      // Assert
      Assert.That(target.Count, Is.EqualTo(1));
      factory.Verify(x => x(It.IsAny<IDisconnectRule>()), Times.Never());
    }

    [Test]
    public void SynchronizeRulesRemove()
    {
      // Arrange
      var target = CreateTarget();
      var rule = GetRule();
      target.SynchronizeRules(new[] { rule }, GetFactory());

      // Act
      target.SynchronizeRules(new IDisconnectRule[0], GetFactory());

      // Assert
      Assert.That(target.Count, Is.EqualTo(0));
    }

    [Test]
    public void Add()
    {
      // Arrange
      var target = CreateTarget();
      var meterSerieName = new SeriesName("TheMeter", ObisCode.ElectrActualPowerP14L1);
      var rule = GetRule(meterSerieName);
      var cacheItem = new Mock<IDisconnectCacheItem>();
      var factory = GetFactory(cacheItem.Object);
      target.SynchronizeRules(new[] { rule }, factory);
      var lr1 = new LiveReading(meterSerieName.Label, "1", DateTime.UtcNow, new[] {
        new RegisterValue(meterSerieName.ObisCode, 1, 1, Unit.Watt),
        new RegisterValue(ObisCode.ElectrActualPowerP14L2, 2, 2, Unit.Watt)});
      var lr2 = new LiveReading("OtherMeter", "2", DateTime.UtcNow, new[] {
        new RegisterValue(meterSerieName.ObisCode, 3, 3, Unit.Watt),
        new RegisterValue(ObisCode.ElectrActualPowerP14L2, 4, 4, Unit.Watt)});

      // Act
      target.Add(new [] { lr1, lr2 });

      // Assert
      cacheItem.Verify(ci => ci.Add(It.Is<IEnumerable<TimeRegisterValue>>(x => x.Count() == 1 && 
        x.SequenceEqual(new [] { new TimeRegisterValue(lr1.SerialNumber, lr1.Timestamp, 1, 1, Unit.Watt) }) )));
    }

    [Test]
    public void Calculate()
    {
      // Arrange
      var target = CreateTarget();
      var rule = GetRule();
      var cacheItem = new Mock<IDisconnectCacheItem>();
      var factory = GetFactory(cacheItem.Object);
      target.SynchronizeRules(new[] { rule }, factory);
      var time = DateTime.UtcNow;

      // Act
      target.Calculate(time);

      // Assert
      cacheItem.Verify(x => x.Calculate(time));
    }

    [Test]
    public void GetStatus()
    {
      // Arrange
      var target = CreateTarget();
      var name = new SeriesName("Relay", "0.1.96.3.10.255");
      var rule = GetRule(name: name);
      var cacheItem = new Mock<IDisconnectCacheItem>();
      cacheItem.Setup(ci => ci.Rule).Returns(rule);
      cacheItem.Setup(ci => ci.Connected).Returns(true);
      var factory = GetFactory(cacheItem.Object);
      target.SynchronizeRules(new[] { rule }, factory);

      // Act
      var status = target.GetStatus();

      // Assert
      Assert.That(status.Count, Is.EqualTo(1));
      Assert.That(status, Contains.Key(name));
      Assert.That(status[name], Is.EqualTo(cacheItem.Object.Connected));
    }

    private static DisconnectCache CreateTarget()
    {
      return new DisconnectCache();
    }

    private static IDisconnectRule GetRule(SeriesName evaluationName = null, SeriesName name = null)
    {
      if (evaluationName == null)
      {
        evaluationName = new SeriesName("lbl", "1.1.21.7.0.255");
      }
      if (name == null)
      {
        name = new SeriesName("lbl2", "0.5.96.3.10.255");
      }
      var rule = new Mock<IDisconnectRule>();
      rule.Setup(x => x.Name).Returns(name);
      rule.Setup(x => x.EvaluationName).Returns(evaluationName);
      return rule.Object;
    }

    private static Func<IDisconnectRule, IDisconnectCacheItem> GetFactory(IDisconnectCacheItem cacheItem = null)
    {
      var factory = new Mock<Func<IDisconnectRule, IDisconnectCacheItem>>();
      factory.Setup(x => x(It.IsAny<IDisconnectRule>())).Returns(cacheItem);
      return factory.Object;
    }

  }
}
