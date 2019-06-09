using System;
using System.Configuration;
using NUnit.Framework;
using PowerView.Configuration;

namespace PowerView.Test.Configuration
{
  [TestFixture]
  public class UriElementTest
  {
    [Test]
    public void ValidateThrows()
    {
      // Arrange
      var target = new UriElement { Value = string.Empty };

      // Act & Assert
      Assert.That(() => target.Validate("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
      target.Value = "AB";
      Assert.That(() => target.Validate("AttrName"), Throws.TypeOf<ConfigurationErrorsException>());
    }

    [Test]
    public void GetValueAs()
    {
      // Arrange
      var target = new UriElement { Value = "http://localhost:12345" };

      // Act
      var uri = target.GetValueAsUri();

      // Assert
      Assert.That(uri, Is.EqualTo(new Uri("http://localhost:12345")));
    }
  }
}

