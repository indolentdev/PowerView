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
  }
}

