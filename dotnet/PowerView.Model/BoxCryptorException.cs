using System;

namespace PowerView.Model
{
  public class BoxCryptorException : DataException
  {
    public BoxCryptorException()
    {
    }
      
    public BoxCryptorException(string message)
      : base(message)
    {
    }

    public BoxCryptorException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}
