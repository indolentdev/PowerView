using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class SeriesColorSetDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        public void DeserializeSeriesColorSetDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<SerieColorSetDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{}", "Items property absent")]
        [TestCase("{\"Items\":null}", "Items property null")]
        public void DeserializeSeriesColorSetDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<SerieColorSetDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeSeriesColorSetDto()
        {
            // Arrange
            const string json = "{\"Items\":[]}";

            // Act
            var dto = JsonSerializer.Deserialize<SerieColorSetDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Items, Is.Empty);
        }

    }
}

