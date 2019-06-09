using System;
using System.Configuration;
using System.IO.Ports;
using NUnit.Framework;
using PowerView.Configuration;

namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class EnumElementTest
  {
    [Test]
    public void ValidateThrows()
    {
      // Arrange
      var target = new EnumElement { Value = string.Empty };

      // Act & Assert
      Assert.That(() => target.Validate<Parity>("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
      target.Value = "Undefined";
      Assert.That(() => target.Validate<Parity>("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
    }

    [Test]
    public void GetValueAs()
    {
      // Arrange
      var target = new EnumElement { Value = "None" };

      // Act
      var parity = target.GetValueAs<Parity>();

      // Assert
      Assert.That(parity, Is.EqualTo(Parity.None));
    }
  }
}

