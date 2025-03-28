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
  }
}

