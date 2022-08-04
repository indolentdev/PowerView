using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class MqttConfigDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"PublishEnabled\":maybe,\"Server\":\"Svr\",\"Port\":1234,\"ClientId\":\"ClId\"}", "PublishEnabled invalid")]
        [TestCase("{\"PublishEnabled\":false,\"Server\":\"Svr\",\"Port\":\"1234\",\"ClientId\":\"ClId\"}", "Port invalid")]
        public void DeserializeMqttConfigDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<MqttConfigDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"Server\":\"Svr\",\"Port\":1234,\"ClientId\":\"ClId\"}", "PublishEnabled absent")]
        [TestCase("{\"PublishEnabled\":null,\"Server\":\"Svr\",\"Port\":1234,\"ClientId\":\"ClId\"}", "PublishEnabled null")]
        [TestCase("{\"PublishEnabled\":true,\"Port\":1234,\"ClientId\":\"ClId\"}", "Server absent")]
        [TestCase("{\"PublishEnabled\":true,\"Server\":null,\"Port\":1234,\"ClientId\":\"ClId\"}", "Server null")]
        [TestCase("{\"PublishEnabled\":true,\"Server\":\"Svr\",\"ClientId\":\"ClId\"}", "Port absent")]
        [TestCase("{\"PublishEnabled\":true,\"Server\":\"Svr\",\"Port\":null,\"ClientId\":\"ClId\"}", "Port null")]
        [TestCase("{\"PublishEnabled\":true,\"Server\":\"Svr\",\"Port\":1234}", "ClientId absent")]
        [TestCase("{\"PublishEnabled\":true,\"Server\":\"Svr\",\"Port\":1234,\"ClientId\":null}", "ClientId null")]
        public void DeserializeMqttConfigDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<MqttConfigDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeMqttConfigDto()
        {
            // Arrange
            const string json = "{\"PublishEnabled\":true,\"Server\":\"Svr\",\"Port\":1234,\"ClientId\":\"ClId\"}";

            // Act
            var dto = JsonSerializer.Deserialize<MqttConfigDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.PublishEnabled, Is.True);
            Assert.That(dto.Server, Is.EqualTo("Svr"));
            Assert.That(dto.Port, Is.EqualTo(1234));
            Assert.That(dto.ClientId, Is.EqualTo("ClId"));
        }

    }
}

