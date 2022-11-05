using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class CrudeDataValueTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            DateTime dt = DateTime.UtcNow;
            ObisCode oc = ObisCode.ElectrActiveEnergyA14;
            const int val = 10;
            const short scale = 2;
            const Unit unit = Unit.CubicMetre;
            const string deviceId = "123";

            // Act & Assert
            Assert.That(() => new CrudeDataValue(DateTime.Now, oc, val,  scale, unit, deviceId), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new CrudeDataValue(dt, oc, val, scale, unit, string.Empty), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new CrudeDataValue(dt, oc, val, scale, unit, null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            DateTime dt = DateTime.UtcNow;
            ObisCode oc = ObisCode.ElectrActiveEnergyA14;
            const int val = 10;
            const short scale = 2;
            const Unit unit = Unit.CubicMetre;
            const string deviceId = "123";

            // Act
            var target = new CrudeDataValue(dt, oc, val, scale, unit, deviceId);

            // Assert
            Assert.That(target.DateTime, Is.EqualTo(dt));
            Assert.That(target.ObisCode, Is.EqualTo(oc));
            Assert.That(target.Value, Is.EqualTo(val));
            Assert.That(target.Scale, Is.EqualTo(scale));
            Assert.That(target.Unit, Is.EqualTo(unit));
            Assert.That(target.DeviceId, Is.EqualTo(deviceId));
        }

    }
}
