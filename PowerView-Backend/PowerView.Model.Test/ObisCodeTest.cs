using System;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class ObisCodeTest
    {
        [Test]
        public void ConstructorAndEnumerable()
        {
            // Arrange
            var bytes = new byte[] { 1, 2, 3, 4, 5, 6 };

            // Act
            var target = new ObisCode(bytes);

            // Assert
            Assert.That(target, Is.EqualTo(bytes));
        }

        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new ObisCode(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new ObisCode(new byte[] { 1, 2, 3, 4, 5 }), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase("0.0.0.0.0.0", true)]
        [TestCase("1.2.3.4.5.6", true)]
        [TestCase("255.255.255.255.255.255", true)]
        [TestCase("1.2.3.4.5.6.", false)]
        [TestCase("1.2.3.4.5", false)]
        [TestCase("1.2.3.4..5", false)]
        [TestCase("a.2.3.4.5.6", false)]
        [TestCase("256.255.255.255.255.255", false)]
        public void TryParse(string s, bool success)
        {
            // Arrange

            // Act & Assert
            Assert.That(ObisCode.TryParse(s, out var obisCode), Is.EqualTo(success));
            Assert.That(obisCode.ToString(), Is.EqualTo(success ? s : new ObisCode().ToString()));
        }

        [Test]
        public void ConstructorImplicitStringToObisCode()
        {
            // Arrange
            const string s = "1.2.3.4.5.6";

            // Act
            ObisCode target = s;

            // Assert
            Assert.That(target, Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6 }));
        }

        [Test]
        public void ConstructorImplicitObisCodeToLong()
        {
            // Arrange
            var target = new ObisCode(new byte[] { 1, 2, 3, 4, 5, 6 });

            // Act
            long l = target;

            // Assert
            Assert.That(l, Is.EqualTo(1108152157446));
        }

        [Test]
        public void ConstructorImplicitLongToObisCode()
        {
            // Arrange
            const long l = 1108152157446L;

            // Act
            ObisCode target = l;

            // Assert
            Assert.That(target, Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6 }));
        }

        [Test]
        public void ToStringTest()
        {
            // Arrange
            var bytes = new byte[] { 1, 2, 3, 4, 5, 6 };

            // Act
            var target = new ObisCode(bytes);

            // Assert
            Assert.That(target.ToString(), Is.EqualTo("1.2.3.4.5.6"));
        }

        [Test]
        public void EqualsAndHashCode()
        {
            // Arrange
            var t1 = new ObisCode(new byte[] { 1, 2, 3, 4, 5, 6 });
            var t2 = new ObisCode(new byte[] { 1, 2, 3, 4, 5, 6 });
            var t3 = new ObisCode(new byte[] { 1, 1, 1, 2, 2, 2 });

            // Act & Assert
            Assert.That(t1, Is.EqualTo(t2));
            Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t3));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
        }

        [Test]
        public void Equeality()
        {
            // Arrange
            var t1 = new ObisCode(new byte[] { 1, 2, 3, 4, 5, 6 });
            var t2 = new ObisCode(new byte[] { 1, 2, 3, 4, 5, 6 });
            var t3 = new ObisCode(new byte[] { 1, 1, 1, 2, 2, 2 });

            // Act & Assert
            Assert.That(t1 == t2, Is.True);
            Assert.That(t1 == t3, Is.False);
        }

        [Test]
        public void StaticObisCodesElectricity()
        {
            // Arrange

            // Act & Assert
            Assert.That(ObisCode.ElectrActiveEnergyA14, Is.EqualTo(new byte[] { 1, 0, 1, 8, 0, 255 }));
            Assert.That(ObisCode.ElectrActiveEnergyA14Delta, Is.EqualTo(new byte[] { 1, 65, 1, 8, 0, 255 }));
            Assert.That(ObisCode.ElectrActiveEnergyA14Period, Is.EqualTo(new byte[] { 1, 66, 1, 8, 0, 255 }));
            Assert.That(ObisCode.ElectrActiveEnergyA14NetDelta, Is.EqualTo(new byte[] { 1, 65, 16, 8, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP14, Is.EqualTo(new byte[] { 1, 0, 1, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP14Average, Is.EqualTo(new byte[] { 1, 67, 1, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP14L1, Is.EqualTo(new byte[] { 1, 0, 21, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP14L2, Is.EqualTo(new byte[] { 1, 0, 41, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP14L3, Is.EqualTo(new byte[] { 1, 0, 61, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActiveEnergyA23, Is.EqualTo(new byte[] { 1, 0, 2, 8, 0, 255 }));
            Assert.That(ObisCode.ElectrActiveEnergyA23Delta, Is.EqualTo(new byte[] { 1, 65, 2, 8, 0, 255 }));
            Assert.That(ObisCode.ElectrActiveEnergyA23Period, Is.EqualTo(new byte[] { 1, 66, 2, 8, 0, 255 }));
            Assert.That(ObisCode.ElectrActiveEnergyA23NetDelta, Is.EqualTo(new byte[] { 1, 65, 26, 8, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP23, Is.EqualTo(new byte[] { 1, 0, 2, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP23Average, Is.EqualTo(new byte[] { 1, 67, 2, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP23L1, Is.EqualTo(new byte[] { 1, 0, 22, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP23L2, Is.EqualTo(new byte[] { 1, 0, 42, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActualPowerP23L3, Is.EqualTo(new byte[] { 1, 0, 62, 7, 0, 255 }));
            Assert.That(ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat, Is.EqualTo(new byte[] { 1, 68, 25, 67, 0, 255 }));
            Assert.That(ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat, Is.EqualTo(new byte[] { 1, 69, 25, 67, 0, 255 }));
        }

        [Test]
        public void StaticObisCodesColdWater()
        {
            // Arrange

            // Act & Assert
            Assert.That(ObisCode.ColdWaterVolume1, Is.EqualTo(new byte[] { 8, 0, 1, 0, 0, 255 }));
            Assert.That(ObisCode.ColdWaterVolume1Delta, Is.EqualTo(new byte[] { 8, 65, 1, 0, 0, 255 }));
            Assert.That(ObisCode.ColdWaterVolume1Period, Is.EqualTo(new byte[] { 8, 66, 1, 0, 0, 255 }));
            Assert.That(ObisCode.ColdWaterFlow1, Is.EqualTo(new byte[] { 8, 0, 2, 0, 0, 255 }));
            Assert.That(ObisCode.ColdWaterFlow1Average, Is.EqualTo(new byte[] { 8, 67, 2, 0, 0, 255 }));
        }

        [Test]
        public void StaticObisCodesHotWater()
        {
            // Arrange

            // Act & Assert
            Assert.That(ObisCode.HotWaterVolume1, Is.EqualTo(new byte[] { 9, 0, 1, 0, 0, 255 }));
            Assert.That(ObisCode.HotWaterVolume1Delta, Is.EqualTo(new byte[] { 9, 65, 1, 0, 0, 255 }));
            Assert.That(ObisCode.HotWaterVolume1Period, Is.EqualTo(new byte[] { 9, 66, 1, 0, 0, 255 }));
            Assert.That(ObisCode.HotWaterFlow1, Is.EqualTo(new byte[] { 9, 0, 2, 0, 0, 255 }));
            Assert.That(ObisCode.HotWaterFlow1Average, Is.EqualTo(new byte[] { 9, 67, 2, 0, 0, 255 }));
        }

        [Test]
        public void StaticObisCodesHeatEnergy()
        {
            // Arrange

            // Act & Assert
            Assert.That(ObisCode.HeatEnergyEnergy1, Is.EqualTo(new byte[] { 6, 0, 1, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyEnergy1Delta, Is.EqualTo(new byte[] { 6, 65, 1, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyEnergy1Period, Is.EqualTo(new byte[] { 6, 66, 1, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyVolume1, Is.EqualTo(new byte[] { 6, 0, 2, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyVolume1Delta, Is.EqualTo(new byte[] { 6, 65, 2, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyVolume1Period, Is.EqualTo(new byte[] { 6, 66, 2, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyPower1, Is.EqualTo(new byte[] { 6, 0, 8, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyPower1Average, Is.EqualTo(new byte[] { 6, 67, 8, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyFlow1, Is.EqualTo(new byte[] { 6, 0, 9, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyFlow1Average, Is.EqualTo(new byte[] { 6, 67, 9, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyFlowTemperature, Is.EqualTo(new byte[] { 6, 0, 10, 0, 0, 255 }));
            Assert.That(ObisCode.HeatEnergyReturnTemperature, Is.EqualTo(new byte[] { 6, 0, 11, 0, 0, 255 }));
        }

        [Test]
        public void StaticObisCodesRoomSensor()
        {
            // Arrange

            // Act & Assert
            Assert.That(ObisCode.RoomTemperature, Is.EqualTo(new byte[] { 15, 0, 223, 0, 0, 255 }));
            Assert.That(ObisCode.RoomRelativeHumidity, Is.EqualTo(new byte[] { 15, 0, 223, 0, 2, 255 }));
        }

        [Test]
        [TestCase("1.0.1.7.0.255")]
        [TestCase("1.67.1.7.0.255")]
        [TestCase("1.0.21.7.0.255")]
        [TestCase("1.0.41.7.0.255")]
        [TestCase("1.0.61.7.0.255")]
        [TestCase("1.0.1.8.0.255")]
        [TestCase("1.65.1.8.0.255")]
        [TestCase("1.66.1.8.0.255")]
        public void IsElectricityImport(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsElectricityImport;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("1.0.2.7.0.255")]
        [TestCase("1.67.2.7.0.255")]
        [TestCase("1.0.22.7.0.255")]
        [TestCase("1.0.42.7.0.255")]
        [TestCase("1.0.62.7.0.255")]
        [TestCase("1.0.2.8.0.255")]
        [TestCase("1.65.2.8.0.255")]
        [TestCase("1.66.2.8.0.255")]
        public void IsElectricityExport(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsElectricityExport;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("1.0.1.8.0.255")]
        [TestCase("1.0.2.8.0.255")]
        public void IsElectricityCumulative(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsElectricityCumulative;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("8.0.1.0.0.255")]
        [TestCase("8.65.1.0.0.255")]
        [TestCase("8.66.1.0.0.255")]
        [TestCase("8.0.2.0.0.255")]
        [TestCase("8.67.2.0.0.255")]
        [TestCase("9.0.1.0.0.255")]
        [TestCase("9.65.1.0.0.255")]
        [TestCase("9.66.1.0.0.255")]
        [TestCase("9.0.2.0.0.255")]
        [TestCase("9.67.2.0.0.255")]
        public void IsWaterImport(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsWaterImport;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("8.0.1.0.0.255")]
        [TestCase("9.0.1.0.0.255")]
        public void IsWaterCumulative(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsWaterCumulative;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("8.65.1.0.0.255")]
        [TestCase("8.66.1.0.0.255")]
        [TestCase("8.67.1.0.0.255")]
        [TestCase("9.65.1.0.0.255")]
        [TestCase("9.66.1.0.0.255")]
        [TestCase("9.67.1.0.0.255")]
        public void IsNotWaterCumulative(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsWaterCumulative;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [TestCase("6.0.1.0.0.255")]
        [TestCase("6.65.1.0.0.255")]
        [TestCase("6.66.1.0.0.255")]
        [TestCase("6.0.2.0.0.255")]
        [TestCase("6.65.2.0.0.255")]
        [TestCase("6.66.2.0.0.255")]
        [TestCase("6.0.8.0.0.255")]
        [TestCase("6.67.8.0.0.255")]
        [TestCase("6.0.9.0.0.255")]
        [TestCase("6.67.9.0.0.255")]
        [TestCase("6.0.10.0.0.255")]
        [TestCase("6.0.11.0.0.255")]
        public void IsEnergyImport(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsEnergyImport;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("15.0.223.0.0.255")]
        public void IsNotEnergyImport(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsEnergyImport;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [TestCase("6.0.1.0.0.255")]
        [TestCase("6.0.2.0.0.255")]
        public void IsEnergyCumulative(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsEnergyCumulative;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("1.0.1.8.0.255")]
        [TestCase("1.0.2.8.0.255")]
        [TestCase("8.0.1.0.0.255")]
        [TestCase("6.0.1.0.0.255")]
        [TestCase("6.0.2.0.0.255")]
        public void IsCumulative(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsCumulative;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("1.65.1.8.0.255")]
        [TestCase("1.65.16.8.0.255")]
        [TestCase("1.65.2.8.0.255")]
        [TestCase("1.65.26.8.0.255")]
        [TestCase("8.65.1.0.0.255")]
        [TestCase("6.65.1.0.0.255")]
        [TestCase("6.65.2.0.0.255")]
        public void IsDelta(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsDelta;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("1.66.1.8.0.255")]
        [TestCase("1.66.2.8.0.255")]
        [TestCase("8.66.1.0.0.255")]
        [TestCase("6.66.1.0.0.255")]
        [TestCase("6.66.2.0.0.255")]
        public void IsPeriod(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsPeriod;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("1.67.1.7.0.255")]
        [TestCase("1.67.2.7.0.255")]
        [TestCase("8.67.2.0.0.255")]
        [TestCase("6.67.8.0.0.255")]
        [TestCase("6.67.9.0.0.255")]
        public void IsAverage(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsAverage;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("0.0.96.3.10.255")]
        [TestCase("0.1.96.3.10.255")]
        [TestCase("0.8.96.3.10.255")]
        [TestCase("0.9.96.3.10.255")]
        public void IsDisconnectControl(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsDisconnectControl;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("1.65.1.8.0.255")]
        [TestCase("1.66.1.8.0.255")]
        [TestCase("1.67.1.7.0.255")]
        [TestCase("1.65.1.1.1.1")]
        [TestCase("1.68.25.67.0.255")]
        [TestCase("1.127.1.1.1.1")]
        public void IsUtilitySpecific(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.IsUtilitySpecific;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("1.0.1.8.0.255")]
        [TestCase("1.0.2.8.0.255")]
        [TestCase("8.0.1.0.0.255")]
        public void ToDelta(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.ToDelta();

            // Assert
            var fields = obisCode.Split('.');
            fields[1] = "65";
            Assert.That(result.ToString(), Is.EqualTo(string.Join(".", fields)));
        }

        [Test]
        [TestCase("1.0.1.8.0.255")]
        [TestCase("1.0.2.8.0.255")]
        [TestCase("8.0.1.0.0.255")]
        public void ToPeriod(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.ToPeriod();

            // Assert
            var fields = obisCode.Split('.');
            fields[1] = "66";
            Assert.That(result.ToString(), Is.EqualTo(string.Join(".", fields)));
        }

        [Test]
        [TestCase("1.0.1.8.0.255")]
        [TestCase("1.0.2.8.0.255")]
        [TestCase("8.0.1.0.0.255")]
        public void ToAverage(string obisCode)
        {
            // Arrange
            ObisCode target = obisCode;

            // Act
            var result = target.ToAverage();

            // Assert
            var fields = obisCode.Split('.');
            fields[1] = "67";
            Assert.That(result.ToString(), Is.EqualTo(string.Join(".", fields)));
        }

        [Test]
        public void GetDefined()
        {
            // Arrange

            // Act
            var definedObisCodes = ObisCode.GetDefined();

            // Assert
            Assert.That(definedObisCodes.Count(), Is.EqualTo(44));
        }
    }
}

