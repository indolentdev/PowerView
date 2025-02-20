using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Service.EnergiDataService;

namespace PowerView.Service.Test.EnergiDataService
{
    [TestFixture]
    public class ElSpotPricesDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        public void DeserializeInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<ElSpotPricesDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{}", "Records absent")]
        [TestCase("{\"Records\":null}", "Records null")]
        public void DeserializeInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<ElSpotPricesDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void Deserialize()
        {
            // Arrange
            const string json = "{\"Records\":[]}";

            // Act
            var dto = JsonSerializer.Deserialize<ElSpotPricesDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Records, Is.Empty);
        }

    }
}

