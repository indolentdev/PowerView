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
      if (frm == null) throw new ArgumentNullException("frm");
      if (to == null) throw new ArgumentNullException("to");
      if (subject == null) throw new ArgumentNullException("subject");
      if (body == null) throw new ArgumentNullException("body");

      var dbEmailMessage = new Db.EmailMessage
      {
        FromName = frm.Name, FromEmailAddress = frm.EmailAddress,
        ToName = to.Name, ToEmailAddress = to.EmailAddress, Subject = subject, Body = body
      };
      DbContext.ExecuteTransaction("AddEmailMessage",
        "INSERT INTO EmailMessage (FromName,FromEmailAddress,ToName,ToEmailAddress,Subject,Body) VALUES (@FromName,@FromEmailAddress,@ToName,@ToEmailAddress,@Subject,@Body);",
        dbEmailMessage);
    }
  }
}

