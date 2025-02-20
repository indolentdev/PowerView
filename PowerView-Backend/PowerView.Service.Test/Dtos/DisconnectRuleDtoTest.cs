using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class DisconnectRuleDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":\"BAD\",\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DurationMinutes property invalid")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":\"BAD\",\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DisconnectToConnectValue property invalid")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":\"BAD\",\"Unit\":\"W\"}", "ConnectToDisconnectValue property invalid")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":2147483648,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DisconnectToConnectValue too high")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":2147483648,\"Unit\":\"W\"}", "ConnectToDisconnectValue too low")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"BAD\"}", "Unit bad")]
        public void DeserializeDisconnectRuleDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<DisconnectRuleDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "NameLabel absent")]
        [TestCase("{\"NameLabel\":null,\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "NameLabel null")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "NameObisCode absent")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"BAD\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "NameObisCode null")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "EvaluationLabel absent")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":null,\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "EvaluationLabel null")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "EvaluationObisCode absent")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":null,\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "EvaluationObisCode null")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DurationMinutes absent")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":null,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DurationMinutes null")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DisconnectToConnectValue absent")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":null,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DisconnectToConnectValue null")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"Unit\":\"W\"}", "ConnectToDisconnectValue absent")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":null,\"Unit\":\"W\"}", "ConnectToDisconnectValue null")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300}", "Unit absent")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":null}", "Unit null")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"A.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "NameObisCode invalid")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"A.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "EvaluationObisCode invalid")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":14,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DurationMinutes too low")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":3601,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DurationMinutes too high")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":0,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}", "DisconnectToConnectValue too low")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":0,\"Unit\":\"W\"}", "ConnectToDisconnectValue too low")]
        [TestCase("{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":1501,\"Unit\":\"W\"}", "ConnectToDisconnectValue greater than DisconnectToConnectValue")]
        public void DeserializeDisconnectRuleDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<DisconnectRuleDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeDisconnectRuleDto()
        {
            // Arrange
            const string json = "{\"NameLabel\":\"lbl1\",\"NameObisCode\":\"1.2.3.4.5.6\",\"EvaluationLabel\":\"lbl2\",\"EvaluationObisCode\":\"6.5.4.3.2.1\",\"DurationMinutes\":30,\"DisconnectToConnectValue\":1500,\"ConnectToDisconnectValue\":300,\"Unit\":\"W\"}";

            // Act
            var dto = JsonSerializer.Deserialize<DisconnectRuleDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.NameLabel, Is.EqualTo("lbl1"));
            Assert.That(dto.NameObisCode, Is.EqualTo("1.2.3.4.5.6"));
            Assert.That(dto.EvaluationLabel, Is.EqualTo("lbl2"));
            Assert.That(dto.EvaluationObisCode, Is.EqualTo("6.5.4.3.2.1"));
            Assert.That(dto.DurationMinutes, Is.EqualTo(30));
            Assert.That(dto.DisconnectToConnectValue, Is.EqualTo(1500));
            Assert.That(dto.ConnectToDisconnectValue, Is.EqualTo(300));
            Assert.That(dto.Unit, Is.EqualTo(DisconnectRuleUnit.W));
        }

    }
}

