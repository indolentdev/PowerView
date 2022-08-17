using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using PowerView.Service.Controllers;
using PowerView.Service.EventHub;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.Test.Controllers;

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
    public void Accept()
    {
        // Arrange
        var liveReadingRepository = new Mock<ILiveReadingRepository>();
        var hub = new Mock<IHub>();
        var target = new ReadingAccepter(liveReadingRepository.Object, hub.Object);
        var liveReadings = new[] { new LiveReading("lbl", "sn1", DateTime.UtcNow,
                                                  new [] { new RegisterValue(ObisCode.ElectrActiveEnergyA14, 1, 0, Unit.WattHour)}) };

        // Act
        target.Accept(liveReadings);

        // Assert
        liveReadingRepository.Verify(lrr => lrr.Add(liveReadings));
        hub.Verify(h => h.Signal(liveReadings));
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
        var liveReadings = new[] { new LiveReading("lbl", "sn1", DateTime.UtcNow,
                                                  new [] { new RegisterValue(obisCode, 1, 0, Unit.WattHour)}) };

        // Act
        target.Accept(liveReadings);

        // Assert
        liveReadingRepository.Verify(lrr => lrr.Add(It.Is<IList<LiveReading>>(p => p.Count == 0)));
        hub.Verify(h => h.Signal(It.Is<IList<LiveReading>>(p => p.Count == 0)));
    }

}
