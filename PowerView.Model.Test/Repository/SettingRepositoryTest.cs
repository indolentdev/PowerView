using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using PowerView.Model.Repository;

namespace PowerView.Model.Test.Repository;

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
        AssertExists(name, value);
    }

    [Test]
    public void UpsertUpdates()
    {
        // Arrange
        var target = CreateTarget();
        const string name = "theName";
        const string value = "theValue";
        InsertSettings(new Db.Setting { Name = name, Value = "old Value" });

        // Act
        target.Upsert(name, value);

        // Assert
        AssertExists(name, value);
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
        target.Upsert(new[] { new KeyValuePair<string, string>(name1, value1), new KeyValuePair<string, string>(name2, value2) });

        // Assert
        AssertExists(name1, value1);
        AssertExists(name2, value2);
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
        InsertSettings(new Db.Setting { Name = name1, Value = "old Value1" }, new Db.Setting { Name = name2, Value = "old Value2" });

        // Act
        target.Upsert(new[] { new KeyValuePair<string, string>(name1, value1), new KeyValuePair<string, string>(name2, value2) });

        // Assert
        AssertExists(name1, value1);
        AssertExists(name2, value2);
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
        InsertSettings(new Db.Setting { Name = name, Value = value });

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
        var s1 = new Db.Setting { Name = "A-name1", Value = "A-value1" };
        var s2 = new Db.Setting { Name = "A-name2", Value = "A-value2" };
        var s3 = new Db.Setting { Name = "B-name1", Value = "B-value1" };
        InsertSettings(s1, s2, s3);

        // Act
        var list = target.Find("A-");

        // Assert
        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list, Contains.Item(new KeyValuePair<string, string>("A-name1", "A-value1")));
        Assert.That(list, Contains.Item(new KeyValuePair<string, string>("A-name2", "A-value2")));
    }

    [Test]
    public void GetMqttConfig()
    {
        // Arrange
        var s1 = new Db.Setting { Name = "MQTT_Server", Value = "TheMqttServer" };
        var s2 = new Db.Setting { Name = "MQTT_Port", Value = "12345" };
        var s3 = new Db.Setting { Name = "MQTT_PublishEnabled", Value = "True" };
        InsertSettings(s1, s2, s3);
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
        var mqttConfig = new MqttConfig("TheMqttServer", 12345, true, "TheClientId");
        var target = CreateTarget();

        // Act
        target.UpsertMqttConfig(mqttConfig);

        // Assert
        AssertMqttConfig("TheMqttServer", "12345", "True", "TheClientId");
    }

    [Test]
    public void UpsertMqttConfigUpdate()
    {
        // Arrange
        var target = CreateTarget();
        target.UpsertMqttConfig(new MqttConfig("Some server", 55555, false, "SomeClientId"));
        var mqttConfig = new MqttConfig("TheMqttServer", 12345, true, "TheClientId");

        // Act
        target.UpsertMqttConfig(mqttConfig);

        // Assert
        AssertMqttConfig("TheMqttServer", "12345", "True", "TheClientId");
    }

    private void AssertMqttConfig(string server, string port, string enabled, string clientId)
    {
        var mqttSettings = DbContext.QueryTransaction<Db.Setting>("SELECT * FROM Setting WHERE Name like @mqtt;", new { mqtt = "MQTT_%" });
        Assert.That(mqttSettings.Count, Is.EqualTo(4));

        AssertExists("MQTT_Server", server);
        AssertExists("MQTT_Port", port);
        AssertExists("MQTT_PublishEnabled", enabled);
        AssertExists("MQTT_ClientId", clientId);
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
        var smtpConfig = new SmtpConfig("TheSmtpServer", 12345, "TheUser", "TheAuth", "TheEmail");
        var target = CreateTarget();

        // Act
        target.UpsertSmtpConfig(smtpConfig);

        // Assert
        AssertSmtpConfig("TheSmtpServer", "12345", "TheUser", "TheEmail");
    }

    [Test]
    public void UpsertSmtpConfigUpdate()
    {
        // Arrange
        var target = CreateTarget();
        target.UpsertSmtpConfig(new SmtpConfig("SomeServer", 55555, "SomeUser", "SomeAuth", "SomeEmail"));
        var smtpConfig = new SmtpConfig("TheSmtpServer", 12345, "TheUser", "TheAuth", "TheEmail");

        // Act
        target.UpsertSmtpConfig(smtpConfig);

        // Assert
        AssertSmtpConfig("TheSmtpServer", "12345", "TheUser", "TheEmail");
    }

    private void AssertSmtpConfig(string server, string port, string user, string email)
    {
        var smtpSettings = DbContext.QueryTransaction<Db.Setting>("SELECT * FROM Setting WHERE Name like @smtp;", new { smtp = "SMTP_%" });
        Assert.That(smtpSettings.Count, Is.EqualTo(6));

        AssertExists("SMTP_Server", server);
        AssertExists("SMTP_Port", port);
        AssertExists("SMTP_User", user);

        var smtpAuthSettings = smtpSettings.Where(x => x.Name.StartsWith("SMTP_Auth", StringComparison.InvariantCultureIgnoreCase)).ToList();
        Assert.That(smtpAuthSettings.Count, Is.EqualTo(2));
        Assert.That(smtpAuthSettings.Select(x => x.Name).ToArray(), Is.EquivalentTo(new[] { "SMTP_AuthCrypt", "SMTP_AuthIv" }));
        Assert.That(smtpAuthSettings[0].Value, Is.Not.Null);
        Assert.That(smtpAuthSettings[1].Value, Is.Not.Null);

        AssertExists("SMTP_Email", email);
    }

    [Test]
    public void GetSmtpConfig()
    {
        // Arrange
        var target = CreateTarget();
        target.UpsertSmtpConfig(new SmtpConfig("TheSmtpServer", 12345, "TheUser", "TheAuth", "TheEmail"));

        // Act
        var smtpConfig = target.GetSmtpConfig();

        // Assert
        Assert.That(smtpConfig.Server, Is.EqualTo("TheSmtpServer"));
        Assert.That(smtpConfig.Port, Is.EqualTo(12345));
        Assert.That(smtpConfig.User, Is.EqualTo("TheUser"));
        Assert.That(smtpConfig.Auth, Is.EqualTo("TheAuth"));
        Assert.That(smtpConfig.Email, Is.EqualTo("TheEmail"));
    }

    private SettingRepository CreateTarget()
    {
        return new SettingRepository(DbContext);
    }

    private void InsertSettings(params Db.Setting[] settings)
    {
        DbContext.ExecuteTransaction("INSERT INTO Setting (Name, Value) VALUES (@Name, @Value);", settings);
    }

    private void AssertExists(string name, string value)
    {
        var settings = DbContext.QueryTransaction<Db.Setting>("SELECT * FROM Setting WHERE Name=@name and Value=@value;", new { name, value });
        Assert.That(settings.Count, Is.EqualTo(1));
    }

}
