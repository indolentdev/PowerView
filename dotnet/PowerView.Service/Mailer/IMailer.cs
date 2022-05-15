using PowerView.Model;

namespace PowerView.Service.Mailer
{
  public interface IMailer
  {
    EmailRecipient Send(SmtpConfig smtpConfig, EmailRecipient emailRecipient, string subject, string message);
  }
}
