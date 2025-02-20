using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model
{
  public class CostBreakdownGeneratorSeries
  {
    public CostBreakdownGeneratorSeries(CostBreakdown costBreakdown, GeneratorSeries generatorSeries)
    {
      CostBreakdown = costBreakdown ?? throw new ArgumentNullException(nameof(costBreakdown));
      GeneratorSeries = generatorSeries ?? throw new ArgumentNullException(nameof(generatorSeries));

      if (CostBreakdown.Title != GeneratorSeries.CostBreakdownTitle) throw new ArgumentOutOfRangeException(nameof(generatorSeries), "CostBreakdown and GeneratorSeries must match");
    }

    public CostBreakdown CostBreakdown { get; private set; }
    public GeneratorSeries GeneratorSeries { get; private set; }
  }
}

