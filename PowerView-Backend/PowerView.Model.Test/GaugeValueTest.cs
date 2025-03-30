using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class GaugeValueTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            const string label = "lbl";
            const string deviceId = "123";
            DateTime dt = DateTime.UtcNow;
            ObisCode oc = ObisCode.ElectrActiveEnergyA14;
            var unitValue = new UnitValue(1, Unit.CubicMetre);

            // Act & Assert
            Assert.That(() => new GaugeValue(string.Empty, deviceId, dt, oc, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new GaugeValue(null, deviceId, dt, oc, unitValue), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new GaugeValue(label, string.Empty, dt, oc, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new GaugeValue(label, null, dt, oc, unitValue), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new GaugeValue(label, deviceId, DateTime.Now, oc, unitValue), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            const string label = "lbl";
            const string deviceId = "123";
            DateTime dt = DateTime.UtcNow;
            ObisCode oc = ObisCode.ElectrActiveEnergyA14;
            var unitValue = new UnitValue(1, Unit.CubicMetre);

            // Act
            var target = new GaugeValue(label, deviceId, dt, oc, unitValue);

            // Assert
            Assert.That(target.Label, Is.EqualTo(label));
            Assert.That(target.DeviceId, Is.EqualTo(deviceId));
            Assert.That(target.DateTime, Is.EqualTo(dt));
            Assert.That(target.ObisCode, Is.EqualTo(oc));
            Assert.That(target.UnitValue, Is.EqualTo(unitValue));
        }

    }
}
