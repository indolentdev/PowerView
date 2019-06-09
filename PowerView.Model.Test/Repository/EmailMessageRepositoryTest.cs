using System;
using System.Linq;
using NUnit.Framework;
using DapperExtensions;
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
      var frmName = Predicates.Field<Db.EmailMessage>(x => x.FromName, Operator.Eq, frm.Name);
      var frmAddr = Predicates.Field<Db.EmailMessage>(x => x.FromEmailAddress, Operator.Eq, frm.EmailAddress);
      var toName = Predicates.Field<Db.EmailMessage>(x => x.ToName, Operator.Eq, to.Name);
      var toAddr = Predicates.Field<Db.EmailMessage>(x => x.ToEmailAddress, Operator.Eq, to.EmailAddress);
      var sub = Predicates.Field<Db.EmailMessage>(x => x.Subject, Operator.Eq, subject);
      var bdy = Predicates.Field<Db.EmailMessage>(x => x.Body, Operator.Eq, body);
      var ands = Predicates.Group(GroupOperator.And, frmName, frmAddr, toName, toAddr, sub, bdy);
      var count = DbContext.Connection.Count<Db.EmailMessage>(ands);
      Assert.That(count, Is.EqualTo(1));
    }

    private EmailMessageRepository CreateTarget()
    {
      return new EmailMessageRepository(DbContext);
    }

  }
}
