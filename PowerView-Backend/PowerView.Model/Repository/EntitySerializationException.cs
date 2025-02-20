using System;
using System.Runtime.Serialization;

namespace PowerView.Model.Repository
{
  [Serializable]
  public class EntitySerializationException : Exception
  {
    public EntitySerializationException() : base()
    {
    }

    public EntitySerializationException(string message) : base(message)
    {
    }

    public EntitySerializationException(string message, Exception e) : base(message, e)
    {
    }

    protected EntitySerializationException(SerializationInfo info, StreamingContext context) 
      : base(info, context)
    {
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
    }
  }
}

