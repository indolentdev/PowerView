using System.Collections.Generic;

namespace PowerView.Model.Expression
{
  public interface IValueExpressionSet
  {
    ICollection<NormalizedTimeRegisterValue> Evaluate();
  }
}
