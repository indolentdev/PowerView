using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;

namespace PowerView.Model.Repository;
internal class SettingRepository : RepositoryBase, ISettingRepository
{
    public SettingRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public void Upsert(string name, string value)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

        Upsert(new[] { new KeyValuePair<string, string>(name, value) });
    }

    public void Upsert(ICollection<KeyValuePair<string, string>> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (items.Any(x => string.IsNullOrEmpty(x.Key) || string.IsNullOrEmpty(x.Value))) throw new ArgumentNullException(nameof(items));

        DbContext.ExecuteTransaction("INSERT INTO Setting (Name, Value) VALUES (@Key, @Value) ON CONFLICT (Name) DO UPDATE SET Value = excluded.Value;", items);
    }

    public string Get(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        return DbContext.QueryTransaction<string>("SELECT Value FROM Setting WHERE Name = @name;", new { name }).FirstOrDefault();
    }

    public IList<KeyValuePair<string, string>> Find(string startsWith)
    {
        if (string.IsNullOrEmpty(startsWith)) throw new ArgumentNullException(nameof(startsWith));

        return DbContext
          .QueryTransaction<Db.Setting>("SELECT Id, Name, Value FROM Setting WHERE Name LIKE @StartsWith;", new { StartsWith = startsWith + "%" })
          .Select(x => new KeyValuePair<string, string>(x.Name, x.Value))
          .ToList();
    }

    public MqttConfig GetMqttConfig()
    {
        return new MqttConfig(Find(MqttConfig.MqttPrefix));
    }

    public void UpsertMqttConfig(MqttConfig mqttConfig)
    {
        if (mqttConfig == null) throw new ArgumentNullException(nameof(mqttConfig));

        Upsert(mqttConfig.GetSettings());
    }

    public SmtpConfig GetSmtpConfig()
    {
        return new SmtpConfig(Find(SmtpConfig.SmtpPrefix));
    }

    public void UpsertSmtpConfig(SmtpConfig smtpConfig)
    {
        if (smtpConfig == null) throw new ArgumentNullException(nameof(smtpConfig));

        Upsert(smtpConfig.GetSettings());
    }

    public EnergiDataServiceImportConfig GetEnergiDataServiceImportConfig()
    {
        return new EnergiDataServiceImportConfig(Find(EnergiDataServiceImportConfig.SettingPrefix));
    }

    public void UpsertEnergiDataServiceImportConfig(EnergiDataServiceImportConfig edsiConfig)
    {
        if (edsiConfig == null) throw new ArgumentNullException(nameof(edsiConfig));

        Upsert(edsiConfig.GetSettings());
    }

    private const string edsiPositionName = "EnergiDataServiceImportPosition";
    public DateTime? GetEnergiDataServiceImportPosition()
    {
        var positionString = Get(edsiPositionName);

        if (string.IsNullOrEmpty(positionString)) return null;

        return DateTime.Parse(positionString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    public void UpsertEnergiDataServiceImportPosition(DateTime position)
    {
        var positionString = position.ToString("o");
        Upsert(edsiPositionName, positionString);
    }

}
