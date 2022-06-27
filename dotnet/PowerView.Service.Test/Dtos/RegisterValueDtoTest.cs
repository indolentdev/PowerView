using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class RegisterValueDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":\"BAD\",\"Scale\":1,\"Unit\":\"watt\"}", "Value property invalid")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":\"BAD\",\"Unit\":\"watt\"}", "Scale property invalid")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"BAD\"}", "Unit property invalid")]
        public void DeserializeLiveReadingDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<RegisterValueDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}", "ObisCode property absent")]
        [TestCase("{\"ObisCode\":null,\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}", "ObisCode property null")]
        [TestCase("{\"ObisCode\":\"BAD\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}", "ObisCode property invalid")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}", "ObisCode property invalid")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6.\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}", "ObisCode property invalid")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Scale\":1,\"Unit\":\"watt\"}", "Value property absent")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":null,\"Scale\":1,\"Unit\":\"watt\"}", "Value property null")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Unit\":\"watt\"}", "Scale property absent")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":null,\"Unit\":\"watt\"}", "Scale property null")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1}", "Unit property absent")]
        [TestCase("{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":null}", "Unit property null")]
        public void DeserializeLiveReadingDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<RegisterValueDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeRegisterValueDto()
        {
            // Arrange
            const string json = "{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}";

            // Act
            var dto = JsonSerializer.Deserialize<RegisterValueDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.ObisCode, Is.EqualTo("1.2.3.4.5.6"));
            Assert.That(dto.Value, Is.EqualTo(2));
            Assert.That(dto.Scale, Is.EqualTo(1));
            Assert.That(dto.Unit, Is.EqualTo(Unit.Watt));
        }



    }
}

