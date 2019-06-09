using System;
using System.Configuration;
using NUnit.Framework;
using PowerView.Configuration;

namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class ByteArrayElementTest
  {
    [Test]
    public void ValidateThrows()
    {
      // Arrange
      var target = new ByteArrayElement { Value = string.Empty };

      // Act & Assert
      Assert.That(() => target.Validate("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
      target.Value = "AB";
      Assert.That(() => target.Validate("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
      target.Value = "1.2";
      Assert.That(() => target.Validate("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
    }

    [Test]
    public void Enumerate()
    {
      // Arrange
      var target = new ByteArrayElement { Value = "1,2,3,4,5" };

      // Act & Assert
      Assert.That(target, Is.EqualTo(new byte[] { 1,2,3,4,5 }));
    }
  }
}

