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

    public ICollection<CoarseTimeRegisterValue> Evaluate()
    {
      var a1Values = minuend.Evaluate();
      var a2Values = subtrahend.Evaluate();

      var addedValues = a1Values
        .Join(a2Values,
              x => new { x.CoarseTimestamp, x.TimeRegisterValue.UnitValue.Unit },
              x => new { x.CoarseTimestamp, x.TimeRegisterValue.UnitValue.Unit },
              (a1, a2) => new CoarseTimeRegisterValue(a1.CoarseTimestamp,
                new TimeRegisterValue(TimeRegisterValue.DummySerialNumber,
                                ValueExpressionSetHelper.GetMeanTimestamp(a1.TimeRegisterValue, a2.TimeRegisterValue),
                                a1.TimeRegisterValue.UnitValue.Value - a2.TimeRegisterValue.UnitValue.Value,
                                a1.TimeRegisterValue.UnitValue.Unit))).ToList();

      return addedValues;
    }


    public ICollection<TimeRegisterValue> Evaluate2()
    {
      var a1Values = minuend.Evaluate2();
      var a2Values = subtrahend.Evaluate2();

      var subtractedValues = a1Values
        .Join(a2Values,
              x => new { x.Timestamp, x.UnitValue.Unit },
              x => new { x.Timestamp, x.UnitValue.Unit },
              (a1, a2) => new TimeRegisterValue(TimeRegisterValue.DummySerialNumber, a1.Timestamp, a1.UnitValue - a2.UnitValue))
        .ToList();

      return subtractedValues;
    }

    #endregion

  }
}

