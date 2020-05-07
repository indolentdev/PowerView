using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.Expression
{
  internal class SubtractValueExpressionSet : IValueExpressionSet
  {
    private readonly IValueExpressionSet minuend;
    private readonly IValueExpressionSet subtrahend;

    public SubtractValueExpressionSet(IValueExpressionSet minuend, IValueExpressionSet subtrahend)
    {
      if (minuend == null) throw new ArgumentNullException("minuend");
      if (subtrahend == null) throw new ArgumentNullException("subtrahend");

      this.minuend = minuend;
      this.subtrahend = subtrahend;
    }

    #region IValueExpressionSet implementation

    public ICollection<NormalizedTimeRegisterValue> Evaluate()
    {
      var a1Values = minuend.Evaluate();
      var a2Values = subtrahend.Evaluate();

      var addedValues = a1Values
        .Join(a2Values,
              x => new { x.NormalizedTimestamp, x.TimeRegisterValue.UnitValue.Unit },
              x => new { x.NormalizedTimestamp, x.TimeRegisterValue.UnitValue.Unit },
              (a1, a2) => new NormalizedTimeRegisterValue(
                new TimeRegisterValue(TimeRegisterValue.DummyDeviceId,
                                ValueExpressionSetHelper.GetMeanTimestamp(a1.TimeRegisterValue, a2.TimeRegisterValue),
                                a1.TimeRegisterValue.UnitValue.Value - a2.TimeRegisterValue.UnitValue.Value,
                                a1.TimeRegisterValue.UnitValue.Unit),
                a1.NormalizedTimestamp))
        .ToList();

      return addedValues;
    }

    #endregion

  }
}

