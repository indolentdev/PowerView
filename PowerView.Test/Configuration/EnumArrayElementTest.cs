using System;
using System.Configuration;
using System.IO.Ports;
using NUnit.Framework;
using PowerView.Configuration;

namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class EnumArrayElementTest
  {
    [Test]
    public void ValidateThrows()
    {
      // Arrange
      var target = new EnumArrayElement { Value = string.Empty };

      // Act & Assert
      Assert.That(() => target.Validate<Parity>("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
      target.Value = "AB";
      Assert.That(() => target.Validate<Parity>("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
    }

    [Test]
    public void GetItems()
    {
      // Arrange
      var target = new EnumArrayElement { Value = "Even,Odd" };

      // Act
      var items = target.GetItems<Parity>();

      // Assert
      Assert.That(items, Is.EqualTo(new [] { Parity.Even, Parity.Odd }));
    }
  }
}

