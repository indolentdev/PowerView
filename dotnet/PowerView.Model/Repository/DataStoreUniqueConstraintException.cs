using System;

namespace PowerView.Model.Repository
{
  public class DataStoreUniqueConstraintException : DataStoreException
  {
    public DataStoreUniqueConstraintException()
    {
    }
      
    public DataStoreUniqueConstraintException(string message)
      : base(message)
    {
    }

    public DataStoreUniqueConstraintException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}

