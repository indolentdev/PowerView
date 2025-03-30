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
            const string plainText = "This is the plain text";
            var ivDateTime = DateTime.UtcNow;

            // Act & Assert
            Assert.That(() => BoxCryptor.Encrypt(null, ivDateTime), Throws.ArgumentNullException);
            Assert.That(() => BoxCryptor.Encrypt(string.Empty, ivDateTime), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => BoxCryptor.Encrypt(plainText, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => BoxCryptor.Encrypt(plainText, new DateTime()), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Encrypt()
        {
            // Arrange
            const string plainText = "This is the plain text";
            var ivDateTime = DateTime.UtcNow;

            // Act
            var cipherText = BoxCryptor.Encrypt(plainText, ivDateTime);

            // Assert
            Assert.That(cipherText, Is.Not.EqualTo(plainText));
        }

        [Test]
        public void DecryptThrows()
        {
            // Arrange
            const string cipherText = "dummy cipher";
            var ivDateTime = DateTime.UtcNow;

            // Act & Assert
            Assert.That(() => BoxCryptor.Decrypt(null, ivDateTime), Throws.ArgumentNullException);
            Assert.That(() => BoxCryptor.Decrypt(string.Empty, ivDateTime), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => BoxCryptor.Decrypt(cipherText, DateTime.Now), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => BoxCryptor.Decrypt(cipherText, new DateTime()), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase("bad base64 cipher", typeof(BoxCryptorException))]
        [TestCase("QmFkIGNpcGhlciB0ZXh0", typeof(BoxCryptorException))]  // base64 - cipher wrong length
        [TestCase("MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=", typeof(BoxCryptorException))] // base64 - wrong pkcs7 padding
        [TestCase("A+BNQVhvbSxd2W8k7sgLoJ2MhPJ9j8kI/13X6mgfg40==", typeof(BoxCryptorException))] // base64 - ciper text does not match iv
        public void DecryptThrows2(string cipherText, Type exceptionType)
        {
            // Arrange
            var ivDateTime = DateTime.UtcNow;

            // Act & Assert
            Assert.That(() => BoxCryptor.Decrypt(cipherText, ivDateTime), Throws.TypeOf(exceptionType));
        }


        [Test]
        public void EncryptIvDependent()
        {
            // Arrange
            const string plainText = "This is the plain text";
            var ivDateTime = DateTime.UtcNow;

            // Act
            var cipherText1 = BoxCryptor.Encrypt(plainText, ivDateTime);
            var cipherText2 = BoxCryptor.Encrypt(plainText, ivDateTime + TimeSpan.FromDays(1));

            // Assert
            Assert.That(cipherText2, Is.Not.EqualTo(cipherText1));
        }

        [Test]
        public void EncryptDecrypt()
        {
            // Arrange
            const string plainText = "This is the plain text";
            var ivDateTime = DateTime.UtcNow;

            // Act
            var cipherText = BoxCryptor.Encrypt(plainText, ivDateTime);
            var decryptedPlainText = BoxCryptor.Decrypt(cipherText, ivDateTime);

            // Assert
            Assert.That(decryptedPlainText, Is.EqualTo(plainText));
        }

    }
}
