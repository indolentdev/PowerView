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
    public class CostBreakdownEntrytoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"FromDate\":\"BAD\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "FromDate value invalid")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"BAD\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "ToDate value invalid")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":\"BAD\",\"EndTime\":21,\"Amount\":1.234567}", "StartTime value invalid")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":\"BAD\",\"Amount\":1.234567}", "EndTime value invalid")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":\"BAD\"}", "Amount value invalid")]
        public void DeserializeCostBreakdownEntryDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<CostBreakdownEntryDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "FromDate absent")]
        [TestCase("{\"FromDate\":null,\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "FromDate null")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "FromDate not UTC")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "ToDate absent")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":null,\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "ToDate null")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "ToDate not UTC")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "Name absent")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":null,\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "Name null")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "Name min length")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"12345678901234567890123456\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}", "Name max length")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"EndTime\":21,\"Amount\":1.234567}", "StartTime absent")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":null,\"EndTime\":21,\"Amount\":1.234567}", "StartTime null")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":-1,\"EndTime\":21,\"Amount\":1.234567}", "StartTime min value")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":23,\"EndTime\":21,\"Amount\":1.234567}", "StartTime max value")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"Amount\":1.234567}", "EndTime absent")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":null,\"Amount\":1.234567}", "EndTime null")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":0,\"Amount\":1.234567}", "EndTime min value")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":24,\"Amount\":1.234567}", "EndTime max value")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":10,\"EndTime\":10,\"Amount\":1.234567}", "EndTime equal to StartTime")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":10,\"EndTime\":9,\"Amount\":1.234567}", "EndTime less than StartTime")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21}", "Amount absent")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":null}", "Amount null")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":-1}", "Amount min value")]
        [TestCase("{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1001}", "Amount max value")]
        public void DeserializeCostBreakdownEntryDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<CostBreakdownEntryDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeCostBreakdownEntryDto()
        {
            // Arrange
            var json = "{\"FromDate\":\"2023-10-31T23:00:00Z\",\"ToDate\":\"2023-11-30T23:00:00Z\",\"Name\":\"TheName\",\"StartTime\":2,\"EndTime\":21,\"Amount\":1.234567}";

            // Act
            var dto = JsonSerializer.Deserialize<CostBreakdownEntryDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.FromDate, Is.EqualTo(DateTimeOffset.Parse("2023-10-31T23:00:00Z", CultureInfo.InvariantCulture).UtcDateTime));
            Assert.That(dto.FromDate.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(dto.ToDate, Is.EqualTo(DateTimeOffset.Parse("2023-11-30T23:00:00Z", CultureInfo.InvariantCulture).UtcDateTime));
            Assert.That(dto.ToDate.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(dto.Name, Is.EqualTo("TheName"));
            Assert.That(dto.StartTime, Is.EqualTo(2));
            Assert.That(dto.EndTime, Is.EqualTo(21));
            Assert.That(dto.Amount, Is.EqualTo(1.234567));
        }

    }
}
