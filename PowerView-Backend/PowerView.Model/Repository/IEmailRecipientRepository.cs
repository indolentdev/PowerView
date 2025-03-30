using System.Collections.Generic;

namespace PowerView.Model.Repository
{
    public interface IEmailRecipientRepository
    {
        EmailRecipient GetEmailRecipient(string emailAddress);

        IList<EmailRecipient> GetEmailRecipients();

        void AddEmailRecipient(EmailRecipient emailRecipient);

        void DeleteEmailRecipient(string emailAddress);

        IDictionary<EmailRecipient, long?> GetEmailRecipientsMeterEventPosition();

        void SetEmailRecipientMeterEventPosition(string emailAddress, long meterEventId);
    }
}
