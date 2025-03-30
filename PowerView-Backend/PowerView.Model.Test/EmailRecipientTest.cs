using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class EmailRecipientTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            const string name = "name";
            const string emailAddress = "a@b.com";

            // Act & Assert
            Assert.That(() => new EmailRecipient(null, emailAddress), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new EmailRecipient(string.Empty, emailAddress), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new EmailRecipient(name, null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new EmailRecipient(name, string.Empty), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new EmailRecipient(name, "WrongEmailAddressStructure"), Throws.TypeOf<FormatException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            const string name = "name";
            const string emailAddress = "a@b.com";

            // Act
            var target = new EmailRecipient(name, emailAddress);

            // Assert
            Assert.That(target.Name, Is.EqualTo(name));
            Assert.That(target.EmailAddress, Is.EqualTo(emailAddress));
        }
    }
}
