using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class SmtpConfigDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"Server\":\"Svr\",\"Port\":\"A1234\",\"User\":\"Usr\",\"Auth\":\"Ath\",\"Email\":\"a@b.com\"}", "Port invalid")]
        public void DeserializeSmtpConfigDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<SmtpConfigDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"Port\":1234,\"User\":\"Usr\",\"Auth\":\"Ath\",\"Email\":\"Eml\"}", "Server absent")]
        [TestCase("{\"Server\":null,\"Port\":1234,\"User\":\"Usr\",\"Auth\":\"Ath\",\"Email\":\"Eml\"}", "Server null")]
        [TestCase("{\"Server\":\"Svr\",\"User\":\"Usr\",\"Auth\":\"Ath\",\"Email\":\"Eml\"}", "Port absent")]
        [TestCase("{\"Server\":\"Svr\",\"Port\":null,\"User\":\"Usr\",\"Auth\":\"Ath\",\"Email\":\"Eml\"}", "Port null")]
        [TestCase("{\"Server\":\"Svr\",\"Port\":1234,\"Auth\":\"Ath\",\"Email\":\"Eml\"}", "User absent")]
        [TestCase("{\"Server\":\"Svr\",\"Port\":1234,\"User\":null,\"Auth\":\"Ath\",\"Email\":\"Eml\"}", "User null")]
        [TestCase("{\"Server\":\"Svr\",\"Port\":1234,\"User\":\"Usr\",\"Email\":\"Eml\"}", "Auth absent")]
        [TestCase("{\"Server\":\"Svr\",\"Port\":1234,\"User\":\"Usr\",\"Auth\":null,\"Email\":\"Eml\"}", "Auth null")]
        [TestCase("{\"Server\":\"Svr\",\"Port\":1234,\"User\":\"Usr\",\"Auth\":\"Ath\"}", "Email absent")]
        [TestCase("{\"Server\":\"Svr\",\"Port\":1234,\"User\":\"Usr\",\"Auth\":\"Ath\",\"Email\":null}", "Email null")]
        [TestCase("{\"Server\":\"Svr\",\"Port\":1234,\"User\":\"Usr\",\"Auth\":\"Ath\",\"Email\":\"NotEmail\"}", "Email null")]
        public void DeserializeSmtpConfigDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<SmtpConfigDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeSmtpConfigDto()
        {
            // Arrange
            const string json = "{\"Server\":\"Svr\",\"Port\":1234,\"User\":\"Usr\",\"Auth\":\"Ath\",\"Email\":\"a@b.com\"}";

            // Act
            var dto = JsonSerializer.Deserialize<SmtpConfigDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Server, Is.EqualTo("Svr"));
            Assert.That(dto.Port, Is.EqualTo(1234));
            Assert.That(dto.User, Is.EqualTo("Usr"));
            Assert.That(dto.Auth, Is.EqualTo("Ath"));
            Assert.That(dto.Email, Is.EqualTo("a@b.com"));
        }

    }
}

