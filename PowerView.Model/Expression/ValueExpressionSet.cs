using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.Expression
{
  public class ValueExpressionSet : IValueExpressionSet
  {
    private readonly IList<CoarseTimeRegisterValue> coarseTimeRegisterValues;
    private readonly IList<TimeRegisterValue> timeRegisterValues;

    public ValueExpressionSet(IEnumerable<CoarseTimeRegisterValue> coarseTimeRegisterValues)
    {
      if (coarseTimeRegisterValues == null) throw new ArgumentNullException("coarseTimeRegisterValues");

      this.coarseTimeRegisterValues = coarseTimeRegisterValues.ToList();
    }

    public ValueExpressionSet(IEnumerable<TimeRegisterValue> timeRegisterValues)
    {
      if (timeRegisterValues == null) throw new ArgumentNullException("timeRegisterValues");

      this.timeRegisterValues = timeRegisterValues.ToList();
    }

    public ICollection<CoarseTimeRegisterValue> Evaluate()
    {
      return coarseTimeRegisterValues;
    }

    public ICollection<TimeRegisterValue> Evaluate2()
    {
      return timeRegisterValues;
    }
  }
}
