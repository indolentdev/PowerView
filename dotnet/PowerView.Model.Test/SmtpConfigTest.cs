using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class SmtpConfigTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            const string server = "theServer";
            const ushort port = 1234;
            const string user = "theUser";
            const string auth = "theAuth";
            const string email = "a@b.com";

            // Act & Asssert
            Assert.That(() => new SmtpConfig(null, port, user, auth, email), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Server"));
            Assert.That(() => new SmtpConfig(string.Empty, port, user, auth, email), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Server"));
            Assert.That(() => new SmtpConfig(server, 0, user, auth, email), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Port"));
            Assert.That(() => new SmtpConfig(server, port, null, auth, email), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("User"));
            Assert.That(() => new SmtpConfig(server, port, string.Empty, auth, email), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("User"));
            Assert.That(() => new SmtpConfig(server, port, user, null, email), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Auth"));
            Assert.That(() => new SmtpConfig(server, port, user, string.Empty, email), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Auth"));
            Assert.That(() => new SmtpConfig(server, port, user, auth, null), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Email"));
            Assert.That(() => new SmtpConfig(server, port, user, auth, string.Empty), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Email"));
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange

            // Act
            var target = new SmtpConfig("theServer", 1234, "theUser", "theAuth", "theEmail");

            // Assert
            Assert.That(target.Server, Is.EqualTo("theServer"));
            Assert.That(target.Port, Is.EqualTo(1234));
            Assert.That(target.User, Is.EqualTo("theUser"));
            Assert.That(target.Auth, Is.EqualTo("theAuth"));
            Assert.That(target.Email, Is.EqualTo("theEmail"));
        }

        [Test]
        public void ConstructorSettingsThrows()
        {
            // Arrange
            var ivString = DateTime.UtcNow.ToString("o");
            var settings = new Dictionary<string, string>
            {
              { "SMTP_Server", "theServer" },
              { "SMTP_Port", "587" },
              { "SMTP_User", "theUser" },
              { "SMTP_AuthCrypt", "Bad-crypt" },
              { "SMTP_AuthIv", ivString },
              { "SMTP_Email", "a@b.com" },
            };

            // Act & Assert
            Assert.That(() => new SmtpConfig(null), Throws.ArgumentNullException);
            Assert.That(() => new SmtpConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("SMTP_Server", "theServer") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Server"));
            Assert.That(() => new SmtpConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("SMTP_Port", "587") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Port"));
            Assert.That(() => new SmtpConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("SMTP_User", "theUser") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("User"));
            Assert.That(() => new SmtpConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("SMTP_AuthIv", ivString) }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Auth (iv)"));
            Assert.That(() => new SmtpConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("SMTP_AuthCrypt", "Bad-crypt") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Auth (crypt)"));
            Assert.That(() => new SmtpConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("SMTP_Email", "a@b.com") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Email"));

            var settings2 = new Dictionary<string, string>(settings);
            settings2["SMTP_AuthIv"] = "Bad-Iv";
            Assert.That(() => new SmtpConfig(settings2),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Auth iv"));
            Assert.That(() => new SmtpConfig(settings),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Auth decrypt"));
        }

        [Test]
        public void GetSettings()
        {
            // Arrange
            var target = new SmtpConfig("theServer", 1234, "theUser", "theAuth", "theEmail");

            // Act
            var settings = target.GetSettings();

            // Assert
            Assert.That(settings.Count, Is.EqualTo(6));
            Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("SMTP_Server", "theServer")));
            Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("SMTP_Port", "1234")));
            Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("SMTP_User", "theUser")));

            Assert.That(settings.FirstOrDefault(x => x.Key == "SMTP_AuthCrypt"), Is.Not.Null);
            Assert.That(settings.First(x => x.Key == "SMTP_AuthCrypt").Value, Is.Not.EqualTo("theAuth"));

            Assert.That(settings.FirstOrDefault(x => x.Key == "SMTP_AuthIv"), Is.Not.Null);
            Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("SMTP_Email", "theEmail")));
        }

        [Test]
        public void GetSettingsConstructorSettings()
        {
            // Arrange
            var target = new SmtpConfig("theServer", 1234, "theUser", "theAuth", "theEmail");

            // Act
            var settings = target.GetSettings();
            var target2 = new SmtpConfig(settings);

            // Assert
            Assert.That(target2.Server, Is.EqualTo(target.Server));
            Assert.That(target2.Port, Is.EqualTo(target.Port));
            Assert.That(target2.User, Is.EqualTo(target.User));
            Assert.That(target2.Auth, Is.EqualTo(target.Auth));
            Assert.That(target2.Email, Is.EqualTo(target.Email));
        }

        [Test]
        public void ToStringTest()
        {
            // Arrange
            var target = new SmtpConfig("theServer", 1234, "theUser", "theAuth", "theEmail");

            // Act
            var s = target.ToString();

            // Assert
            Assert.That(s, Contains.Substring("theServer"));
            Assert.That(s, Contains.Substring("1234"));
            Assert.That(s, Contains.Substring("theUser"));
            Assert.That(s, Does.Not.Contain("theAuth"));
            Assert.That(s, Contains.Substring("theEmail"));
        }

    }
}
