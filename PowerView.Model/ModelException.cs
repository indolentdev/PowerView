using System;

namespace PowerView.Model
{
  public class ModelException : DataException
  {
    public ModelException()
    {
    }
      
    public ModelException(string message)
      : base(message)
    {
    }

    public ModelException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}

