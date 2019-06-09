using NUnit.Framework;

namespace PowerView.Service.Test
{
  [TestFixture]
  public class ExitSignalTest
  {
    [Test]
    public void FireExitEvent()
    {
      // Arrange
      var target = new ExitSignal();
      bool signalled = false;
      target.Exit += (sender, e) => { signalled = true; };

      // Act
      target.FireExitEvent();

      // Assert
      Assert.That(signalled, Is.True);
    }

    [Test]
    public void FireExitEventNoSubscriber()
    {
      // Arrange
      var target = new ExitSignal();

      // Act
      target.FireExitEvent();

      // Assert
      // Does not throw
    }

  }
}
