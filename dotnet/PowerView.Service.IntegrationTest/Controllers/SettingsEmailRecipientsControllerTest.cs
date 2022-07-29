using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Controllers;
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;
using PowerView.Service.Mailer;
using PowerView.Service.Translations;

namespace PowerView.Service.IntegrationTest;

[TestFixture]
public class SettingsEmailRecipientsControllerTest
{
    private WebApplicationFactory<TestProgram> application;
    private HttpClient httpClient;
    private Mock<IEmailRecipientRepository> emailRecipientRepository;
    private Mock<ITranslation> translation;
    private Mock<IMailMediator> mailMediator;

    [SetUp]
    public void SetUp()
    {
        emailRecipientRepository = new Mock<IEmailRecipientRepository>();
        translation = new Mock<ITranslation>();
        mailMediator = new Mock<IMailMediator>();

        application = new WebApplicationFactory<TestProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices((ctx, sc) =>
                {
                    sc.AddSingleton(emailRecipientRepository.Object);
                    sc.AddSingleton(translation.Object);
                    sc.AddSingleton(mailMediator.Object);
                });
            });

        httpClient = application.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        application?.Dispose();
    }

    [Test]
    public async Task GetEmailRecipients()
    {
        // Arrange
        var er1 = new EmailRecipient("Hugo", "a@b.com");
        var er2 = new EmailRecipient("Hansi", "p@q.com");
        var emailRecipients = new[] { er1, er2 };
        emailRecipientRepository.Setup(err => err.GetEmailRecipients()).Returns(emailRecipients);

        // Act
        var response = await httpClient.GetAsync($"api/settings/emailrecipients");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<TestEmailRecipientSetDto>();
        Assert.That(json.items, Is.Not.Null);
        Assert.That(json.items.Length, Is.EqualTo(2));
        AssertEmailRecipient(er1, json.items.First());
        AssertEmailRecipient(er2, json.items.Last());
    }

    private static void AssertEmailRecipient(EmailRecipient expected, TestEmailRecipientDto actual)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.name, Is.EqualTo(expected.Name));
        Assert.That(actual.emailAddress, Is.EqualTo(expected.EmailAddress));
    }

    [Test]
    public async Task GetEmailRecipientsCallsRepository()
    {
        // Arrange
        var emailRecipients = new[] { new EmailRecipient("Hugo", "a@b.com") };
        emailRecipientRepository.Setup(err => err.GetEmailRecipients()).Returns(emailRecipients);

        // Act
        var response = await httpClient.GetAsync($"api/settings/emailrecipients");

        // Assert
        emailRecipientRepository.Verify(err => err.GetEmailRecipients());
    }

    [Test]
    public async Task AddEmailRecipient()
    {
        // Arrange
        var emailRecipientDto = new EmailRecipientDto { Name = "The Name", EmailAddress = "a@b.com" };
        var content = JsonContent.Create(emailRecipientDto);

        // Act
        var response = await httpClient.PostAsync($"api/settings/emailrecipients", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        emailRecipientRepository.Verify(err => err.AddEmailRecipient(
          It.Is<EmailRecipient>(x => x.Name == emailRecipientDto.Name && x.EmailAddress == emailRecipientDto.EmailAddress)));
    }

    [Test]
    public async Task AddEmailRecipientNoContent()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync($"api/settings/emailrecipients", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AddEmailRecipientBadEmailAddress()
    {
        // Arrange
        var emailRecipientDto = new { Name = "The Name", EmailAddress = "BadEmailAddress" };
        var content = JsonContent.Create(emailRecipientDto);

        // Act
        var response = await httpClient.PostAsync($"api/settings/emailrecipients", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AddEmailRecipientEmailAddressAlreadyExists()
    {
        // Arrange
        emailRecipientRepository.Setup(err => err.AddEmailRecipient(It.IsAny<EmailRecipient>())).Throws(new DataStoreUniqueConstraintException());
        var emailRecipientDto = new { Name = "The Name", EmailAddress = "a@b.com" };
        var content = JsonContent.Create(emailRecipientDto);

        // Act
        var response = await httpClient.PostAsync($"api/settings/emailrecipients", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task DeleteEmailRecipientBadEmailAddress()
    {
        // Arrange
        const string emailAddress = "badEmailAddress";

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/emailrecipients/{emailAddress}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        emailRecipientRepository.Verify(drr => drr.DeleteEmailRecipient(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task DeleteEmailRecipient()
    {
        // Arrange
        const string emailAddress = "a@b.com";

        // Act
        var response = await httpClient.DeleteAsync($"api/settings/emailrecipients/{emailAddress}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        emailRecipientRepository.Verify(err => err.DeleteEmailRecipient(emailAddress));
    }

    [Test]
    public async Task TestEmailRecipientNotFound()
    {
        // Arrange
        const string emailAddress = "a@b.com";

        // Act
        var response = await httpClient.PutAsync($"api/settings/emailrecipients/{emailAddress}/test", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        emailRecipientRepository.Verify(err => err.GetEmailRecipient(emailAddress));
    }

    [Test]
    public async Task TestEmailRecipient()
    {
        // Arrange
        const string emailAddress = "a@b.com";
        var emailRecipient = new EmailRecipient("The Name", emailAddress);
        emailRecipientRepository.Setup(err => err.GetEmailRecipient(It.IsAny<string>())).Returns(emailRecipient);
        const string subject = "The Subject";
        translation.Setup(t => t.Get(ResId.MailSubjectTest)).Returns(subject);
        const string message = "The Message";
        translation.Setup(t => t.Get(ResId.MailMessageTest)).Returns(message);

        // Act
        var response = await httpClient.PutAsync($"api/settings/emailrecipients/{emailAddress}/test", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        translation.Verify(t => t.Get(ResId.MailSubjectTest));
        translation.Verify(t => t.Get(ResId.MailMessageTest));
        mailMediator.Verify(mm => mm.SendEmail(emailRecipient, subject, message));
    }

    [Test]
    public async Task TestEmailRecipientConnectError()
    {
        // Arrange
        const string emailAddress = "a@b.com";
        var emailRecipient = new EmailRecipient("The Name", emailAddress);
        emailRecipientRepository.Setup(err => err.GetEmailRecipient(It.IsAny<string>())).Returns(emailRecipient);
        mailMediator.Setup(mm => mm.SendEmail(It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ConnectMailerException());

        // Act
        var response = await httpClient.PutAsync($"api/settings/emailrecipients/{emailAddress}/test", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.GatewayTimeout));
    }

    [Test]
    public async Task TestEmailRecipientAuthenticateError()
    {
        // Arrange
        const string emailAddress = "a@b.com";
        var emailRecipient = new EmailRecipient("The Name", emailAddress);
        emailRecipientRepository.Setup(err => err.GetEmailRecipient(It.IsAny<string>())).Returns(emailRecipient);
        mailMediator.Setup(mm => mm.SendEmail(It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new AuthenticateMailerException());

        // Act
        var response = await httpClient.PutAsync($"api/settings/emailrecipients/{emailAddress}/test", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)567));
    }

    [Test]
    public async Task TestEmailRecipientError()
    {
        // Arrange
        const string emailAddress = "a@b.com";
        var emailRecipient = new EmailRecipient("The Name", emailAddress);
        emailRecipientRepository.Setup(err => err.GetEmailRecipient(It.IsAny<string>())).Returns(emailRecipient);
        mailMediator.Setup(mm => mm.SendEmail(It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new MailerException());

        // Act
        var response = await httpClient.PutAsync($"api/settings/emailrecipients/{emailAddress}/test", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    internal class TestEmailRecipientSetDto
    {
        public TestEmailRecipientDto[] items { get; set; }
    }

    internal class TestEmailRecipientDto
    {
        public string name { get; set; }
        public string emailAddress { get; set; }
    }

}
