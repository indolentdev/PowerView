using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class GeneratorSeriesTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            var series = new SeriesName("lbl1", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat);
            var baseSeries = new SeriesName("lbl2", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat);
            const string costBreakdownTitle = "cbTitle";

            // Act & Assert
            Assert.That(() => new GeneratorSeries(null, baseSeries, costBreakdownTitle), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new GeneratorSeries(series, null, costBreakdownTitle), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new GeneratorSeries(series, baseSeries, null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => new GeneratorSeries(series, baseSeries, string.Empty), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange
            var series = new SeriesName("lbl1", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat);
            var baseSeries = new SeriesName("lbl2", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat);
            const string costBreakdownTitle = "cbTitle";

            // Act
            var target = new GeneratorSeries(series, baseSeries, costBreakdownTitle);

            // Assert
            Assert.That(target.Series, Is.EqualTo(series));
            Assert.That(target.BaseSeries, Is.EqualTo(baseSeries));
            Assert.That(target.CostBreakdownTitle, Is.EqualTo(costBreakdownTitle));
        }

        [Test]
        public void EqualsAndHashCode()
        {
            // Arrange
            var t1 = new GeneratorSeries(new SeriesName("lbl", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat), new SeriesName("lbl2", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "CbTitle");
            var t2 = new GeneratorSeries(new SeriesName("lbl", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat), new SeriesName("lbl2", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "CbTitle");
            var t3 = new GeneratorSeries(new SeriesName("zzz", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat), new SeriesName("lbl2", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "CbTitle");
            var t4 = new GeneratorSeries(new SeriesName("lbl", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat), new SeriesName("zzz", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "CbTitle");
            var t5 = new GeneratorSeries(new SeriesName("lbl", ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat), new SeriesName("lbl2", ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "zzz");

            // Act & Assert
            Assert.That(t1, Is.EqualTo(t2));
            Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t3));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t3.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t4));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t4.GetHashCode()));
            Assert.That(t1, Is.Not.EqualTo(t5));
            Assert.That(t1.GetHashCode(), Is.Not.EqualTo(t5.GetHashCode()));

            Assert.That(t1.Equals((object)t2), Is.True);
            Assert.That(t1.Equals((object)t3), Is.False);
        }

        [Test]
        [TestCase("1.69.25.67.0.255", "1.68.25.67.0.255", "60-minutes", true)]
        [TestCase("1.69.25.67.0.255", "1.68.25.67.0.255", "30-minutes", false)]
        [TestCase("1.69.25.67.0.255", "1.70.25.67.0.255", "60-minutes", false)]
        [TestCase("1.70.25.67.0.255", "1.68.25.67.0.255", "60-minutes", false)]
        public void SupportsInterval(string obisCode, string baseObisCode, string interval, bool expected)
        {
            // Arrange
            var target = new GeneratorSeries(new SeriesName("lbl", obisCode), new SeriesName("lbl", baseObisCode), "cbTitle");

            // Act
            var supported = target.SupportsInterval(interval);

            // Assert
            Assert.That(supported, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("1.69.25.67.0.255", "1.68.25.67.0.255", "60-minutes", "2024-05-12T20:00:00Z", true)]
        [TestCase("1.69.25.67.0.255", "1.68.25.67.99.255", "60-minutes", "2024-05-12T20:00:00Z", false)]
        [TestCase("1.69.25.67.0.255", "1.68.25.67.0.255", "60-minutes", "2024-05-12T20:00:01Z", false)]
        public void SupportsDurations(string obisCode, string baseObisCode, string interval, string timestampString, bool expected)
        {
            // Arrange
            var target = new GeneratorSeries(new SeriesName("lbl", obisCode), new SeriesName("lbl", baseObisCode), "cbTitle");
            var timestamp = DateTime.Parse(timestampString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var values = new[] { new NormalizedDurationRegisterValue(
            timestamp, timestamp.AddMinutes(60),
            timestamp, timestamp.AddMinutes(60),
            new UnitValue(12.34, Unit.Eur), "EnergiDataService" ) };

            // Act
            var supported = target.SupportsDurations(values);

            // Assert
            Assert.That(supported, Is.EqualTo(expected));
        }

    }
}
