using System;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Service.Mappers;

namespace PowerView.Service.Test.Mappers
{
    [TestFixture]
    public class UnitMapperTest
    {
        [Test]
        [TestCase((Unit)123)]
        public void MapUnit(Unit unit)
        {
            // Arrange

            // Act & Assert
            Assert.That(UnitMapper.Map(unit), Is.EqualTo("Unknown"));
        }

        [Test]
        [TestCaseSource(nameof(MapSpec))]
        public void MapUnit(string unitString, Unit unit)
        {
            // Arrange

            // Act & Assert
            Assert.That(UnitMapper.Map(unit), Is.EqualTo(unitString));
        }

        [Test]
        [TestCase(typeof(ArgumentNullException), null)]
        [TestCase(typeof(ArgumentOutOfRangeException), "whatnot")]
        public void MapStringThrows(Type expectedException, string unitString)
        {
            // Arrange

            // Act & Assert
            Assert.That(() => UnitMapper.Map(unitString), Throws.TypeOf(expectedException));
        }

        [Test]
        [TestCaseSource(nameof(MapSpec))]
        public void MapString(string unitString, Unit unit)
        {
            // Arrange

            // Act & Assert
            Assert.That(UnitMapper.Map(unitString), Is.EqualTo(unit));
        }

        private static object[] MapSpec = new[] {
          new object[] { "W", Unit.Watt },
          new object[] { "Wh", Unit.WattHour },
          new object[] { "m3", Unit.CubicMetre },
          new object[] { "m3/h", Unit.CubicMetrePrHour },
          new object[] { "C", Unit.DegreeCelsius },
          new object[] { "J", Unit.Joule },
          new object[] { "J/h", Unit.JoulePrHour },
          new object[] { "%", Unit.Percentage },
          new object[] { "EUR", Unit.Eur },
          new object[] { "DKK", Unit.Dkk },

          new object[] { "", Unit.NoUnit },
        };


    }
}

