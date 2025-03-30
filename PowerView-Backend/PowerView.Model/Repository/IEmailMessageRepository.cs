
namespace PowerView.Model.Repository
{
    public interface IEmailMessageRepository
    {
        void AddEmailMessage(EmailRecipient frm, EmailRecipient to, string subject, string body);
    }
}
