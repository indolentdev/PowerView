using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NUnit.Framework;
using PowerView.Service.Dtos;
using PowerView.Model;

namespace PowerView.Service.Test.Dto
{
    [TestFixture]
    public class ProfileGraphDtoTest
    {
        [Test]
        [TestCase("Bad JSON", "Json invalid")]
        public void DeserializeProfileGraphDtoInvalidThrowsJsonException(string json, string message)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => JsonSerializer.Deserialize<ProfileGraphDto>(json), Throws.TypeOf<JsonException>(), message);
        }

        [Test]
        [TestCase("{\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Period absent")]
        [TestCase("{\"Period\":null,\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Period null")]
        [TestCase("{\"Period\":\"\",\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Period invalid")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Title absent")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":null,\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Title null")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":\"\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Title min length")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Interval absent")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":null,\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Interval null")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":\"\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Interval null")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\"}", "Series absent")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":null}", "Series null")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":[]}", "Series min length")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"},{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Series duplicate")]
        public void DeserializeProfileGraphDtoInvalidThrowsValidationException(string json, string message)
        {
            // Arrange
            var dto = JsonSerializer.Deserialize<ProfileGraphDto>(json);

            // Act & Assert
            Assert.That(() => Validator.ValidateObject(dto, new ValidationContext(dto), true), Throws.TypeOf<ValidationException>(), message);
        }

        [Test]
        [TestCase("{\"Period\":\"day\",\"Page\":\"Pg\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "Pg")]
        [TestCase("{\"Period\":\"day\",\"Page\":\"\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", "")]
        [TestCase("{\"Period\":\"day\",\"Page\":null,\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", null)]
        [TestCase("{\"Period\":\"day\",\"Title\":\"TheTitle\",\"Interval\":\"5-minutes\",\"Series\":[{\"Label\":\"Lbl\",\"ObisCode\":\"1.2.3.4.5.6\"}]}", null)]
        public void DeserializeProfileGraphDto(string json, string page)
        {
            // Arrange

            // Act
            var dto = JsonSerializer.Deserialize<ProfileGraphDto>(json);
            Validator.ValidateObject(dto, new ValidationContext(dto), true);

            // Assert
            Assert.That(dto.Period, Is.EqualTo("day"));
            Assert.That(dto.Page, Is.EqualTo(page));
            Assert.That(dto.Title, Is.EqualTo("TheTitle"));
            Assert.That(dto.Interval, Is.EqualTo("5-minutes"));
            Assert.That(dto.Series, Is.Not.Empty);
        }

    }
}

