using System;
using System.Linq;

namespace PowerView.Model.Expression
{
  public class TemplateExpressionFactory : ITemplateExpressionFactory
  {
    public ITemplateExpression Create(string template)
    {
      if (string.IsNullOrEmpty(template)) throw new ArgumentNullException("template");

      var trimmedTemplate = RemoveWhitespace(template);
      return Parse(trimmedTemplate);
    }

    private static string RemoveWhitespace(string input)
    {
      return new string(input.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    private static ITemplateExpression Parse(string template)
    {
      string part;
      string op;

      FindPartAndOperator(ref template, new [] {'+','-'}, out part, out op);
      if (!string.IsNullOrEmpty(op))
      {
        var left = Parse(template);
        var right = Parse(part);
        return new OperationTemplateExpression(left, op, right);
      }

      return new RegisterTemplateExpression(part);
    }

    private static void FindPartAndOperator(ref string template, char[] operators, out string part, out string op)
    {
      var opIx = template.LastIndexOfAny(operators);
      if (opIx == -1)
      {
        part = template;
        op = string.Empty;
        template = string.Empty;
        return;
      }

      op = template.Substring(opIx, 1);
      part = template.Substring(opIx + op.Length);
      template = template.Substring(0, opIx);
    }

  }
}

