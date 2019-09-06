using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.Expression
{
  public class ValueExpressionSet : IValueExpressionSet
  {
    private readonly IList<NormalizedTimeRegisterValue> normalizedTimeRegisterValues;

    public ValueExpressionSet(IEnumerable<NormalizedTimeRegisterValue> normalizedTimeRegisterValues)
    {
      if (normalizedTimeRegisterValues == null) throw new ArgumentNullException("normalizedTimeRegisterValues");

      this.normalizedTimeRegisterValues = normalizedTimeRegisterValues.ToList();
    }

    public ICollection<NormalizedTimeRegisterValue> Evaluate()
    {
      return normalizedTimeRegisterValues;
    }
  }
}
