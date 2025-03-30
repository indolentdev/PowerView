using PowerView.Model;

namespace PowerView.Service.Mailer
{
    public interface IMailMediator
    {
        void SendEmail(EmailRecipient emailRecipient, string subject, string message);
    }
}
