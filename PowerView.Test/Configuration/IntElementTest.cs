using System;
using System.Configuration;
using NUnit.Framework;
using PowerView.Configuration;

namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class IntElementTest
  {
    [Test]
    public void ValidateThrows()
    {
      // Arrange
      var target = new IntElement { Value = string.Empty };

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
      var target = new IntElement { Value = "1234" };

      // Act
      var i = target.GetValueAsInt();

      // Assert
      Assert.That(i, Is.EqualTo(1234));
    }
  }
}

