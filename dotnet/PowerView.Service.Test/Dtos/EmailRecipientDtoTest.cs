using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class EmailRecipientDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        public void DeserializeEmailRecipientDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<EmailRecipientDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"EmailAddress\":\"a@b.com\"}", "Name absent")]
        [TestCase("{\"Name\":null,\"EmailAddress\":\"a@b.com\"}", "Name null")]
        [TestCase("{\"Name\":\"theName\"}", "EmailAddress absent")]
        [TestCase("{\"Name\":\"theName\",\"EmailAddress\":null}", "EmailAddress null")]
        [TestCase("{\"Name\":\"theName\",\"EmailAddress\":\"no_good\"}", "EmailAddress bad")]
        public void DeserializeEmailRecipientDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<EmailRecipientDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeEmailRecipientDto()
        {
            // Arrange
            const string json = "{\"Name\":\"theName\",\"EmailAddress\":\"a@b.com\"}";

            // Act
            var dto = JsonSerializer.Deserialize<EmailRecipientDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Name, Is.EqualTo("theName"));
            Assert.That(dto.EmailAddress, Is.EqualTo("a@b.com"));
        }

    }
}

