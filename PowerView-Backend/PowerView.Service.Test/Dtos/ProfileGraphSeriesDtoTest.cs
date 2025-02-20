using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class ProfileGraphSeriesDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        public void DeserializeProfileGraphSeriesDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<ProfileGraphSerieDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\"}", "Label property absent")]
        [TestCase("{\"Label\":null,\"ObisCode\":\"1.2.3.4.5.6\"}", "Label property null")]
        [TestCase("{\"Label\":\"Lbl\"}", "ObisCode property absent")]
        [TestCase("{\"Label\":\"Lbl\",\"ObisCode\":null}", "ObisCode property null")]
        public void DeserializeProfileGraphSeriesDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<ProfileGraphSerieDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeProfileGraphSeriesDto()
        {
            // Arrange
            const string json = "{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}";

            // Act
            var dto = JsonSerializer.Deserialize<ProfileGraphSerieDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Label, Is.EqualTo("Lbl"));
            Assert.That(dto.ObisCode, Is.EqualTo("1.2.3.4.5.6"));
        }



    }
}

