using System;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class UnitValueTest
    {
        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange

            // Act
            var target = new UnitValue(10000, Unit.WattHour);

            // Assert
            Assert.That(target.Value, Is.EqualTo(10000));
            Assert.That(target.Unit, Is.EqualTo(Unit.WattHour));
        }

        [Test]
        public void ConstructorWithScaleAndProperties()
        {
            // Arrange

            // Act
            var target = new UnitValue(10, 3, Unit.WattHour);

            // Assert
            Assert.That(target.Value, Is.EqualTo(10000));
            Assert.That(target.Unit, Is.EqualTo(Unit.WattHour));
        }

        [Test]
        public void ToStringTest()
        {
            // Arrange

            // Act
            var target = new UnitValue(100, Unit.Watt);

            // Assert
            Assert.That(target.ToString(), Is.EqualTo("[value=100, unit=Watt]"));
        }

        [Test]
        public void EqualsAndHashCode()
        {
            // Arrange
            var t1 = new UnitValue(1, Unit.Watt);
            var t2 = new UnitValue(1, Unit.Watt);
            var t3 = new UnitValue(2, Unit.Watt);
            var t4 = new UnitValue(1, Unit.WattHour);

            // Act & Assert
            Assert.That(t1, Is.EqualTo(t2));
            Assert.That(t1, Is.EqualTo((object)t2));
            Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t3));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t4));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
        }

        [Test]
        public void Equeality()
        {
            // Arrange
            var t1 = new UnitValue(1, Unit.Watt);
            var t2 = new UnitValue(1, Unit.Watt);
            var t3 = new UnitValue(2, Unit.Watt);
            var t4 = new UnitValue(1, Unit.WattHour);

            // Act & Assert
            Assert.That(t1 == t2, Is.True);
            Assert.That(t1 == t3, Is.False);
            Assert.That(t1 == t4, Is.False);
        }

        [Test]
        public void OperatorAdd()
        {
            // Arrange
            var a = new UnitValue(23, Unit.Joule);
            var b = new UnitValue(12, Unit.Joule);

            // Act
            var target = a + b;

            // Assert
            Assert.That(target.Value, Is.EqualTo(a.Value + b.Value));
            Assert.That(target.Unit, Is.EqualTo(a.Unit));
        }

        [Test]
        public void OperatorAddThrows()
        {
            // Arrange
            var a = new UnitValue(23, Unit.Joule);
            var b = new UnitValue(12, Unit.CubicMetre);

            // Act & Assert
            Assert.That(() => a + b, Throws.TypeOf<DataMisalignedException>());
        }

        [Test]
        public void OperatorSubstract()
        {
            // Arrange
            var a = new UnitValue(23, Unit.Joule);
            var b = new UnitValue(12, Unit.Joule);

            // Act
            var target = a - b;

            // Assert
            Assert.That(target.Value, Is.EqualTo(a.Value - b.Value));
            Assert.That(target.Unit, Is.EqualTo(a.Unit));
        }

        [Test]
        public void OperatorSubstractThrows()
        {
            // Arrange
            var a = new UnitValue(23, Unit.Joule);
            var b = new UnitValue(12, Unit.CubicMetre);

            // Act & Assert
            Assert.That(() => a - b, Throws.TypeOf<DataMisalignedException>());
        }

    }
}

