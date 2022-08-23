using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class LiveReadingDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"Label\":\"lbl\",\"DeviceId\":\"sn\",\"Timestamp\":\"2020-03-07Q21:44:22Z\",\"RegisterValues\":[]}", "Timestamp property invalid format")]
        public void DeserializeLiveReadingDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<LiveReadingDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"DeviceId\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[]}", "Label property absent")]
        [TestCase("{\"Label\":null,\"DeviceId\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[]}", "Label property null")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[]}", "DeviceId property absent")]
        [TestCase("{\"Label\":\"lbl\",\"DeviceId\":null,\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[]}", "DeviceId property null")]
        [TestCase("{\"Label\":\"lbl\",\"DeviceId\":\"sn\",\"RegisterValues\":[]}", "Timestamp property absent")]
        [TestCase("{\"Label\":\"lbl\",\"DeviceId\":\"sn\",\"Timestamp\":null,\"RegisterValues\":[]}", "Timestamp property null")]
        [TestCase("{\"Label\":\"lbl\",\"DeviceId\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22\",\"RegisterValues\":[]}", "Timestamp property not UTC")]
        [TestCase("{\"Label\":\"lbl\",\"DeviceId\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\"}", "RegisterValues property absent")]
        [TestCase("{\"Label\":\"lbl\",\"DeviceId\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":null}", "RegisterValues property absent")]
        public void DeserializeLiveReadingDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<LiveReadingDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeLiveReadingDto()
        {
            // Arrange
            const string json = "{\"Label\":\"lbl\",\"DeviceId\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[]}";

            // Act
            var dto = JsonSerializer.Deserialize<LiveReadingDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Label, Is.EqualTo("lbl"));
            Assert.That(dto.DeviceId, Is.EqualTo("sn"));
            Assert.That(dto.SerialNumber, Is.Null);
            Assert.That(dto.Timestamp, Is.EqualTo(new DateTime(2020, 3, 7, 21, 44, 22, DateTimeKind.Utc)));
            Assert.That(dto.RegisterValues, Is.Empty);
        }

        [Test]
        public void DeserializeLiveReadingDtoWithSerialNumber()
        {
            // Arrange
            const string json = "{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[]}";

            // Act
            var dto = JsonSerializer.Deserialize<LiveReadingDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Label, Is.EqualTo("lbl"));
            Assert.That(dto.DeviceId, Is.Null);
            Assert.That(dto.SerialNumber, Is.EqualTo("sn"));
            Assert.That(dto.Timestamp, Is.EqualTo(new DateTime(2020, 3, 7, 21, 44, 22, DateTimeKind.Utc)));
            Assert.That(dto.RegisterValues, Is.Empty);
        }

    }
}

