using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
  [TestFixture]
  public class BoxCryptorTest
  {
    [Test]
    public void EncryptThrows()
    {
      // Arrange
      var target = new BoxCryptor();
      const string plainText = "This is the plain text";
      var ivDateTime = DateTime.UtcNow;

      // Act & Assert
      Assert.That(() => target.Encrypt(null, ivDateTime), Throws.ArgumentNullException);
      Assert.That(() => target.Encrypt(string.Empty, ivDateTime), Throws.ArgumentNullException);
      Assert.That(() => target.Encrypt(plainText, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.Encrypt(plainText, new DateTime()), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Encrypt()
    {
      // Arrange
      var target = new BoxCryptor();
      const string plainText = "This is the plain text";
      var ivDateTime = DateTime.UtcNow;

      // Act
      var cipherText = target.Encrypt(plainText, ivDateTime);

      // Assert
      Assert.That(cipherText, Is.Not.EqualTo(plainText));
    }

    [Test]
    public void DecryptThrows()
    {
      // Arrange
      var target = new BoxCryptor();
      const string cipherText = "dummy cipher";
      var ivDateTime = DateTime.UtcNow;

      // Act & Assert
      Assert.That(() => target.Decrypt(null, ivDateTime), Throws.ArgumentNullException);
      Assert.That(() => target.Decrypt(string.Empty, ivDateTime), Throws.ArgumentNullException);
      Assert.That(() => target.Decrypt(cipherText, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.Decrypt(cipherText, new DateTime()), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    [TestCase("bad base64 cipher", typeof(BoxCryptorException))]
    [TestCase("QmFkIGNpcGhlciB0ZXh0", typeof(BoxCryptorException))]  // base64 - cipher wrong length
    [TestCase("MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=", typeof(BoxCryptorException))] // base64 - wrong pkcs7 padding
    [TestCase("A+BNQVhvbSxd2W8k7sgLoJ2MhPJ9j8kI/13X6mgfg40==", typeof(BoxCryptorException))] // base64 - ciper text does not match iv
    public void DecryptThrows2(string cipherText, Type exceptionType)
    {
      // Arrange
      var target = new BoxCryptor();
      var ivDateTime = DateTime.UtcNow;

      // Act & Assert
      Assert.That(() => target.Decrypt(cipherText, ivDateTime), Throws.TypeOf(exceptionType));
    }


    [Test]
    public void EncryptIvDependent()
    {
      // Arrange
      var target = new BoxCryptor();
      const string plainText = "This is the plain text";
      var ivDateTime = DateTime.UtcNow;

      // Act
      var cipherText1 = target.Encrypt(plainText, ivDateTime);
      var cipherText2 = target.Encrypt(plainText, ivDateTime + TimeSpan.FromDays(1));

      // Assert
      Assert.That(cipherText2, Is.Not.EqualTo(cipherText1));
    }

    [Test]
    public void EncryptDecrypt()
    {
      // Arrange
      var target = new BoxCryptor();
      const string plainText = "This is the plain text";
      var ivDateTime = DateTime.UtcNow;

      // Act
      var cipherText = target.Encrypt(plainText, ivDateTime);
      var decryptedPlainText = target.Decrypt(cipherText, ivDateTime);

      // Assert
      Assert.That(decryptedPlainText, Is.EqualTo(plainText));
    }

  }
}
