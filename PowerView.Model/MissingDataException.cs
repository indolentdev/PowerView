using System;

namespace PowerView.Model
{
  public class MissingDataException : DataException
  {
    public MissingDataException()
    {
    }
      
    public MissingDataException(string message)
      : base(message)
    {
    }

    public MissingDataException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}

