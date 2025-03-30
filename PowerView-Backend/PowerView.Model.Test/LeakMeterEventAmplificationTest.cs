using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class LeakMeterEventAmplificationTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            var utcNow = DateTime.UtcNow;
            var unitValue = new UnitValue(1, Unit.Watt);

            // Act & Assert
            Assert.That(() => new LeakMeterEventAmplification(DateTime.Now, utcNow, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new LeakMeterEventAmplification(utcNow, DateTime.Now, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            var start = DateTime.UtcNow - TimeSpan.FromDays(1);
            var end = DateTime.UtcNow;
            var unitValue = new UnitValue(1, Unit.Watt);

            // Act
            var target = new LeakMeterEventAmplification(start, end, unitValue);

            // Assert
            Assert.That(target.Start, Is.EqualTo(start));
            Assert.That(target.End, Is.EqualTo(end));
            Assert.That(target.UnitValue, Is.EqualTo(unitValue));
        }

        [Test]
        public void ConstructorDeserializationAndProperties()
        {
            // Arrange
            var start = DateTime.UtcNow - TimeSpan.FromDays(1);
            var end = DateTime.UtcNow;
            var unitValue = new UnitValue(1, Unit.Watt);
            var json = JsonSerializer.Serialize(new LeakMeterEventAmplification(start, end, unitValue));
            var entityDeserializer = new EntityDeserializer(JsonNode.Parse(json));

            // Act
            var target = new LeakMeterEventAmplification(entityDeserializer);

            // Assert
            Assert.That(target.Start, Is.EqualTo(start));
            Assert.That(target.End, Is.EqualTo(end));
            Assert.That(target.UnitValue, Is.EqualTo(unitValue));
        }

    }
}

