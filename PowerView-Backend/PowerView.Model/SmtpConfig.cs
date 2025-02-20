using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PowerView.Model
{
    public class SmtpConfig
    {
        internal const string SmtpPrefix = "SMTP_";
        private const string SmtpServer = SmtpPrefix + "Server";
        private const string SmtpPort = SmtpPrefix + "Port";
        private const string SmtpUser = SmtpPrefix + "User";
        private const string SmtpAuthCrypt = SmtpPrefix + "AuthCrypt";
        private const string SmtpAuthIv = SmtpPrefix + "AuthIv";
        private const string SmtpEmail = SmtpPrefix + "Email";

        public SmtpConfig(string server, ushort port, string user, string auth, string email)
        {
            if (string.IsNullOrEmpty(server)) throw new DomainConstraintException("Server is required");
            if (port < 1) throw new DomainConstraintException("Port is required");
            if (string.IsNullOrEmpty(user)) throw new DomainConstraintException("User is required");
            if (string.IsNullOrEmpty(auth)) throw new DomainConstraintException("Auth is required");
            if (string.IsNullOrEmpty(email)) throw new DomainConstraintException("Email is required");

            Server = server;
            Port = port;
            User = user;
            Auth = auth;
            Email = email;
        }

        internal SmtpConfig(ICollection<KeyValuePair<string, string>> smtpSettings)
        {
            if (smtpSettings == null) throw new ArgumentNullException("smtpSettings");

            var serverString = GetValue(smtpSettings, SmtpServer);
            var portString = GetValue(smtpSettings, SmtpPort);
            var userString = GetValue(smtpSettings, SmtpUser);
            var authCryptString = GetValue(smtpSettings, SmtpAuthCrypt);
            var authIvString = GetValue(smtpSettings, SmtpAuthIv);
            var emailString = GetValue(smtpSettings, SmtpEmail);

            if (string.IsNullOrEmpty(serverString)) throw new DomainConstraintException("Server is required");
            if (string.IsNullOrEmpty(portString)) throw new DomainConstraintException("Port is required");
            if (string.IsNullOrEmpty(userString)) throw new DomainConstraintException("User is required");
            if (string.IsNullOrEmpty(authCryptString)) throw new DomainConstraintException("Auth (crypt) is required");
            if (string.IsNullOrEmpty(authIvString)) throw new DomainConstraintException("Auth (iv) is required");
            if (string.IsNullOrEmpty(emailString)) throw new DomainConstraintException("Email is required");

            Server = serverString;

            ushort port;
            if (!ushort.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out port))
            {
                throw new DomainConstraintException("Port must be a positive number. Was:" + portString);
            }
            Port = port;

            User = userString;

            DateTime ivDateTime;
            if (!DateTime.TryParse(authIvString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out ivDateTime))
            {
                throw new DomainConstraintException("Auth iv must be a date time (roundtrip). Was:" + authIvString);
            }
            var boxCrypto = new BoxCryptor();
            try
            {
                Auth = boxCrypto.Decrypt(authCryptString, ivDateTime);
            }
            catch (BoxCryptorException e)
            {
                throw new DomainConstraintException("Auth decrypt failed. IV:" + authIvString, e);
            }

            Email = emailString;
        }

        private static string GetValue(ICollection<KeyValuePair<string, string>> smtpSettings, string key)
        {
            return smtpSettings.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value;
        }

        public string Server { get; private set; }
        public ushort Port { get; private set; }
        public string User { get; private set; }
        public string Auth { get; private set; }
        public string Email { get; private set; }

        internal ICollection<KeyValuePair<string, string>> GetSettings()
        {
            var boxCrypto = new BoxCryptor();

            var ivDateTime = DateTime.UtcNow;
            var settings = new List<KeyValuePair<string, string>>
            {
              new KeyValuePair<string, string>(SmtpServer, Server),
              new KeyValuePair<string, string>(SmtpPort, Port.ToString(CultureInfo.InvariantCulture)),
              new KeyValuePair<string, string>(SmtpUser, User),
              new KeyValuePair<string, string>(SmtpAuthCrypt, boxCrypto.Encrypt(Auth, ivDateTime)),
              new KeyValuePair<string, string>(SmtpAuthIv, ivDateTime.ToString("o")),
              new KeyValuePair<string, string>(SmtpEmail, Email),
            };
            return settings;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "SmtpConfig [Server:{0}, Port:{1}, User:{2}, Email:{3}]", Server, Port, User, Email);
        }

    }
}
