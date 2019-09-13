using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerView.Model.Repository
{
  internal class EmailRecipientRepository : RepositoryBase, IEmailRecipientRepository
  {
    //    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public EmailRecipientRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public EmailRecipient GetEmailRecipient(string emailAddress)
    {
      return DbContext.QueryTransaction<Db.EmailRecipient>("GetEmailRecipient",
          "SELECT Id, Name, EmailAddress FROM EmailRecipient WHERE EmailAddress = @emailAddress;", new { emailAddress })
        .Select(ToEmailRecipient)
        .FirstOrDefault();
    }

    public IList<EmailRecipient> GetEmailRecipients()
    {
      return DbContext
        .QueryTransaction<Db.EmailRecipient>("GetEmailRecipients", "SELECT Id, Name, EmailAddress FROM EmailRecipient ORDER BY Id LIMIT 50;")
        .Select(ToEmailRecipient)
        .ToList();
    }

    private EmailRecipient ToEmailRecipient(Db.EmailRecipient dbEmailRecipient)
    {
      return ToEmailRecipient(dbEmailRecipient.Name, dbEmailRecipient.EmailAddress);
    }

    private EmailRecipient ToEmailRecipient(string name, string emailAddress)
    {
      return new EmailRecipient(name, emailAddress);
    }

    public void AddEmailRecipient(EmailRecipient emailRecipient)
    {
      if (emailRecipient == null) throw new ArgumentNullException("emailRecipient");

      const string sql = @"
      INSERT INTO [EmailRecipient] ([Name],[EmailAddress]) VALUES (@name, @emailAddress);

      INSERT INTO [EmailRecipientMeterEventPosition] ([EmailRecipientId],[MeterEventId]) 
      SELECT last_insert_rowid() AS [EmailRecipientId], [Id] AS [MeterEventId] FROM [MeterEvent] ORDER By [Id] DESC LIMIT 1;";

      DbContext.ExecuteTransaction("AddEmailRecipient", sql, 
                                   new { name = emailRecipient.Name, emailAddress = emailRecipient.EmailAddress });
    }

    public void DeleteEmailRecipient(string emailAddress)
    {
      const string sql = @"
      DELETE FROM [EmailRecipientMeterEventPosition] WHERE [EmailRecipientId] IN (SELECT [Id] FROM [EmailRecipient] WHERE [EmailAddress]=@emailAddress);
      DELETE FROM [EmailRecipient] WHERE [EmailAddress]=@emailAddress;";
      DbContext.ExecuteTransaction("DeleteEmailRecipient", sql, new { emailAddress });
    }

    public IDictionary<EmailRecipient, long?> GetEmailRecipientsMeterEventPosition()
    {
      const string sql = @"
      SELECT [Name],[EmailAddress],[MeterEventId]
      FROM [EmailRecipient] 
      LEFT JOIN [EmailRecipientMeterEventPosition] ON [EmailRecipientId]=[EmailRecipient].[Id];";

      var rows = DbContext.QueryTransaction<dynamic>("GetEmailRecipientsMeterEventPosition", sql);
      var result = new Dictionary<EmailRecipient, long?>(rows.Count);
      foreach (dynamic row in rows)
      {
        var emailRecipient = ToEmailRecipient(row.Name, row.EmailAddress);
        long? meterEventId = row.MeterEventId;
        result.Add(emailRecipient, meterEventId);
      }
      return result;
    }

    public void SetEmailRecipientMeterEventPosition(string emailAddress, long meterEventId)
    {
      const string sql = @"
      UPDATE [EmailRecipientMeterEventPosition] 
      SET [MeterEventId]=@meterEventId 
      WHERE [EmailRecipientId] IN (SELECT [Id] FROM [EmailRecipient] WHERE [EmailAddress]=@emailAddress);

      INSERT INTO [EmailRecipientMeterEventPosition] ([EmailRecipientId],[MeterEventId]) 
      SELECT [Id] AS [EmailRecipientId], @meterEventId AS [MeterEventId] FROM [EmailRecipient] WHERE [EmailAddress]=@emailAddress AND changes() = 0;";

      DbContext.ExecuteTransaction("SetEmailRecipientMeterEventPosition", sql, new { emailAddress, meterEventId });
    }

  }
}
