using System;
using System.Linq;
using NUnit.Framework;
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
      InsertEmailRecipients(er1, er2);

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
      InsertEmailRecipients(er1, er2);

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
      var emailRecipeintMeterEventPositions = DbContext.QueryTransaction<Db.EmailRecipientMeterEventPosition>("",
        "SELECT * FROM EmailRecipientMeterEventPosition;");
      Assert.That(emailRecipeintMeterEventPositions.Count, Is.EqualTo(0));
    }

    [Test]
    public void AddEmailRecipientMeterEventExists()
    {
      // Arrange
      var target = CreateTarget();
      var emailRecipient = new EmailRecipient("TheName", "a@b.com");
      var dbMeterEvent = new Db.MeterEvent { Label = "ehh", MeterEventType = "ohh", Amplification = "ihh", Flag = true };
      InsertMeterEvents(dbMeterEvent);

      // Act
      target.AddEmailRecipient(emailRecipient);

      // Assert
      AssertEmailRecipientExists(emailRecipient);
      var erId = DbContext.QueryTransaction<long>("SELECT [Id] FROM [EmailRecipient] WHERE [EmailAddress]=@emailAddress;", 
                                                    new { emailAddress = emailRecipient.EmailAddress }).First();
      var emailRecipeintMeterEventPositions = DbContext.QueryTransaction<Db.EmailRecipientMeterEventPosition>(
        "SELECT * FROM EmailRecipientMeterEventPosition WHERE EmailRecipientId=@erId AND MeterEventId=@Id;", new { erId, dbMeterEvent.Id });
      Assert.That(emailRecipeintMeterEventPositions.Count, Is.EqualTo(1));
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
      var emailRecipients = DbContext.QueryTransaction<Db.EmailRecipient>(
        "SELECT * FROM EmailRecipient WHERE Name=@Name AND EmailAddress=@EmailAddress;", emailRecipient);
      Assert.That(emailRecipients.Count, Is.EqualTo(not ? 0 : 1));
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
      var dbEmailRecipient = new Db.EmailRecipient { Name = "TheName", EmailAddress = "a@b.com " };
      InsertEmailRecipients(dbEmailRecipient);
      var dbMeterEvent = new Db.MeterEvent { Label = "ehh", MeterEventType = "ohh", Amplification = "ihh", Flag = true };
      InsertMeterEvents(dbMeterEvent);
      var dbPosition = new Db.EmailRecipientMeterEventPosition { EmailRecipientId = dbEmailRecipient.Id, MeterEventId = dbMeterEvent.Id };
      InsertEmailRecipientMeterEventPosition(dbPosition);

      // Act
      target.DeleteEmailRecipient(dbEmailRecipient.EmailAddress);

      // Assert
      var emailRecipients = DbContext.QueryTransaction<Db.EmailRecipient>("SELECT * FROM EmailRecipient WHERE EmailAddress=@EmailAddress", dbEmailRecipient);
      Assert.That(emailRecipients.Count, Is.Zero);
      var positions = DbContext.QueryTransaction<Db.EmailRecipientMeterEventPosition>("SELECT * FROM EmailRecipientMeterEventPosition;");
      Assert.That(positions.Count, Is.Zero);
    }

    [Test]
    public void GetEmailRecipientsMeterEventPositionWithoutPosition()
    {
      // Arrange
      var target = CreateTarget();
      var er1 = new Db.EmailRecipient { Name = "N1", EmailAddress = "a@b.com" };
      InsertEmailRecipients(er1);

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
      InsertEmailRecipients(er);
      var me = new Db.MeterEvent { Label="lbl", MeterEventType="met", DetectTimestamp=DateTime.UtcNow, Amplification="Amp" };
      InsertMeterEvents(me);
      var pos = new Db.EmailRecipientMeterEventPosition { EmailRecipientId = er.Id, MeterEventId = me.Id };
      InsertEmailRecipientMeterEventPosition(pos);

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
      InsertEmailRecipients(er);
      var me = new Db.MeterEvent { Label="lbl", MeterEventType="met", DetectTimestamp=DateTime.UtcNow, Amplification="Amp" };
      InsertMeterEvents(me);

      // Act
      target.SetEmailRecipientMeterEventPosition(er.EmailAddress, me.Id);

      // Assert
      var positions = DbContext.QueryTransaction<Db.EmailRecipientMeterEventPosition>(
        "SELECT * FROM EmailRecipientMeterEventPosition WHERE EmailRecipientId=@EmailRecipientId AND MeterEventId=@MeterEventId;", 
        new { EmailRecipientId = er.Id, MeterEventId = me.Id });
      Assert.That(positions.Count, Is.EqualTo(1));
    }

    [Test]
    public void SetEmailRecipientMeterEventPositionUpdates()
    {
      // Arrange
      var target = CreateTarget();
      var er = new Db.EmailRecipient { Name = "N1", EmailAddress = "a@b.com" };
      InsertEmailRecipients(er);
      var me1 = new Db.MeterEvent { Label="lbl", MeterEventType="met", DetectTimestamp=DateTime.UtcNow, Amplification="Amp" };
      InsertMeterEvents(me1);
      target.SetEmailRecipientMeterEventPosition(er.EmailAddress, me1.Id);

      var me2 = new Db.MeterEvent { Label="lbl", MeterEventType="met", DetectTimestamp=DateTime.UtcNow, Amplification="Amp" };
      InsertMeterEvents(me2);

      // Act
      target.SetEmailRecipientMeterEventPosition(er.EmailAddress, me2.Id);

      // Assert
      var positions = DbContext.QueryTransaction<Db.EmailRecipientMeterEventPosition>(
        "SELECT * FROM EmailRecipientMeterEventPosition WHERE EmailRecipientId=@EmailRecipientId AND MeterEventId=@MeterEventId;",
        new { EmailRecipientId = er.Id, MeterEventId = me1.Id });
      Assert.That(positions.Count, Is.EqualTo(0));

      positions = DbContext.QueryTransaction<Db.EmailRecipientMeterEventPosition>(
        "SELECT * FROM EmailRecipientMeterEventPosition WHERE EmailRecipientId=@EmailRecipientId AND MeterEventId=@MeterEventId;",
        new { EmailRecipientId = er.Id, MeterEventId = me2.Id });
      Assert.That(positions.Count, Is.EqualTo(1));
    }

    private EmailRecipientRepository CreateTarget()
    {
      return new EmailRecipientRepository(DbContext);
    }

    private void InsertEmailRecipients(params Db.EmailRecipient[] emailRecipients)
    {
      foreach (var emailRecipient in emailRecipients)
      {
        var id = DbContext.QueryTransaction<long>(
          "INSERT INTO EmailRecipient (Name,EmailAddress) VALUES (@Name,@EmailAddress); SELECT last_insert_rowid();",
          emailRecipient).First();
        emailRecipient.Id = id;
      }
    }

    private void InsertMeterEvents(params Db.MeterEvent[] meterEvents)
    {
      foreach (var meterEvent in meterEvents)
      {
        var id = DbContext.QueryTransaction<long>(
          "INSERT INTO MeterEvent (Label,MeterEventType,DetectTimestamp,Flag,Amplification) VALUES (@Label,@MeterEventType,@DetectTimestamp,@Flag,@Amplification); SELECT last_insert_rowid();",
          meterEvent).First();
        meterEvent.Id = id;
      }
    }

    private void InsertEmailRecipientMeterEventPosition(params Db.EmailRecipientMeterEventPosition[] positions)
    {
      DbContext.ExecuteTransaction("INSERT INTO EmailRecipientMeterEventPosition (EmailRecipientId,MeterEventId) VALUES (@EmailRecipientId,@MeterEventId);", positions);
    }

  }
}
