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
    public class GeneratorSeriesDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        public void DeserializeGeneratorSeriesDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<GeneratorSeriesDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":\"1.68.25.67.0.255\",\"CostBreakdownTitle\":\"CBTitle\"}", "NameLabel absent")]
        [TestCase("{\"NameLabel\":null,\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":\"1.68.25.67.0.255\",\"CostBreakdownTitle\":\"CBTitle\"}", "NameLabel null")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":\"1.68.25.67.0.255\",\"CostBreakdownTitle\":\"CBTitle\"}", "NameObisCode absent")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"NameObisCode\":null,\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":\"1.68.25.67.0.255\",\"CostBreakdownTitle\":\"CBTitle\"}", "NameObisCode null")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"NameObisCode\":\"BAD\",\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":\"1.68.25.67.0.255\",\"CostBreakdownTitle\":\"CBTitle\"}", "NameObisCode bad")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseObisCode\":\"1.68.25.67.0.255\",\"CostBreakdownTitle\":\"CBTitle\"}", "BaseLabel absent")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseLabel\":null,\"BaseObisCode\":\"1.68.25.67.0.255\",\"CostBreakdownTitle\":\"CBTitle\"}", "BaseLabel null")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseLabel\":\"BaseLabel\",\"CostBreakdownTitle\":\"CBTitle\"}", "BaseObisCode absent")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":null,\"CostBreakdownTitle\":\"CBTitle\"}", "BaseObisCode null")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":\"BAD\",\"CostBreakdownTitle\":\"CBTitle\"}", "BaseObisCode bad")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":\"1.68.25.67.0.255\"}", "CostBreakdownTitle absent")]
        [TestCase("{\"NameLabel\":\"TheLabel\",\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":\"1.68.25.67.0.255\",\"CostBreakdownTitle\":null}", "CostBreakdownTitle null")]
        public void DeserializeGeneratorSeriesDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<GeneratorSeriesDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        public void DeserializeGeneratorSeriesDto()
        {
            // Arrange
            var json = "{\"NameLabel\":\"TheLabel\",\"NameObisCode\":\"1.69.25.67.0.255\",\"BaseLabel\":\"BaseLabel\",\"BaseObisCode\":\"1.68.25.67.0.255\",\"CostBreakdownTitle\":\"CBTitle\"}";

            // Act
            var dto = JsonSerializer.Deserialize<GeneratorSeriesDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.NameLabel, Is.EqualTo("TheLabel"));
            Assert.That(dto.NameObisCode, Is.EqualTo("1.69.25.67.0.255"));
            Assert.That(dto.BaseLabel, Is.EqualTo("BaseLabel"));
            Assert.That(dto.BaseObisCode, Is.EqualTo("1.68.25.67.0.255"));
            Assert.That(dto.CostBreakdownTitle, Is.EqualTo("CBTitle"));
        }

    }
}
