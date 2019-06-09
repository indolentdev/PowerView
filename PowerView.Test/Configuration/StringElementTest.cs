using System;
using System.Configuration;
using NUnit.Framework;
using PowerView.Configuration;

namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class StringElementTest
  {
    [Test]
    public void ValidateThrows()
    {
      // Arrange
      var target = new StringElement { Value = string.Empty };

      // Act & Assert
      Assert.That(() => target.Validate("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
    }
  }
}

