using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using NUnit.Framework;
using SmtpServer;     // https://github.com/cosullivan/SmtpServer
using SmtpServer.Authentication;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace PowerView.Service.Test.Mailer
{
    internal class TestSmtpServer
    {
        private SmtpServer.SmtpServer smtpServer;
        private Task smtpServerTask;
        private IUserAuthenticator userAuthenticator;
        private X509Certificate2 certificate;
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

        public void SetNextSendSmtpResponse(SmtpResponse smtpResponse)
        {
            nextSmtpResponse = smtpResponse;
        }

        public void RequireTls(string siteName)
        {
            var password = "123456789";
            var certificateGenerator = new CertificateGenerator();
            var pfxCertificateBytes = certificateGenerator.GenerateCertificateForPersonalFileExchange(siteName, password);
            certificate = new X509Certificate2(pfxCertificateBytes, password);
        }

        public void RequireUser(string user, string password)
        {
            userAuthenticator = new SingleUserAuthenticator(user, password);
        }

        public void Start()
        {
            var optionsBuilder = new SmtpServerOptionsBuilder()
              .ServerName(ServerName)
              .MaxRetryCount(1);

            if (certificate != null)
            {
                optionsBuilder.Endpoint(builder => builder
                                .Port(Port)
                                .SupportedSslProtocols(System.Security.Authentication.SslProtocols.None)
                                .Certificate(certificate));
            }
            else
            {
                optionsBuilder.Endpoint(builder => builder
                                .Port(Port));
            }

            var options = optionsBuilder.Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IMessageStore>(messageStore);

            if (userAuthenticator != null)
            {
                serviceCollection.AddSingleton(userAuthenticator);
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();

            smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);
            smtpServerTask = smtpServer.StartAsync(CancellationToken.None);
        }

        public async Task Shutdown()
        {
            if (smtpServer != null)
            {
                smtpServer.Shutdown();
                await smtpServer.ShutdownTask;
            }

            if (smtpServerTask != null)
            {
                await smtpServerTask;
            }
        }
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

        public async override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();

            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                await stream.WriteAsync(memory, cancellationToken);
            }
            stream.Position = 0;

            var message = await MimeMessage.LoadAsync(stream, cancellationToken);
            Messages.Add(message);

            return getSmtpResponse();
        }
    }

    internal class SingleUserAuthenticator : IUserAuthenticator
    {
        private readonly string user;
        private readonly string password;

        public SingleUserAuthenticator(string user, string password)
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
    }

}
