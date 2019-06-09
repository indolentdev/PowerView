using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using DapperExtensions;

namespace PowerView.Model.Repository
{
  internal class SettingRepository : RepositoryBase, ISettingRepository
  {
    //    private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public SettingRepository(IDbContext dbContext)
      : base(dbContext)
    {
    }

    public void Upsert(string name, string value)
    {
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
      if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");

      Upsert(new[] { new KeyValuePair<string, string>(name, value) });
    }

    public void Upsert(ICollection<KeyValuePair<string, string>> items)
    {
      if (items == null) throw new ArgumentNullException("items");
      if (items.Any(x => string.IsNullOrEmpty(x.Key) || string.IsNullOrEmpty(x.Value))) throw new ArgumentNullException("items");

      var transaction = DbContext.BeginTransaction();
      try
      {
        foreach (var item in items)
        {
          
          var predicate = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, item.Key);
          var setting = DbContext.Connection.GetList<Db.Setting>(predicate, null, transaction).SingleOrDefault();
          if (setting != null)
          {
            setting.Value = item.Value;
            DbContext.Connection.Update(setting, transaction);
          }
          else
          {
            setting = new Db.Setting { Name = item.Key, Value = item.Value };
            DbContext.Connection.Insert(setting, transaction);
          }
        }
        transaction.Commit();
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public string Get(string name)
    {
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

      var transaction = DbContext.BeginTransaction();
      try
      {
        var predicate = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, name);
        var setting = DbContext.Connection.GetList<Db.Setting>(predicate, null, transaction).SingleOrDefault();
        transaction.Commit();
        return setting == null ? null : setting.Value;
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public IList<KeyValuePair<string, string>> Find(string startsWith)
    {
      if (string.IsNullOrEmpty(startsWith)) throw new ArgumentNullException("startsWith");

      var transaction = DbContext.BeginTransaction();
      try
      {
        var predicate = Predicates.Field<Db.Setting>(s => s.Name, Operator.Like, startsWith + "%");
        var settings = DbContext.Connection.GetList<Db.Setting>(predicate, null, transaction).ToList();
        transaction.Commit();
        return new List<KeyValuePair<string, string>>(settings.Select(x => new KeyValuePair<string, string>(x.Name, x.Value)));
      }
      catch (SqliteException e)
      {
        transaction.Rollback();
        throw DataStoreExceptionFactory.Create(e);
      }
    }

    public string ProvideInstallationId()
    {
      const string name = "InstallationId";
      var idString = Get(name);
      if (!string.IsNullOrEmpty(idString))
      {
        return idString;
      }

      var id = Guid.NewGuid();
      idString = id.ToString("B", System.Globalization.CultureInfo.InvariantCulture);
      Upsert(name, idString);

      return idString;
    }

    public MqttConfig GetMqttConfig()
    {
      return new MqttConfig(Find("MQTT_"));
    }

    public void UpsertMqttConfig(MqttConfig mqttConfig)
    {
      if (mqttConfig == null) throw new ArgumentNullException("mqttConfig");

      Upsert(mqttConfig.GetSettings());
    }

    public SmtpConfig GetSmtpConfig()
    {
      return new SmtpConfig(Find("SMTP_"));
    }

    public void UpsertSmtpConfig(SmtpConfig smtpConfig)
    {
      if (smtpConfig == null) throw new ArgumentNullException("smtpConfig");

      Upsert(smtpConfig.GetSettings());
    }

  }
}
