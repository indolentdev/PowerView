using System;
using System.Runtime.Serialization;

namespace PowerView.Model.Expression
{
  [Serializable]
  public class TemplateExpressionException : Exception
  {
    public TemplateExpressionException()
    {
    }

    public TemplateExpressionException(string message) : base(message)
    {
    }

    public TemplateExpressionException(string message, Exception exception) : base(message, exception)
    {
    }

    protected TemplateExpressionException(SerializationInfo info, StreamingContext context)
    {
    }
  }

}

