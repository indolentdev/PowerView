
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
  }
}

