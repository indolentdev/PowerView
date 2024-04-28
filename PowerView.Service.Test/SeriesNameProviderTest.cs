using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using PowerView.Service.EventHub;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.Test;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SeriesNameProviderTest
{
    private readonly ILocationContext locationContext;
    private readonly Mock<ISeriesNameRepository> seriesNameRepository;
    private readonly Mock<ICostBreakdownGeneratorSeriesRepository> costBreakdownGeneratorSeriesRepository;

    private SeriesNameProvider target;

    public SeriesNameProviderTest()
    {
        locationContext = TimeZoneHelper.GetDenmarkLocationContext();
        seriesNameRepository = new Mock<ISeriesNameRepository>();
        costBreakdownGeneratorSeriesRepository = new Mock<ICostBreakdownGeneratorSeriesRepository>();

        target = new SeriesNameProvider(locationContext, seriesNameRepository.Object, costBreakdownGeneratorSeriesRepository.Object);

    }

    [Test]
    public void ConstructorThrows()
    {
        // Arrange

        // Act & Assert
        Assert.That(() => new SeriesNameProvider(null, seriesNameRepository.Object, costBreakdownGeneratorSeriesRepository.Object), Throws.ArgumentNullException);
        Assert.That(() => new SeriesNameProvider(locationContext, null, costBreakdownGeneratorSeriesRepository.Object), Throws.ArgumentNullException);
        Assert.That(() => new SeriesNameProvider(locationContext, seriesNameRepository.Object, null), Throws.ArgumentNullException);
    }

    [Test]
    public void GetSeriesNames()
    {
        // Arrange
        const string label1 = "lbl1";
        const string label2 = "lbl2";
        ObisCode obisCode1 = "1.2.3.4.5.6";
        ObisCode obisCode2 = "6.5.4.3.2.1";
        ObisCode obisCode3 = "3.4.3.4.3.4";
        var seriesNames = new[] { new SeriesName(label1, obisCode1), new SeriesName(label2, obisCode2), 
          new SeriesName(label1, obisCode3), new SeriesName(label2, obisCode3) };
        seriesNameRepository.Setup(x => x.GetSeriesNames()).Returns(seriesNames);
        costBreakdownGeneratorSeriesRepository.Setup(x => x.GetCostBreakdownGeneratorSeries()).Returns(Array.Empty<CostBreakdownGeneratorSeries>());

        // Act
        var serieNames = target.GetSeriesNames();

        // Assert
        Assert.That(serieNames, Is.EquivalentTo(seriesNames));
        seriesNameRepository.Verify(x => x.GetSeriesNames());
        costBreakdownGeneratorSeriesRepository.Verify(x => x.GetCostBreakdownGeneratorSeries());
    }

    [Test]
    public void GetSeriesNamesReplaceCumulativeObisCodes()
    {
        // Arrange
        const string label = "lbl1";
        var seriesNames = new[] { new SeriesName(label, ObisCode.ElectrActiveEnergyA14) };
        seriesNameRepository.Setup(x => x.GetSeriesNames()).Returns(seriesNames);
        costBreakdownGeneratorSeriesRepository.Setup(x => x.GetCostBreakdownGeneratorSeries()).Returns(Array.Empty<CostBreakdownGeneratorSeries>());

        // Act
        var serieNames = target.GetSeriesNames();

        // Assert
        Assert.That(serieNames.Count, Is.EqualTo(3));
        Assert.That(serieNames.Count(sc => sc.Label == label && sc.ObisCode == ObisCode.ElectrActiveEnergyA14Period), Is.EqualTo(1));
        Assert.That(serieNames.Count(sc => sc.Label == label && sc.ObisCode == ObisCode.ElectrActiveEnergyA14Delta), Is.EqualTo(1));
        Assert.That(serieNames.Count(sc => sc.Label == label && sc.ObisCode == ObisCode.ElectrActualPowerP14Average), Is.EqualTo(1));
    }

    [Test]
    [TestCase(Unit.Dkk)]
    [TestCase(Unit.Eur)]
    public void GetSeriesNamesAddCostBreakdownGeneratorSeriesObisCode(Unit unit)
    {
        // Arrange
        const string label = "lbl1";
        var seriesNames = new[] { new SeriesName(label, ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat) };
        seriesNameRepository.Setup(x => x.GetSeriesNames()).Returns(seriesNames);
        var utcNow = DateTime.UtcNow;
        var costBreakdownGeneratorSeries = new CostBreakdownGeneratorSeries(
            new CostBreakdown("CbTitle", unit, 25, new [] 
            { 
                new CostBreakdownEntry(utcNow - TimeSpan.FromDays(2), utcNow + TimeSpan.FromDays(2), "entry name", 0, 23, 2) 
            }),
            new GeneratorSeries(new SeriesName(label, ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat), new SeriesName(label, ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), "CbTitle")
        );
        costBreakdownGeneratorSeriesRepository.Setup(x => x.GetCostBreakdownGeneratorSeries()).Returns(Array.Empty<CostBreakdownGeneratorSeries>());

        // Act
        var serieNames = target.GetSeriesNames();

        // Assert
        Assert.That(serieNames.Count, Is.EqualTo(2));
        Assert.That(serieNames.Count(sc => sc.Label == label && sc.ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpenseExclVat), Is.EqualTo(1));
        Assert.That(serieNames.Count(sc => sc.Label == label && sc.ObisCode == ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat), Is.EqualTo(1));
    }

}
