using System.Collections.Generic;

namespace PowerView.Model.Expression
{
  public interface IValueExpressionSet
  {
    ICollection<CoarseTimeRegisterValue> Evaluate();

    ICollection<TimeRegisterValue> Evaluate2();
  }
}
