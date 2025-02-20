using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class ImportEnableDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"Enabled\":\"BAD\"}", "Enabled invalid")]
        public void DeserializeImportEnableDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<ImportEnableDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"Enabled\":null}", "Enabled null")]
        public void DeserializeImportEnableDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<ImportEnableDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeImportEnableDto()
        {
            // Arrange
            var json = "{\"Enabled\":true}";

            // Act
            var dto = JsonSerializer.Deserialize<ImportEnableDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Enabled, Is.True);
        }

    }
}
