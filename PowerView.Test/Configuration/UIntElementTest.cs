using System;
using System.Configuration;
using NUnit.Framework;
using PowerView.Configuration;

namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class UIntElementTest
  {
    [Test]
    public void ValidateThrows()
    {
      // Arrange
      var target = new UIntElement { Value = string.Empty };

      // Act & Assert
      Assert.That(() => target.Validate("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
      target.Value = "AB";
      Assert.That(() => target.Validate("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
      target.Value = "-1";
      Assert.That(() => target.Validate("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
    }

    [Test]
    public void GetValueAs()
    {
      // Arrange
      var target = new UIntElement { Value = "1234" };

      // Act
      var i = target.GetValueAsUInt();

      // Assert
      Assert.That(i, Is.EqualTo(1234));
    }
  }
}

