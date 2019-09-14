using System;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class EmailMessageRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new EmailMessageRepository(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void AddEmailMessageThrows()
    {
      // Arrange
      var emailRecipient = new EmailRecipient("The Name", "a@b.com");
      const string subject = "subject";
      const string body = "body";
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddEmailMessage(null, emailRecipient, subject, body), Throws.ArgumentNullException);
      Assert.That(() => target.AddEmailMessage(emailRecipient, null, subject, body), Throws.ArgumentNullException);
      Assert.That(() => target.AddEmailMessage(emailRecipient, emailRecipient, null, body), Throws.ArgumentNullException);
      Assert.That(() => target.AddEmailMessage(emailRecipient, emailRecipient, subject, null), Throws.ArgumentNullException);
    }

    [Test]
    public void AddEmailMessage()
    {
      // Arrange
      var frm = new EmailRecipient("The Sender", "sender@b.com");
      var to = new EmailRecipient("The Receiver", "receiver@b.com");
      const string subject = "subject";
      const string body = "body";
      var target = CreateTarget();

      // Act
      target.AddEmailMessage(frm, to, subject, body);

      // Assert
      var emailMessages = DbContext.QueryTransaction<Db.EmailMessage>("",
        "SELECT * FROM EmailMessage WHERE FromName=@fromName AND FromEmailAddress=@fromEmailAddress AND ToName=@toName AND ToEmailAddress=@toEmailAddress AND Subject=@subject AND Body=@body",
        new { fromName = frm.Name, fromEmailAddress = frm.EmailAddress, toName = to.Name, toEmailAddress = to.EmailAddress, subject, body });
      Assert.That(emailMessages.Count, Is.EqualTo(1));
    }

    private EmailMessageRepository CreateTarget()
    {
      return new EmailMessageRepository(DbContext);
    }

  }
}
