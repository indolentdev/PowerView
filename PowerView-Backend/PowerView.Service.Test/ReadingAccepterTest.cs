using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using PowerView.Service.EventHub;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.Test;

[TestFixture]
public class ReadingAccepterTest
{
    [Test]
    public void ConstructorThrows()
    {
        // Arrange
        var liveReadingRepository = new Mock<ILiveReadingRepository>();
        var hub = new Mock<IHub>();

        // Act & Assert
        Assert.That(() => new ReadingAccepter(null, hub.Object), Throws.ArgumentNullException);
        Assert.That(() => new ReadingAccepter(liveReadingRepository.Object, null), Throws.ArgumentNullException);
    }

    [Test]
    [TestCase("1.0.1.8.0.255")]
    public void Accept(string obisCode)
    {
        // Arrange
        var liveReadingRepository = new Mock<ILiveReadingRepository>();
        var hub = new Mock<IHub>();
        var target = new ReadingAccepter(liveReadingRepository.Object, hub.Object);
        var liveReadings = new[] { new Reading("lbl", "sn1", DateTime.UtcNow,
                                                  new [] { new RegisterValue(obisCode, 1, 0, Unit.WattHour)}) };

        // Act
        target.Accept(liveReadings);

        // Assert
        liveReadingRepository.Verify(lrr => lrr.Add(liveReadings));
        hub.Verify(h => h.Signal(liveReadings));
    }

    [Test]
    [TestCase("1.68.25.67.0.255")] // Electricity Income/expense amount
    [TestCase("1.68.25.68.0.255")] // Electricity Income/expense amount
    public void AcceptButSkipHubSignal(string obisCode)
    {
        // Arrange
        var liveReadingRepository = new Mock<ILiveReadingRepository>();
        var hub = new Mock<IHub>();
        var target = new ReadingAccepter(liveReadingRepository.Object, hub.Object);
        var liveReadings = new[] { new Reading("lbl", "sn1", DateTime.UtcNow,
                                                  new [] { new RegisterValue(obisCode, 1, 0, Unit.WattHour)}) };

        // Act
        target.Accept(liveReadings);

        // Assert
        liveReadingRepository.Verify(lrr => lrr.Add(liveReadings));
        hub.Verify(h => h.Signal(It.IsAny<IList<Reading>>()), Times.Never);
    }

    [Test]
    [TestCase("1.65.1.8.0.255")] // Delta
    [TestCase("1.66.1.8.0.255")] // Period
    [TestCase("1.67.2.7.0.255")] // Average
    public void AcceptFiltersUtilitySpecificObisCodes(string obisCode)
    {
        // Arrange
        var liveReadingRepository = new Mock<ILiveReadingRepository>();
        var hub = new Mock<IHub>();
        var target = new ReadingAccepter(liveReadingRepository.Object, hub.Object);
        var liveReadings = new[] { new Reading("lbl", "sn1", DateTime.UtcNow,
                                                  new [] { new RegisterValue(obisCode, 1, 0, Unit.WattHour)}) };

        // Act
        target.Accept(liveReadings);

        // Assert
        liveReadingRepository.Verify(lrr => lrr.Add(It.Is<IList<Reading>>(p => p.Count == 0)));
        hub.Verify(h => h.Signal(It.Is<IList<Reading>>(p => p.Count == 0)));
    }

    [Test]
    public void AcceptReadingContainsSomeOkAndSomeNotOk()
    {
        // Arrange
        var liveReadingRepository = new Mock<ILiveReadingRepository>();
        var hub = new Mock<IHub>();
        var target = new ReadingAccepter(liveReadingRepository.Object, hub.Object);
        var liveReadings = new[] { new Reading("lbl", "sn1", DateTime.UtcNow,
                                                  new [] {
                                                    new RegisterValue(ObisCode.ElectrActiveEnergyA14, 1, 0, Unit.WattHour),
                                                    new RegisterValue(ObisCode.ElectrActiveEnergyA14Delta, 1, 0, Unit.WattHour)
                                                  }) };

        // Act
        target.Accept(liveReadings);

        // Assert
        liveReadingRepository.Verify(lrr => lrr.Add(It.Is<IList<Reading>>(p => p.Count == 1 && p.First().GetRegisterValues().Count == 1 && p.First().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyA14)));
        hub.Verify(h => h.Signal(It.Is<IList<Reading>>(p => p.Count == 1 && p.First().GetRegisterValues().Count == 1 && p.First().GetRegisterValues().First().ObisCode == ObisCode.ElectrActiveEnergyA14)));
    }

}
