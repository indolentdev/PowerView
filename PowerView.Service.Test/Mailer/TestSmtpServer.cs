using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using NUnit.Framework;
using SmtpServer;     // https://github.com/cosullivan/SmtpServer
using SmtpServer.Authentication;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace PowerView.Service.Test.Mailer
{
  internal class TestSmtpServer : IDisposable
  {
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private Task smtpServerTask;
    private SmtpLogger smtpLogger = new SmtpLogger();
    private MemoryMessageStore messageStore;
    private SmtpResponse nextSmtpResponse;

    public readonly string ServerName = "localhost";
    public int Port { get; private set; }
    public IList<MimeMessage> Messages { get { return messageStore.Messages; } }

    public TestSmtpServer()
    {
      Port = FreeTcpPort();
      messageStore = new MemoryMessageStore(() => nextSmtpResponse);
      nextSmtpResponse = SmtpResponse.Ok;
    }

    private static int FreeTcpPort()
    {
      TcpListener l = new TcpListener(IPAddress.Loopback, 0);
      l.Start();
      int port = ((IPEndPoint)l.LocalEndpoint).Port;
      l.Stop();
      return port;
    }

    public string GetSmtpLog()
    {
      return smtpLogger.ToString();
    }

    public void SetNextSendSmtpResponse(SmtpResponse smtpResponse)
    {
      nextSmtpResponse = smtpResponse;
    }

    public Action<SmtpServerOptionsBuilder> EnableTls(string siteName)
    {
      Action<SmtpServerOptionsBuilder> action = ob => {
        ob.SupportedSslProtocols(System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12);

        var password = "123456789";
        using (var certificateGenerator = new CertificateGenerator())
        {
          var pfxCertificateBytes = certificateGenerator.GenerateCertificateForPersonalFileExchange(siteName, password);

          var certificate = new X509Certificate2(pfxCertificateBytes, password);
          ob.Certificate(certificate);
        }
      };

      return action;
    }

    public Action<SmtpServerOptionsBuilder> EnableUser(string user, string password)
    {
      Action<SmtpServerOptionsBuilder> action = ob =>
      {
        var userAuthenticator = new UserAuthenticator(user, password);
        ob.UserAuthenticator(userAuthenticator);
      };

      return action;
    }

    public Action<SmtpServerOptionsBuilder> VoidUserAuthenticator()
    {
      Action<SmtpServerOptionsBuilder> action = ob =>
      {
        var userAuthenticator = new VoidUserAuthenticator();
        ob.UserAuthenticator(userAuthenticator);
      };

      return action;
    }

    public void Start(params Action<SmtpServerOptionsBuilder>[] optionsActions)
    {
      var optionsBuilder = new SmtpServerOptionsBuilder().ServerName(ServerName).Port(Port);
      optionsBuilder.MessageStore(messageStore).MaxRetryCount(1);
      optionsBuilder.Logger(smtpLogger);
      foreach (var optionsAction in optionsActions)
      {
        optionsAction(optionsBuilder);
      }
      var options = optionsBuilder.Build();

      var smtpServer = new SmtpServer.SmtpServer(options);
      smtpServerTask = smtpServer.StartAsync(cancellationTokenSource.Token);
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          cancellationTokenSource.Cancel(true);

          if (smtpServerTask != null)
          {
            try
            {
              Assert.IsTrue(smtpServerTask.Wait(1000), "SmtpServer to did not stop timely");
            }
            catch (AggregateException e)
            {
              var ee = e.GetBaseException();
              Assert.That(ee, Is.TypeOf<System.IO.IOException>(), "Seems the smtp server failed...");
            }
            smtpServerTask = null;
          }

          cancellationTokenSource.Dispose();
        }

        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }

  internal class MemoryMessageStore : MessageStore
  {
    private readonly Func<SmtpResponse> getSmtpResponse;

    public MemoryMessageStore(Func<SmtpResponse> getSmtpResponse)
    {
      this.getSmtpResponse = getSmtpResponse;
      Messages = new List<MimeMessage>();
    }

    public IList<MimeMessage> Messages { get; private set; }

    public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
    {
      var textMessage = (ITextMessage)transaction.Message;
      var message = MimeMessage.Load(textMessage.Content);
      Messages.Add(message);

      return Task.FromResult(getSmtpResponse());
    }
  }

  internal class UserAuthenticator : IUserAuthenticator, IUserAuthenticatorFactory
  {
    private readonly string user;
    private readonly string password;
    
    public UserAuthenticator(string user, string password)
    {
      this.user = user;
      this.password = password;
    }

    public Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken)
    {
      var userOk = this.user == user;
      var passwordOk = this.password == password;
      return Task.FromResult(userOk && passwordOk);
    }

    public IUserAuthenticator CreateInstance(ISessionContext context)
    {
      return this;
    }
  }

  internal class VoidUserAuthenticator : IUserAuthenticatorFactory
  {
    public IUserAuthenticator CreateInstance(ISessionContext context)
    {
      return null;
    }
  }

  internal class SmtpLogger : ILogger
  {
    private readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();
    
    public void LogVerbose(string format, params object[] args)
    {
      sb.AppendFormat(format, args).AppendLine(string.Empty);
    }

    public override string ToString()
    {
      return sb.ToString();
    }
  }
}
