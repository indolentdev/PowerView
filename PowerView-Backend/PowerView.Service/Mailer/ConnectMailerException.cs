using System;
using System.Runtime.Serialization;
//using System.Security.Permissions;

namespace PowerView.Service.Mailer
{
  [Serializable]
  public class ConnectMailerException : MailerException
  {
    public ConnectMailerException()
    {
    }

    public ConnectMailerException(string message)
      : base(message)
    {
    }

    public ConnectMailerException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected ConnectMailerException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    //    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    //    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    //    {
    //      base.GetObjectData(info, context);
    //    }
  }
}

