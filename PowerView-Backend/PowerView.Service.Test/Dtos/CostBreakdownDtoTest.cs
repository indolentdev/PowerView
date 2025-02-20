using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class CostBreakdownDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"Title\":\"TheTitle\",\"Currency\":\"XYZ\",\"Vat\":25}", "Currency invalid value")]
        [TestCase("{\"Title\":\"TheTitle\",\"Currency\":\"DKK\",\"Vat\":\"BAD\"}", "Vat invalid value")]
        public void DeserializeCostBreakdownDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<CostBreakdownDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"Currency\":\"DKK\",\"Vat\":25}", "Title absent")]
        [TestCase("{\"Title\":null,\"Currency\":\"DKK\",\"Vat\":25}", "Title null")]
        [TestCase("{\"Title\":\"\",\"Currency\":\"DKK\",\"Vat\":25}", "Title min length")]
        [TestCase("{\"Title\":\"1234567890123456789012345678901\",\"Currency\":\"DKK\",\"Vat\":25}", "Title max length")]
        [TestCase("{\"Title\":\"TheTitle\",\"Vat\":25}", "Currency absent")]
        [TestCase("{\"Title\":\"TheTitle\",\"Currency\":null,\"Vat\":25}", "Currency null")]
        [TestCase("{\"Title\":\"TheTitle\",\"Currency\":\"DKK\"}", "Vat property absent")]
        [TestCase("{\"Title\":\"TheTitle\",\"Currency\":\"DKK\",\"Vat\":null}", "Vat property null")]
        public void DeserializeCostBreakdownDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<CostBreakdownDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        [TestCase(Unit.Eur, "Eur")]
        [TestCase(Unit.Dkk, "Dkk")]
        [TestCase(Unit.Eur, "EUR")]
        [TestCase(Unit.Dkk, "DKK")]
        public void DeserializeCostBreakdownDto(Unit currency, string currencyString)
        {
            // Arrange
            var json = "{\"Title\":\"TheTitle\",\"Currency\":\"" + currencyString + "\",\"Vat\":25}";

            // Act
            var dto = JsonSerializer.Deserialize<CostBreakdownDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Title, Is.EqualTo("TheTitle"));
            Assert.That(dto.Currency, Is.EqualTo(currency));
            Assert.That(dto.Vat, Is.EqualTo(25));
        }

    }
}

