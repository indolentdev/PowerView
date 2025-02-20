using System;
using System.Runtime.Serialization;
//using System.Security.Permissions;

namespace PowerView.Service.Mailer
{
  [Serializable]
  public class MailerException : Exception
  {
    public MailerException()
    {
    }

    public MailerException(string message)
      : base(message)
    {
    }

    public MailerException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected MailerException(SerializationInfo info, StreamingContext context)
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

