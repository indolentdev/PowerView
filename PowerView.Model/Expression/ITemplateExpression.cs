using System;
using System.Collections.Generic;

namespace PowerView.Model.Expression
{
  public interface ITemplateExpression
  {
    bool IsSatisfied(IDictionary<string, ICollection<ObisCode>> labelsAndObisCodes);

    IValueExpressionSet GetValueExpressionSet(LabelProfileSet labelProfileSet, Func<DateTime, DateTime> timeDivider);
  }
}
