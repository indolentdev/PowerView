using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.Expression
{
  public class ValueExpressionSet : IValueExpressionSet
  {
    private readonly IList<CoarseTimeRegisterValue> coarseTimeRegisterValues;
    
    public ValueExpressionSet(IEnumerable<CoarseTimeRegisterValue> coarseTimeRegisterValues)
    {
      if (coarseTimeRegisterValues == null) throw new ArgumentNullException("coarseTimeRegisterValues");

      this.coarseTimeRegisterValues = coarseTimeRegisterValues.ToList();
    }

    public ICollection<CoarseTimeRegisterValue> Evaluate()
    {
      return coarseTimeRegisterValues;
    }
  }
}
