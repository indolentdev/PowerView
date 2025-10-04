using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.EnergiDataService;

namespace PowerView.Service.Test.EnergiDataService
{
    [TestFixture]
    public class DayAheadPriceRecordDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("\"DayAheadPriceDkk\":12.34,\"DayAheadPriceEur\":56.78}", "TimeUtc absent")]
        [TestCase("{\"TimeUtc\":\"BadDateTime\",\"DayAheadPriceDkk\":12.34,\"DayAheadPriceEur\":56.78}", "TimeUtc bad")]
        [TestCase("{\"TimeUtc\":\"2023-04-17T20:00:00\",\"DayAheadPriceDkk\":\"12.34\",\"DayAheadPriceEur\":56.78}", "DayAheadPriceDKK string")]
        [TestCase("{\"TimeUtc\":\"2023-04-17T20:00:00\",\"DayAheadPriceDkk\":12.34,\"DayAheadPriceEur\":\"56.78\"}", "DayAheadPriceEUR string")]
        public void DeserializeInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<DayAheadPriceRecordDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"TimeUtc\":null,\"DayAheadPriceDkk\":12.34,\"DayAheadPriceEur\":56.78}", "TimeUtc null")]
        [TestCase("{\"TimeUtc\":\"2023-04-17T20:00:00\",\"DayAheadPriceEur\":56.78}", "DayAheadPriceDkk absent")]
        [TestCase("{\"TimeUtc\":\"2023-04-17T20:00:00\",\"DayAheadPriceDkk\":null,\"DayAheadPriceEur\":56.78}", "DayAheadPriceDkk null")]
        [TestCase("{\"TimeUtc\":\"2023-04-17T20:00:00\",\"DayAheadPriceDkk\":12.34}", "DayAheadPriceEur absent")]
        [TestCase("{\"TimeUtc\":\"2023-04-17T20:00:00\",\"DayAheadPriceDkk\":12.34,\"DayAheadPriceEur\":null}", "DayAheadPriceEur null")]
        public void DeserializeInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<DayAheadPriceRecordDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void Deserialize()
        {
            // Arrange
            const string json = "{\"TimeUtc\":\"2023-04-17T20:00:00\",\"DayAheadPriceDkk\":12.34,\"DayAheadPriceEur\":56.78}";

            // Act
            var dto = JsonSerializer.Deserialize<DayAheadPriceRecordDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.TimeUtc, Is.EqualTo(new DateTime(2023, 4, 17, 20, 0, 0)));
            Assert.That(dto.DayAheadPriceDkk, Is.EqualTo(12.34));
            Assert.That(dto.DayAheadPriceEur, Is.EqualTo(56.78));
        }

    }
}

