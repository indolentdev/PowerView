using System;
using System.Runtime.Serialization;
//using System.Security.Permissions;

namespace PowerView.Service.Mailer
{
  [Serializable]
  public class AuthenticateMailerException : MailerException
  {
    public AuthenticateMailerException()
    {
    }

    public AuthenticateMailerException(string message)
      : base(message)
    {
    }

    public AuthenticateMailerException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected AuthenticateMailerException(SerializationInfo info, StreamingContext context)
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

