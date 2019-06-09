using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.Expression
{
  internal class AddValueExpressionSet : IValueExpressionSet
  {
    private readonly IValueExpressionSet addend1;
    private readonly IValueExpressionSet addend2;

    public AddValueExpressionSet(IValueExpressionSet addend1, IValueExpressionSet addend2)
    {
      if (addend1 == null) throw new ArgumentNullException("addend1");
      if (addend2 == null) throw new ArgumentNullException("addend2");

      this.addend1 = addend1;
      this.addend2 = addend2;
    }

    #region IValueExpressionSet implementation

    public ICollection<CoarseTimeRegisterValue> Evaluate()
    {
      var a1Values = addend1.Evaluate();
      var a2Values = addend2.Evaluate();

      var addedValues = a1Values
        .Join(a2Values,
              x => new { x.CoarseTimestamp, x.TimeRegisterValue.UnitValue.Unit },
              x => new { x.CoarseTimestamp, x.TimeRegisterValue.UnitValue.Unit },
              (a1, a2) => new CoarseTimeRegisterValue(a1.CoarseTimestamp,
                new TimeRegisterValue(TimeRegisterValue.DummySerialNumber,
                                ValueExpressionSetHelper.GetMeanTimestamp(a1.TimeRegisterValue, a2.TimeRegisterValue),
                                a1.TimeRegisterValue.UnitValue.Value + a2.TimeRegisterValue.UnitValue.Value,
                                a1.TimeRegisterValue.UnitValue.Unit))).ToList();

      return addedValues;
    }

    #endregion

  }
}

