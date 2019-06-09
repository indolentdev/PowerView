using System;
using System.Collections.Generic;
using NUnit.Framework;
using DapperExtensions;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository
{
  [TestFixture]
  public class SettingRepositoryTest : DbTestFixtureWithSchema
  {
    [Test]
    public void UpsertThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Upsert(null, "Value"), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Upsert("", "Value"), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Upsert("Name", null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Upsert("Name", ""), Throws.TypeOf<ArgumentNullException>());

      Assert.That(() => target.Upsert(null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Upsert(new[] { new KeyValuePair<string, string>(null, "Value") }), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Upsert(new[] { new KeyValuePair<string, string>("", "Value") }), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Upsert(new[] { new KeyValuePair<string, string>("Name", null) }), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Upsert(new[] { new KeyValuePair<string, string>("Name", "") }), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void UpsertInserts()
    {
      // Arrange
      var target = CreateTarget();
      const string name = "theName";
      const string value = "theValue";

      // Act
      target.Upsert(name, value);

      // Assert
      var predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, name);
      var predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, value);
      var predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
    }

    [Test]
    public void UpsertUpdates()
    {
      // Arrange
      var target = CreateTarget();
      const string name = "theName";
      const string value = "theValue";
      DbContext.Connection.Insert(new Db.Setting {Name=name, Value="old Value"});

      // Act
      target.Upsert(name, value);

      // Assert
      var predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, name);
      var predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, value);
      var predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
    }

    [Test]
    public void UpsertListInserts()
    {
      // Arrange
      var target = CreateTarget();
      const string name1 = "theName1";
      const string value1 = "theValue1";
      const string name2 = "theName2";
      const string value2 = "theValue2";

      // Act
      target.Upsert(new [] { new KeyValuePair<string, string>(name1, value1), new KeyValuePair<string, string>(name2, value2) });

      // Assert
      var predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, name1);
      var predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, value1);
      var predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
      predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, name2);
      predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, value2);
      predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
    }

    [Test]
    public void UpsertListUpdates()
    {
      // Arrange
      var target = CreateTarget();
      const string name1 = "theName1";
      const string value1 = "theValue1";
      const string name2 = "theName2";
      const string value2 = "theValue2";
      DbContext.Connection.Insert(new Db.Setting { Name = name1, Value = "old Value1" });
      DbContext.Connection.Insert(new Db.Setting { Name = name2, Value = "old Value2" });

      // Act
      target.Upsert(new[] { new KeyValuePair<string, string>(name1, value1), new KeyValuePair<string, string>(name2, value2) });

      // Assert
      var predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, name1);
      var predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, value1);
      var predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
      predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, name2);
      predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, value2);
      predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
    }

    [Test]
    public void GetThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Get(null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Get(""), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Get()
    {
      // Arrange
      var target = CreateTarget();
      const string name = "theName";
      const string value = "theValue";
      DbContext.Connection.Insert(new Db.Setting {Name=name, Value=value});

      // Act
      var readValue = target.Get(name);

      // Assert
      Assert.That(readValue, Is.EqualTo(value));
    }

    [Test]
    public void FindThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Find(null), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Find(""), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Find()
    {
      // Arrange
      var target = CreateTarget();
      DbContext.Connection.Insert(new Db.Setting { Name = "A-name1", Value = "A-value1" });
      DbContext.Connection.Insert(new Db.Setting { Name = "A-name2", Value = "A-value2" });
      DbContext.Connection.Insert(new Db.Setting { Name = "B-name1", Value = "B-value1" });

      // Act
      var list = target.Find("A-");

      // Assert
      Assert.That(list.Count, Is.EqualTo(2));
      Assert.That(list, Contains.Item(new KeyValuePair<string, string>("A-name1", "A-value1")));
      Assert.That(list, Contains.Item(new KeyValuePair<string, string>("A-name2", "A-value2")));
    }

    [Test]
    public void ProvideInstallationId()
    {
      // Arrange
      var target = CreateTarget();

      // Act
      var id = target.ProvideInstallationId();

      // Assert
      Assert.That(id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void ProvideInstallationIdSuccessive()
    {
      // Arrange
      var target = CreateTarget();
      var id = target.ProvideInstallationId();

      // Act
      var id2 = target.ProvideInstallationId();

      // Assert
      Assert.That(id2, Is.EqualTo(id));
    }

    [Test]
    public void GetMqttConfig()
    {
      // Arrange
      DbContext.Connection.Insert(new Db.Setting { Name = "MQTT_Server", Value = "TheMqttServer" });
      DbContext.Connection.Insert(new Db.Setting { Name = "MQTT_Port", Value = "12345" });
      DbContext.Connection.Insert(new Db.Setting { Name = "MQTT_PublishEnabled", Value = "True" });
      var target = CreateTarget();

      // Act
      var mqttConfig = target.GetMqttConfig();

      // Assert
      Assert.That(mqttConfig.PublishEnabled, Is.True);
      Assert.That(mqttConfig.Server, Is.EqualTo("TheMqttServer"));
      Assert.That(mqttConfig.Port, Is.EqualTo(12345));
    }

    [Test]
    public void UpsertMqttConfigThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.UpsertMqttConfig(null), Throws.ArgumentNullException);
    }

    [Test]
    public void UpsertMqttConfigInsert()
    {
      // Arrange
      var mqttConfig = new MqttConfig("TheMqttServer", 12345, true);
      var target = CreateTarget();

      // Act
      target.UpsertMqttConfig(mqttConfig);

      // Assert
      AssertMqttConfig("TheMqttServer", "12345", "True");
    }

    [Test]
    public void UpsertMqttConfigUpdate()
    {
      // Arrange
      var target = CreateTarget();
      target.UpsertMqttConfig(new MqttConfig("Some server", 55555, false));
      var mqttConfig = new MqttConfig("TheMqttServer", 12345, true);

      // Act
      target.UpsertMqttConfig(mqttConfig);

      // Assert
      AssertMqttConfig("TheMqttServer", "12345", "True");
    }

    private void AssertMqttConfig(string server, string port, string enabled)
    {
      var predicatePrefix = Predicates.Field<Db.Setting>(s => s.Name, Operator.Like, "MQTT_%");
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicatePrefix), Is.EqualTo(3));

      var predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, "MQTT_Server");
      var predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, server);
      var predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
      predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, "MQTT_Port");
      predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, port);
      predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
      predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, "MQTT_PublishEnabled");
      predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, enabled);
      predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
    }

    [Test]
    public void UpsertSmtpConfigThrows()
    {
      // Arrange
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.UpsertSmtpConfig(null), Throws.ArgumentNullException);
    }

    [Test]
    public void UpsertSmtpConfigInsert()
    {
      // Arrange
      var smtpConfig = new SmtpConfig("TheSmtpServer", 12345, "TheUser", "TheAuth");
      var target = CreateTarget();

      // Act
      target.UpsertSmtpConfig(smtpConfig);

      // Assert
      AssertSmtpConfig("TheSmtpServer", "12345", "TheUser");
    }

    [Test]
    public void UpsertSmtpConfigUpdate()
    {
      // Arrange
      var target = CreateTarget();
      target.UpsertSmtpConfig(new SmtpConfig("SomeServer", 55555, "SomeUser", "SomeAuth"));
      var smtpConfig = new SmtpConfig("TheSmtpServer", 12345, "TheUser", "TheAuth");

      // Act
      target.UpsertSmtpConfig(smtpConfig);

      // Assert
      AssertSmtpConfig("TheSmtpServer", "12345", "TheUser");
    }

    private void AssertSmtpConfig(string server, string port, string user)
    {
      var predicatePrefix = Predicates.Field<Db.Setting>(s => s.Name, Operator.Like, "SMTP_%");
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicatePrefix), Is.EqualTo(5));

      var predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, "SMTP_Server");
      var predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, server);
      var predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
      predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, "SMTP_Port");
      predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, port);
      predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
      predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, "SMTP_User");
      predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, user);
      predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
      predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, "SMTP_AuthCrypt");
      predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, null, true);
      predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
      predicateName = Predicates.Field<Db.Setting>(s => s.Name, Operator.Eq, "SMTP_AuthIv");
      predicateValue = Predicates.Field<Db.Setting>(s => s.Value, Operator.Eq, null, true);
      predicate = Predicates.Group(GroupOperator.And, predicateName, predicateValue);
      Assert.That(DbContext.Connection.Count<Db.Setting>(predicate), Is.EqualTo(1));
    }

    [Test]
    public void GetSmtpConfig()
    {
      // Arrange
      var target = CreateTarget();
      target.UpsertSmtpConfig(new SmtpConfig("TheSmtpServer", 12345, "TheUser", "TheAuth"));

      // Act
      var smtpConfig = target.GetSmtpConfig();

      // Assert
      Assert.That(smtpConfig.Server, Is.EqualTo("TheSmtpServer"));
      Assert.That(smtpConfig.Port, Is.EqualTo(12345));
      Assert.That(smtpConfig.User, Is.EqualTo("TheUser"));
      Assert.That(smtpConfig.Auth, Is.EqualTo("TheAuth"));
    }

    private SettingRepository CreateTarget()
    {
      return new SettingRepository(DbContext);
    }

  }
}
