using System;
using System.Runtime.Serialization;

namespace PowerView.Model.Expression
{
  [Serializable]
  public class ValueExpressionSetException : Exception
  {
    public ValueExpressionSetException()
    {
    }

    public ValueExpressionSetException(string message) : base(message)
    {
    }

    public ValueExpressionSetException(string message, Exception exception) : base(message, exception)
    {
    }

    protected ValueExpressionSetException(SerializationInfo info, StreamingContext context)
    {
    }
  }

}

