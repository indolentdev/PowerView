using System;
using System.Linq;
using System.Globalization;
using MailKit.Net.Smtp;
using MailKit.Security;
using MailKit;
using MimeKit;
using PowerView.Model;
using Microsoft.Extensions.Logging;

namespace PowerView.Service.Mailer
{
    public class SmtpMailer : IMailer
    {
        private readonly ILogger logger;

        public SmtpMailer(ILogger<SmtpMailer> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public EmailRecipient Send(SmtpConfig smtpConfig, EmailRecipient emailRecipient, string subject, string message)
        {
            if (smtpConfig == null) throw new ArgumentNullException("smtpConfig");
            if (emailRecipient == null) throw new ArgumentNullException("emailRecipient");

            var toMailbox = ToMailbox(emailRecipient.Name, emailRecipient.EmailAddress);

            using (var client = new SmtpClient())
            {
                client.Timeout = 10 * 1000;
                client.SslProtocols = System.Security.Authentication.SslProtocols.None;
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None) return true;

                    var chainDigest = new System.Text.StringBuilder();
                    foreach (var element in chain.ChainElements)
                    {
                        if (chainDigest.Length > 0) chainDigest.Append(", ");
                        chainDigest.Append(element.Certificate.Subject).Append("(");
                        chainDigest.Append(string.Join(",", element.ChainElementStatus.Select(x => $"{x.StatusInformation}[{x.Status}]")));
                        chainDigest.Append(")");
                    }

                    if (certificate.Subject.Contains("CN=" + smtpConfig.Server.ToLowerInvariant()))
                    {
                        logger.LogDebug("Accepting smtp server certificate with subject '{0}' and policy error(s):{1}. Chain digest:{2}",
                               certificate.Subject, sslPolicyErrors, chainDigest);
                        return true;
                    }

                    logger.LogError("Could not validate smtp mail server certificate. Certificate Subject:{0}, Error(s):{1}. Chain digest:{2}",
                            certificate, sslPolicyErrors, chainDigest);
                    return false;
                };

                Connect(smtpConfig, client);

                Authenticate(smtpConfig, client);

                var mimeMessage = GetMessage(smtpConfig, subject, message, toMailbox);
                Send(smtpConfig, client, mimeMessage);

                client.Disconnect(true);

                var fromAddr = (MailboxAddress)mimeMessage.From.First();
                return new EmailRecipient(fromAddr.Name, fromAddr.Address);
            }
        }

        private static MailboxAddress ToMailbox(string name, string address)
        {
            try
            {
                return new MailboxAddress(name, address);
            }
            catch (ParseException e)
            {
                throw new MailerException("Unable to parse address:" + address, e);
            }
        }

        private MimeMessage GetMessage(SmtpConfig smtpConfig, string subject, string message, MailboxAddress toMailbox)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("PowerView", smtpConfig.Email));
            mimeMessage.To.Add(toMailbox);
            mimeMessage.ReplyTo.Add(toMailbox);
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart("plain") { Text = message };
            return mimeMessage;
        }

        private void Connect(SmtpConfig smtpConfig, SmtpClient client)
        {
            Connect(smtpConfig, client, SecureSocketOptions.Auto);
        }

        private void Connect(SmtpConfig smtpConfig, SmtpClient client, SecureSocketOptions secureSocketOptions)
        {
            logger.LogDebug($"Connecting smtp client. {smtpConfig.Server}:{smtpConfig.Port}. {secureSocketOptions}");

            try
            {
                client.Connect(smtpConfig.Server, smtpConfig.Port, secureSocketOptions);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Failed connecting to the smtp server. Address:{0}, Port:{1}, SecureSocketOptions:{2}", smtpConfig.Server, smtpConfig.Port, secureSocketOptions);
                throw new ConnectMailerException(msg, e);
            }
            catch (NotSupportedException e)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Failed establising secure session with smtp server. Address:{0}, Port:{1}, SecureSocketOptions:{2}", smtpConfig.Server, smtpConfig.Port, secureSocketOptions);
                throw new ConnectMailerException(msg, e);
            }
            catch (MailKit.Security.SslHandshakeException e)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Certificate authentication failed for smtp server. Address:{0}, Port:{1}, SecureSocketOptions:{2}", smtpConfig.Server, smtpConfig.Port, secureSocketOptions);
                throw new ConnectMailerException(msg, e);
            }

            if (!client.IsSecure || !client.IsSigned || !client.IsEncrypted)
            {
                throw new ConnectMailerException($"Failed establising secure session with smtp server. Address:{smtpConfig.Server}, Port:{smtpConfig.Port}, IsSecure:{client.IsSecure}, IsSigned:{client.IsSigned}, IsEncrypted:{client.IsEncrypted}");
            }
        }

        private void Authenticate(SmtpConfig smtpConfig, SmtpClient client)
        {
            var authMechsRemove = client.AuthenticationMechanisms.Where(x =>
                                    !string.Equals(x, "PLAIN", StringComparison.OrdinalIgnoreCase) && !string.Equals(x, "LOGIN", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var authMecRemove in authMechsRemove)
            {
                client.AuthenticationMechanisms.Remove(authMecRemove);
            }

            try
            {
                logger.LogDebug("Authenticating smtp client");
                client.Authenticate(smtpConfig.User, smtpConfig.Auth);
            }
            catch (AuthenticationException e)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Smtp server rejected user authentication. {0}", smtpConfig);
                throw new AuthenticateMailerException(msg, e);
            }
            catch (NotSupportedException e)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Smtp server failed authenticating. {0}", smtpConfig);
                throw new AuthenticateMailerException(msg, e);
            }
            catch (SmtpProtocolException e)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Smtp server failed authenticating. {0}", smtpConfig);
                throw new AuthenticateMailerException(msg, e);
            }
        }

        private void Send(SmtpConfig smtpConfig, SmtpClient client, MimeMessage mimeMessage)
        {
            try
            {
                logger.LogDebug("Smtp client sending message");
                client.Send(mimeMessage);
            }
            catch (CommandException e)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Smtp server rejected message. {0}", smtpConfig);
                throw new MailerException(msg, e);
            }
            catch (ProtocolException e)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Smtp server error. {0}", smtpConfig);
                throw new MailerException(msg, e);
            }
        }

    }
}
