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
    public class ImportCreateDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":\"DK1\",\"Currency\":\"USD\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Currency invalid")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":\"DK1\",\"Currency\":\"EUR\",\"FromTimestamp\":\"BAD\"}", "FromTimestamp value invalid")]
        public void DeserializeImportCreateDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<ImportCreateDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"Channel\":\"DK1\",\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Label absent")]
        [TestCase("{\"Label\":null,\"Channel\":\"DK1\",\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Label null")]
        [TestCase("{\"Label\":\"\",\"Channel\":\"DK1\",\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Label min length")]
        [TestCase("{\"Label\":\"12345678901234567890123456\",\"Channel\":\"DK1\",\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Label max length")]
        [TestCase("{\"Label\":\"TheLabel\",\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Channel absent")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":null,\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Channel null")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":\"DK\",\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Channel min length")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":\"DK11\",\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Channel max length")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":\"DK1\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Currency absent")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":\"DK1\",\"Currency\":null,\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}", "Currency null")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":\"DK1\",\"Currency\":\"EUR\"}", "FromTimestamp absent")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":\"DK1\",\"Currency\":\"EUR\",\"FromTimestamp\":null}", "FromTimestamp null")]
        [TestCase("{\"Label\":\"TheLabel\",\"Channel\":\"DK1\",\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00\"}", "FromTimestamp not UTC")]
        public void DeserializeImportCreateDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<ImportCreateDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeImportCreateDto()
        {
            // Arrange
            var json = "{\"Label\":\"TheLabel\",\"Channel\":\"DK1\",\"Currency\":\"EUR\",\"FromTimestamp\":\"2023-10-31T23:00:00Z\"}";

            // Act
            var dto = JsonSerializer.Deserialize<ImportCreateDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Label, Is.EqualTo("TheLabel"));
            Assert.That(dto.Channel, Is.EqualTo("DK1"));
            Assert.That(dto.Currency, Is.EqualTo(Unit.Eur));
            Assert.That(dto.FromTimestamp, Is.EqualTo(DateTimeOffset.Parse("2023-10-31T23:00:00Z", CultureInfo.InvariantCulture).UtcDateTime));
            Assert.That(dto.FromTimestamp.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

    }
}
