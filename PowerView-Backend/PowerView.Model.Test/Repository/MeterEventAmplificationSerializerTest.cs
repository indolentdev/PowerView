using System;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
    [TestFixture]
    public class MeterEventAmplificationSerializerTest
    {
        [Test]
        public void SerializeThrows()
        {
            // Arrange

            // Act
            Assert.That(() => MeterEventAmplificationSerializer.Serialize(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void DeserializeThrows()
        {
            // Arrange

            // Act
            Assert.That(() => MeterEventAmplificationSerializer.Deserialize(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => MeterEventAmplificationSerializer.Deserialize(string.Empty), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => MeterEventAmplificationSerializer.Deserialize("BadEnvelope"), Throws.TypeOf<EntitySerializationException>());
            Assert.That(() => MeterEventAmplificationSerializer.Deserialize("{ \"TypeName\":\"BogusTypeName\" }"), Throws.TypeOf<EntitySerializationException>());
            Assert.That(() => MeterEventAmplificationSerializer.Deserialize("{ \"TypeName\":\"TestMeterEventAmplification\", \"Content\":\"\" }"), Throws.TypeOf<EntitySerializationException>());
            Assert.That(() => MeterEventAmplificationSerializer.Deserialize("{ \"TypeName\":\"BadTestMeterEventAmplification\", \"Content\":\"{}\" }"), Throws.TypeOf<EntitySerializationException>());
        }

        [Test]
        public void SerializeDeserialize()
        {
            // Arrange
            var meterEventAmplification = new TestMeterEventAmplification { DoubleValue = 1.2, Nested = new Nesting { StringValue = "34" } };

            // Act
            var s = MeterEventAmplificationSerializer.Serialize(meterEventAmplification);
            var meterEventAmplification2 = (TestMeterEventAmplification)MeterEventAmplificationSerializer.Deserialize(s);

            // Assert
            Assert.That(meterEventAmplification2.DoubleValue, Is.EqualTo(meterEventAmplification.DoubleValue));
            Assert.That(meterEventAmplification2.Nested.StringValue, Is.EqualTo(meterEventAmplification.Nested.StringValue));
        }

        private class TestMeterEventAmplification : IMeterEventAmplification
        {
            public TestMeterEventAmplification()
            {
            }

            internal TestMeterEventAmplification(IEntityDeserializer serializer)
            {
                DoubleValue = serializer.GetValue<double>("DoubleValue");
                Nested = new Nesting();
                Nested.StringValue = serializer.GetValue<string>("Nested", "StringValue");
            }

            public string GetMeterEventType()
            {
                return "Test";
            }

            public double DoubleValue { get; set; }
            public Nesting Nested { get; set; }
        }

        private class Nesting
        {
            public string StringValue { get; set; }
        }

        private class BadTestMeterEventAmplification : IMeterEventAmplification
        {
            // Missing accommodating serialization constructor

            public string GetMeterEventType()
            {
                return "BadTest";
            }
        }

    }
}

