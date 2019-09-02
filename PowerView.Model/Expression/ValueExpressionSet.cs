using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.Expression
{
  public class ValueExpressionSet : IValueExpressionSet
  {
    private readonly IList<CoarseTimeRegisterValue> coarseTimeRegisterValues;
    private readonly IList<NormalizedTimeRegisterValue> normalizedTimeRegisterValues;

    public ValueExpressionSet(IEnumerable<CoarseTimeRegisterValue> coarseTimeRegisterValues)
    {
      if (coarseTimeRegisterValues == null) throw new ArgumentNullException("coarseTimeRegisterValues");

      this.coarseTimeRegisterValues = coarseTimeRegisterValues.ToList();
    }

    public ValueExpressionSet(IEnumerable<NormalizedTimeRegisterValue> normalizedTimeRegisterValues)
    {
      if (normalizedTimeRegisterValues == null) throw new ArgumentNullException("normalizedTimeRegisterValues");

      this.normalizedTimeRegisterValues = normalizedTimeRegisterValues.ToList();
    }

    public ICollection<CoarseTimeRegisterValue> Evaluate()
    {
      return coarseTimeRegisterValues;
    }

    public ICollection<NormalizedTimeRegisterValue> Evaluate2()
    {
      return normalizedTimeRegisterValues;
    }
  }
}
