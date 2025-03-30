using NUnit.Framework;
using Moq;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mailer;

namespace PowerView.Service.Test.Mailer
{
    [TestFixture]
    public class MailMediatorTest
    {
        private Mock<IMailer> mailer;
        private Mock<ISettingRepository> settingRepository;
        private Mock<IEmailMessageRepository> emailMessageRepository;

        [SetUp]
        public void SetUp()
        {
            mailer = new Mock<IMailer>();
            settingRepository = new Mock<ISettingRepository>();
            emailMessageRepository = new Mock<IEmailMessageRepository>();
        }

        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new MailMediator(null, settingRepository.Object, emailMessageRepository.Object), Throws.ArgumentNullException);
            Assert.That(() => new MailMediator(mailer.Object, null, emailMessageRepository.Object), Throws.ArgumentNullException);
            Assert.That(() => new MailMediator(mailer.Object, settingRepository.Object, null), Throws.ArgumentNullException);
        }

        [Test]
        public void SendEmail()
        {
            // Arrange
            var target = CreateTarget();
            var smtpConfig = new SmtpConfig("TheServer", 587, "tx@b.com", "txAuth", "tx@b.com");
            settingRepository.Setup(x => x.GetSmtpConfig()).Returns(smtpConfig);
            var emailRecipient = new EmailRecipient("Rx Name", "rx@b.com");
            const string subject = "The Subject";
            const string message = "The Message";
            var sender = new EmailRecipient("Tx Name", "tx@b.com");
            mailer.Setup(x => x.Send(It.IsAny<SmtpConfig>(), It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>())).Returns(sender);

            // Act
            target.SendEmail(emailRecipient, subject, message);

            // Assert
            settingRepository.Verify(sr => sr.GetSmtpConfig());
            mailer.Verify(m => m.Send(It.Is<SmtpConfig>(x => x == smtpConfig), It.Is<EmailRecipient>(x => x.Name == emailRecipient.Name && x.EmailAddress == emailRecipient.EmailAddress),
                                      It.Is<string>(x => x == subject), It.Is<string>(x => x == message)));
            emailMessageRepository.Verify(emr => emr.AddEmailMessage(It.Is<EmailRecipient>(x => x.Name == sender.Name && x.EmailAddress == sender.EmailAddress),
                                                                    It.Is<EmailRecipient>(x => x.Name == emailRecipient.Name && x.EmailAddress == emailRecipient.EmailAddress),
                                                                    It.Is<string>(x => x == subject), It.Is<string>(x => x == message)));
        }

        [Test]
        public void SendEmailThrows()
        {
            // Arrange
            var target = CreateTarget();
            var emailRecipient = new EmailRecipient("Rx Name", "rx@b.com");
            const string subject = "The Subject";
            const string message = "The Message";
            var exception = new MailerException();
            mailer.Setup(m => m.Send(It.IsAny<SmtpConfig>(), It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Throws(exception);

            // Act & Assert
            Assert.That(() => target.SendEmail(emailRecipient, subject, message), Throws.TypeOf(exception.GetType()));
        }

        [Test]
        public void SendEmailSmtpConfigUnavailable()
        {
            // Arrange
            var target = CreateTarget();
            var emailRecipient = new EmailRecipient("Rx Name", "rx@b.com");
            const string subject = "The Subject";
            const string message = "The Message";
            settingRepository.Setup(sr => sr.GetSmtpConfig()).Throws(new DomainConstraintException());

            // Act & Assert
            Assert.That(() => target.SendEmail(emailRecipient, subject, message), Throws.TypeOf<MailerException>());
        }

        private MailMediator CreateTarget()
        {
            return new MailMediator(mailer.Object, settingRepository.Object, emailMessageRepository.Object);
        }

    }
}
