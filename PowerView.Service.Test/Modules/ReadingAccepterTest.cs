using System;
using NUnit.Framework;
using Moq;
using PowerView.Service.Modules;
using PowerView.Service.EventHub;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.Test.Modules
{
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
                                                  new [] { new RegisterValue(ObisCode.ElectrActiveEnergyA14Period, 1, 0, Unit.WattHour)}) };

      // Act
      target.Accept(liveReadings);

      // Assert
      liveReadingRepository.Verify(lrr => lrr.Add(liveReadings));
      hub.Verify(h => h.Signal(liveReadings));
    }
  }

}
