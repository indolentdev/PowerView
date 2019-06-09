using System;
using System.Linq;
using NUnit.Framework;
using DapperExtensions;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class EmailRecipientRepositoryTest : DbTestFixtureWithSchema
  {
    [SetUp]
    public override void SetUp()
    {
      base.SetUp();
    }
    
    [Test]
    public void ConstructorThrows()
    {
      // Arrange

      // Act & Assert
      Assert.That(() => new EmailRecipientRepository(null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void GetEmailRecipient()
    {
      // Arrange
      var target = CreateTarget();
      var er1 = new Db.EmailRecipient { Name = "N1", EmailAddress = "a@b.com" };
      var er2 = new Db.EmailRecipient { Name = "N2", EmailAddress = "p@q.com" };
      DbContext.InsertTransaction<Db.EmailRecipient>(null, new[] { er1, er2 });

      // Act
      var emailRecipient = target.GetEmailRecipient(er1.EmailAddress);

      // Assert
      AssertEmailRecipient(er1, emailRecipient);
    }

    [Test]
    public void GetEmailRecipients()
    {
      // Arrange
      var target = CreateTarget();
      var er1 = new Db.EmailRecipient { Name = "N1", EmailAddress = "a@b.com" };
      var er2 = new Db.EmailRecipient { Name = "N2", EmailAddress = "p@q.com" };
      DbContext.InsertTransaction<Db.EmailRecipient>(null, new [] { er1, er2 });

      // Act
      var emailRecipients = target.GetEmailRecipients();

      // Assert
      Assert.That(emailRecipients.Count, Is.EqualTo(2));
      AssertEmailRecipient(er1, emailRecipients[0]);
      AssertEmailRecipient(er2, emailRecipients[1]);
    }

    private static void AssertEmailRecipient(Db.EmailRecipient expectedEmailRecipient, EmailRecipient actualEmailRecipient)
    {
      Assert.That(actualEmailRecipient.Name, Is.EqualTo(expectedEmailRecipient.Name));
      Assert.That(actualEmailRecipient.EmailAddress, Is.EqualTo(expectedEmailRecipient.EmailAddress));
    }

    [Test]
    public void AddEmailRecipientThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.AddEmailRecipient(null), Throws.ArgumentNullException);
    }

    [Test]
    public void AddEmailRecipient()
    {
      // Arrange
      var target = CreateTarget();
      var emailRecipient = new EmailRecipient("TheName", "a@b.com");

      // Act
      target.AddEmailRecipient(emailRecipient);

      // Assert
      AssertEmailRecipientExists(emailRecipient);
      Assert.That(DbContext.Connection.Count<Db.EmailRecipientMeterEventPosition>(null), Is.EqualTo(0));
    }

    [Test]
    public void AddEmailRecipientMeterEventExists()
    {
      // Arrange
      var target = CreateTarget();
      var emailRecipient = new EmailRecipient("TheName", "a@b.com");
      var dbMeterEvent = new Db.MeterEvent { Label = "ehh", MeterEventType = "ohh", Amplification = "ihh", Flag = true };
      DbContext.InsertTransaction(null, dbMeterEvent);

      // Act
      target.AddEmailRecipient(emailRecipient);

      // Assert
      AssertEmailRecipientExists(emailRecipient);
      var erId = DbContext.QueryTransaction<long>(null, "SELECT [Id] FROM [EmailRecipient] WHERE [EmailAddress]=@emailAddress;", 
                                                    new { emailAddress = emailRecipient.EmailAddress }).First();
      var pEr = Predicates.Field<Db.EmailRecipientMeterEventPosition>(sc => sc.EmailRecipientId, Operator.Eq, erId);
      var pMe = Predicates.Field<Db.EmailRecipientMeterEventPosition>(sc => sc.MeterEventId, Operator.Eq, dbMeterEvent.Id);
      Assert.That(DbContext.Connection.Count<Db.EmailRecipientMeterEventPosition>(Predicates.Group(GroupOperator.And, pEr, pMe)), Is.EqualTo(1));
    }

    [Test]
    public void AddEmailRecipientDuplicateKey()
    {
      // Arrange
      var target = CreateTarget();
      var emailRecipient = new EmailRecipient("TheName", "a@b.com");
      target.AddEmailRecipient(emailRecipient);

      // Act & Assert
      Assert.That(() => target.AddEmailRecipient(emailRecipient), Throws.TypeOf<DataStoreUniqueConstraintException>());
    }


    private void AssertEmailRecipientExists(EmailRecipient emailRecipient, bool not = false)
    {
      var predicateName = Predicates.Field<Db.EmailRecipient>(sc => sc.Name, Operator.Eq, emailRecipient.Name);
      var predicateEmailAddress = Predicates.Field<Db.EmailRecipient>(sc => sc.EmailAddress, Operator.Eq, emailRecipient.EmailAddress);
      var predicate = Predicates.Group(GroupOperator.And, predicateName, predicateEmailAddress);
      Assert.That(DbContext.Connection.Count<Db.EmailRecipient>(predicate), Is.EqualTo(not ? 0 : 1));
    }

    [Test]
    public void DeleteEmailRecipientNoMatchingRow()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      target.DeleteEmailRecipient("NonExistingEmailAddress");

      // Assert
    }

    [Test]
    public void DeleteEmailRecipient()
    {
      // Arrange
      var target = CreateTarget();
      var emailRecipient = new EmailRecipient("TheName", "a@b.com");
      target.AddEmailRecipient(emailRecipient);
      var dbMeterEvent = new Db.MeterEvent { Label = "ehh", MeterEventType = "ohh", Amplification = "ihh", Flag = true };
      DbContext.InsertTransaction(null, dbMeterEvent);
      var dbEmailRecipient = DbContext.GetPage<Db.EmailRecipient>(null, 0, 1, Predicates.Field<Db.EmailRecipient>(x => x.EmailAddress, Operator.Eq, emailRecipient.EmailAddress)).First();
      DbContext.InsertTransaction(null, new Db.EmailRecipientMeterEventPosition { EmailRecipientId = dbEmailRecipient.Id, MeterEventId = dbMeterEvent.Id });

      // Act
      target.DeleteEmailRecipient(emailRecipient.EmailAddress);

      // Assert
      var predicate = Predicates.Field<Db.EmailRecipient>(sc => sc.EmailAddress, Operator.Eq, emailRecipient.EmailAddress);
      Assert.That(DbContext.Connection.Count<Db.EmailRecipient>(predicate), Is.EqualTo(0));
      Assert.That(DbContext.Connection.Count<Db.EmailRecipientMeterEventPosition>(null), Is.EqualTo(0));
    }

    [Test]
    public void GetEmailRecipientsMeterEventPositionWithoutPosition()
    {
      // Arrange
      var target = CreateTarget();
      var er1 = new Db.EmailRecipient { Name = "N1", EmailAddress = "a@b.com" };
      DbContext.InsertTransaction(null, er1);

      // Act
      var emailRecipientsMeterEventPosition = target.GetEmailRecipientsMeterEventPosition();

      // Assert
      Assert.That(emailRecipientsMeterEventPosition.Count, Is.EqualTo(1));
      AssertEmailRecipient(er1, emailRecipientsMeterEventPosition.First().Key);
      Assert.That(emailRecipientsMeterEventPosition.First().Value, Is.Null);
    }

    [Test]
    public void GetEmailRecipientsMeterEventPositionWithPosition()
    {
      // Arrange
      var target = CreateTarget();
      var er = new Db.EmailRecipient { Name = "N1", EmailAddress = "a@b.com" };
      DbContext.InsertTransaction(null, er);
      var me = new Db.MeterEvent { Label="lbl", MeterEventType="met", DetectTimestamp=DateTime.UtcNow, Amplification="Amp" };
      DbContext.InsertTransaction(null, me);
      DbContext.InsertTransaction(null, new Db.EmailRecipientMeterEventPosition { EmailRecipientId = er.Id, MeterEventId = me.Id });

      // Act
      var emailRecipientsMeterEventPosition = target.GetEmailRecipientsMeterEventPosition();

      // Assert
      Assert.That(emailRecipientsMeterEventPosition.Count, Is.EqualTo(1));
      AssertEmailRecipient(er, emailRecipientsMeterEventPosition.First().Key);
      Assert.That(emailRecipientsMeterEventPosition.First().Value, Is.EqualTo(me.Id));
    }

    [Test]
    public void SetEmailRecipientMeterEventPositionInserts()
    {
      // Arrange
      var target = CreateTarget();
      var er = new Db.EmailRecipient { Name = "N1", EmailAddress = "a@b.com" };
      DbContext.InsertTransaction(null, er);
      var me = new Db.MeterEvent { Label="lbl", MeterEventType="met", DetectTimestamp=DateTime.UtcNow, Amplification="Amp" };
      DbContext.InsertTransaction(null, me);

      // Act
      target.SetEmailRecipientMeterEventPosition(er.EmailAddress, me.Id);

      // Assert
      var pEr = Predicates.Field<Db.EmailRecipientMeterEventPosition>(sc => sc.EmailRecipientId, Operator.Eq, er.Id);
      var pMe = Predicates.Field<Db.EmailRecipientMeterEventPosition>(sc => sc.MeterEventId, Operator.Eq, me.Id);
      Assert.That(DbContext.Connection.Count<Db.EmailRecipientMeterEventPosition>(Predicates.Group(GroupOperator.And, pEr, pMe)), Is.EqualTo(1));
    }

    [Test]
    public void SetEmailRecipientMeterEventPositionUpdates()
    {
      // Arrange
      var target = CreateTarget();
      var er = new Db.EmailRecipient { Name = "N1", EmailAddress = "a@b.com" };
      DbContext.InsertTransaction(null, er);
      var me1 = new Db.MeterEvent { Label="lbl", MeterEventType="met", DetectTimestamp=DateTime.UtcNow, Amplification="Amp" };
      DbContext.InsertTransaction(null, me1);
      target.SetEmailRecipientMeterEventPosition(er.EmailAddress, me1.Id);

      var me2 = new Db.MeterEvent { Label="lbl", MeterEventType="met", DetectTimestamp=DateTime.UtcNow, Amplification="Amp" };
      DbContext.InsertTransaction(null, me2);

      // Act
      target.SetEmailRecipientMeterEventPosition(er.EmailAddress, me2.Id);

      // Assert
      var pEr = Predicates.Field<Db.EmailRecipientMeterEventPosition>(sc => sc.EmailRecipientId, Operator.Eq, er.Id);
      var pMe1 = Predicates.Field<Db.EmailRecipientMeterEventPosition>(sc => sc.MeterEventId, Operator.Eq, me1.Id);
      Assert.That(DbContext.Connection.Count<Db.EmailRecipientMeterEventPosition>(Predicates.Group(GroupOperator.And, pEr, pMe1)), Is.EqualTo(0));

      var pMe2 = Predicates.Field<Db.EmailRecipientMeterEventPosition>(sc => sc.MeterEventId, Operator.Eq, me2.Id);
      Assert.That(DbContext.Connection.Count<Db.EmailRecipientMeterEventPosition>(Predicates.Group(GroupOperator.And, pEr, pMe2)), Is.EqualTo(1));
    }

    private EmailRecipientRepository CreateTarget()
    {
      return new EmailRecipientRepository(DbContext);
    }

  }
}
