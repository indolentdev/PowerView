using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class SeriesColorDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        public void DeserializeSeriesColorDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<SerieColorDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Color\":\"#000000\"}", "Label property absent")]
        [TestCase("{\"Label\":null,\"ObisCode\":\"1.2.3.4.5.6\",\"Color\":\"#000000\"}", "Label property null")]
        [TestCase("{\"Label\":\"Lbl\",\"Color\":\"#000000\"}", "ObisCode property absent")]
        [TestCase("{\"Label\":\"Lbl\",\"ObisCode\":null,\"Color\":\"#000000\"}", "ObisCode property null")]
        [TestCase("{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}", "Color property absent")]
        [TestCase("{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\",\"Color\":null}", "Color property null")]
        public void DeserializeSeriesColorDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<SerieColorDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeSeriesColorDto()
        {
            // Arrange
            const string json = "{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\",\"Color\":\"#000000\"}";

            // Act
            var dto = JsonSerializer.Deserialize<SerieColorDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Label, Is.EqualTo("Lbl"));
            Assert.That(dto.ObisCode, Is.EqualTo("1.2.3.4.5.6"));
            Assert.That(dto.Color, Is.EqualTo("#000000"));
        }



    }
}

