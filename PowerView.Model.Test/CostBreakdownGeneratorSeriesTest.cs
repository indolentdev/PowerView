using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test;

[TestFixture]
public class CostBreakDownGeneratorSeriesTest
{
    [Test]
    public void ConstructorThrows()
    {
        // Arrange
        var costBreakdown = new CostBreakdown("Title", Unit.Dkk, 10, Array.Empty<CostBreakdownEntry>());
        var generatorSeries = new GeneratorSeries(new SeriesName("n", "1.2.3.4.5.6"), new SeriesName("b", "6.5.4.3.2.1"), "OtherTitle");

        // Act & Assert
        Assert.That(() => new CostBreakdownGeneratorSeries(null, generatorSeries), Throws.ArgumentNullException);
        Assert.That(() => new CostBreakdownGeneratorSeries(costBreakdown, null), Throws.ArgumentNullException);
        Assert.That(() => new CostBreakdownGeneratorSeries(costBreakdown, generatorSeries), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAndProperties()
    {
        // Arrange
        var costBreakdown = new CostBreakdown("Title", Unit.Dkk, 10, Array.Empty<CostBreakdownEntry>());
        var generatorSeries = new GeneratorSeries(new SeriesName("n", "1.2.3.4.5.6"), new SeriesName("b", "6.5.4.3.2.1"), "Title");

        // Act
        var target = new CostBreakdownGeneratorSeries(costBreakdown, generatorSeries);

        // Assert
        Assert.That(target.CostBreakdown, Is.SameAs(costBreakdown));
        Assert.That(target.GeneratorSeries, Is.SameAs(generatorSeries));
    }

}