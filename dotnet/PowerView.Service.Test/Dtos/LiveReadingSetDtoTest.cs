using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class LiveReadingSetDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        public void DeserializeLiveReadingSetDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<LiveReadingSetDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{}", "Items array absent")]
        public void DeserializeLiveReadingSetDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<LiveReadingSetDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeLiveReadingSetDto()
        {
            // Arrange
            const string json = "{\"Items\":[]}";

            // Act
            var dto = JsonSerializer.Deserialize<LiveReadingSetDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Items, Is.Empty);
        }

    }
}

