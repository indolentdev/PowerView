using System;

namespace PowerView.Model.Repository
{
    internal class EmailMessageRepository : RepositoryBase, IEmailMessageRepository
    {
        public EmailMessageRepository(IDbContext dbContext)
          : base(dbContext)
        {
        }

        public void AddEmailMessage(EmailRecipient frm, EmailRecipient to, string subject, string body)
        {
            ArgumentNullException.ThrowIfNull(frm);
            ArgumentNullException.ThrowIfNull(to);
            ArgumentNullException.ThrowIfNull(subject);
            ArgumentNullException.ThrowIfNull(body);

            var dbEmailMessage = new Db.EmailMessage
            {
                FromName = frm.Name,
                FromEmailAddress = frm.EmailAddress,
                ToName = to.Name,
                ToEmailAddress = to.EmailAddress,
                Subject = subject,
                Body = body
            };
            DbContext.ExecuteTransaction(
              "INSERT INTO EmailMessage (FromName,FromEmailAddress,ToName,ToEmailAddress,Subject,Body) VALUES (@FromName,@FromEmailAddress,@ToName,@ToEmailAddress,@Subject,@Body);",
              dbEmailMessage);
        }
    }
}

