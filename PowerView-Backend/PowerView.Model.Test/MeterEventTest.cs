using System;
using NUnit.Framework;
using PowerView.Model;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class MeterEventTest
  {
    [Test]
    public void ConstructorThrows() 
    {
      // Arrange
      const string label = "Label";
      var detectTimestamp = DateTime.UtcNow;
      const bool flag = true;
      var amplification = new LocalMeterEventAmplification();

      // Act & Assert
      Assert.That(() => new MeterEvent(null, detectTimestamp, flag, amplification), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => new MeterEvent(string.Empty, detectTimestamp, flag, amplification), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new MeterEvent(label, DateTime.Now, flag, amplification), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => new MeterEvent(label, detectTimestamp, flag, null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void ConstructorAndProperties() 
    {
      // Arrange
      const string label = "Label";
      var detectTimestamp = DateTime.UtcNow;
      const bool flag = true;
      var amplification = new LocalMeterEventAmplification();

      // Act
      var target = new MeterEvent(label, detectTimestamp, flag, amplification);

      // Assert
      Assert.That(target.Label, Is.EqualTo(label));
      Assert.That(target.DetectTimestamp, Is.EqualTo(detectTimestamp));
      Assert.That(target.Flag, Is.EqualTo(flag));
      Assert.That(target.Amplification, Is.SameAs(amplification));
    }

    private class LocalMeterEventAmplification : IMeterEventAmplification
    {
      public string GetMeterEventType()
      {
        return "Local";
      }
    }
  }
}

