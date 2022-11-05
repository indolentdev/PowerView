using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class PostCrudeValueDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02Q20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Timestamp property invalid")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":BAD,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Value property invalid")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":BAD,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Scale property invalid")]
        public void DeserializeDisconnectRuleDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<PostCrudeValueDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Label property absent")]
        [TestCase("{\"Label\":null,\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Label property null")]
        [TestCase("{\"Label\":\"lbl\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Timestamp property absent")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":null,\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Timestamp property null")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "ObisCode property absent")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":null,\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "ObisCode property null")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"A.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "ObisCode property bad")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Value property absent")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":null,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Value property null")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Scale property absent")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":null,\"Unit\":\"W\",\"DeviceId\":\"DID\"}", "Scale property null")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"DeviceId\":\"DID\"}", "Unit property absent")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":null,\"DeviceId\":\"DID\"}", "Unit property null")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\"}", "DeviceId property absent")]
        [TestCase("{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":null}", "DeviceId property null")]
        public void DeserializeDisconnectRuleDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<PostCrudeValueDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializePostCrudeValueDto()
        {
            // Arrange
            const string json = "{\"Label\":\"lbl\",\"Timestamp\":\"2022-11-02T20:34:11Z\",\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":1500,\"Scale\":-1,\"Unit\":\"W\",\"DeviceId\":\"DID\"}";

            // Act
            var dto = JsonSerializer.Deserialize<PostCrudeValueDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Label, Is.EqualTo("lbl"));
            Assert.That(dto.Timestamp, Is.EqualTo(new DateTime(2022, 11, 2, 20, 34, 11, DateTimeKind.Utc)));
            Assert.That(dto.ObisCode, Is.EqualTo("1.2.3.4.5.6"));
            Assert.That(dto.Value, Is.EqualTo(1500));
            Assert.That(dto.Scale, Is.EqualTo(-1));
            Assert.That(dto.Unit, Is.EqualTo("W"));
            Assert.That(dto.DeviceId, Is.EqualTo("DID"));
        }

    }
}

