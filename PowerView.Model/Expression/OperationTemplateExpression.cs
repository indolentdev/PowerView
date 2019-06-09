using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace PowerView.Model.Expression
{
  internal class OperationTemplateExpression : ITemplateExpression
  {
    private const string Plus = "+";
    private const string Minus = "-";
    private readonly string[] supportedOperators = { Plus, Minus };
    
    public ITemplateExpression Left { get; private set; }
    public string Operator { get; private set; }
    public ITemplateExpression Right { get; private set; }

    public OperationTemplateExpression(ITemplateExpression left, string op, ITemplateExpression right)
    {
      if (left == null) throw new ArgumentNullException("left");
      if (string.IsNullOrEmpty(op)) throw new ArgumentNullException("op");
      if (right == null) throw new ArgumentNullException("right");

      if (!supportedOperators.Contains(op))
      {
        var msg = string.Format(CultureInfo.InvariantCulture, "Operator not supported:{0}", op);
        throw new TemplateExpressionException(msg);
      }

      Left = left;
      Operator = op;
      Right = right;
    }

    public bool IsSatisfied(IDictionary<string, ICollection<ObisCode>> labelsAndObisCodes)
    {
      return Left.IsSatisfied(labelsAndObisCodes) && Right.IsSatisfied(labelsAndObisCodes);
    }

    public IValueExpressionSet GetValueExpressionSet(LabelProfileSet labelProfileSet, Func<DateTime, DateTime> timeDivider)
    {
      var valueExpressionSetLeft = Left.GetValueExpressionSet(labelProfileSet, timeDivider);
      var valueExpressionSetRight = Right.GetValueExpressionSet(labelProfileSet, timeDivider);

      switch (Operator)
      {
      case Plus:
        return new AddValueExpressionSet(valueExpressionSetLeft, valueExpressionSetRight);
      case Minus:
        return new SubtractValueExpressionSet(valueExpressionSetLeft, valueExpressionSetRight);

      default:
        throw new NotImplementedException("Missing impl of operator:" + Operator);
      }
    }

  }
}

