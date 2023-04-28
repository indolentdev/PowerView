using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.EnergiDataService;

namespace PowerView.Service.Test.EnergiDataService
{
    [TestFixture]
    public class ElSpotPriceRecordDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("\"SpotPriceDkk\":12.34,\"SpotPriceEur\":56.78}", "HourUtc absent")]
        [TestCase("{\"HourUtc\":\"BadDateTime\",\"SpotPriceDkk\":12.34,\"SpotPriceEur\":56.78}", "HourUtc bad")]
        [TestCase("{\"HourUtc\":\"2023-04-17T20:00:00\",\"SpotPriceDkk\":\"12.34\",\"SpotPriceEur\":56.78}", "SpotPriceDKK string")]
        [TestCase("{\"HourUtc\":\"2023-04-17T20:00:00\",\"SpotPriceDkk\":12.34,\"SpotPriceEur\":\"56.78\"}", "SpotPriceEUR string")]
        public void DeserializeInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<ElSpotPriceRecordDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"HourUtc\":null,\"SpotPriceDkk\":12.34,\"SpotPriceEur\":56.78}", "HourUtc null")]
        [TestCase("{\"HourUtc\":\"2023-04-17T20:00:00\",\"SpotPriceEur\":56.78}", "SpotPriceDkk absent")]
        [TestCase("{\"HourUtc\":\"2023-04-17T20:00:00\",\"SpotPriceDkk\":null,\"SpotPriceEur\":56.78}", "SpotPriceDkk null")]
        [TestCase("{\"HourUtc\":\"2023-04-17T20:00:00\",\"SpotPriceDkk\":12.34}", "SpotPriceEur absent")]
        [TestCase("{\"HourUtc\":\"2023-04-17T20:00:00\",\"SpotPriceDkk\":12.34,\"SpotPriceEur\":null}", "SpotPriceEur null")]
        public void DeserializeInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<ElSpotPriceRecordDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void Deserialize()
        {
            // Arrange
            const string json = "{\"HourUtc\":\"2023-04-17T20:00:00\",\"SpotPriceDkk\":12.34,\"SpotPriceEur\":56.78}";

            // Act
            var dto = JsonSerializer.Deserialize<ElSpotPriceRecordDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.HourUtc, Is.EqualTo(new DateTime(2023, 4, 17, 20, 0, 0)));
            Assert.That(dto.SpotPriceDkk, Is.EqualTo(12.34));
            Assert.That(dto.SpotPriceEur, Is.EqualTo(56.78));
        }

    }
}

