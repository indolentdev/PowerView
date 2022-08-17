using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using PowerView.Service.EventHub;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mailer;
using PowerView.Service.Translations;
using Moq;
using NUnit.Framework;

namespace PowerView.Service.Test.EventHub
{
  [TestFixture]
  public class MeterEventNotifierTest
  {
    private Mock<IEmailRecipientRepository> emailRecipientRepository;
    private Mock<IMeterEventRepository> meterEventRepository;
    private Mock<ITranslation> translation;
    private Mock<IUrlProvider> urlProvider;
    private Mock<IMailMediator> mailMediator;

    [Test]
    public void NotifyEmailRecipientsNoMaxPos()
    {
      // Arrange
      var target = CreateTarget();
      meterEventRepository.Setup(r => r.GetMaxFlaggedMeterEventId()).Returns((long?)null);

      // Act
      target.NotifyEmailRecipients();

      // Assert
      emailRecipientRepository.Verify(r => r.GetEmailRecipientsMeterEventPosition(), Times.Never());
    }

    [Test]
    public void NotifyEmailRecipientsNoEmailRecipients()
    {
      // Arrange
      var target = CreateTarget();
      meterEventRepository.Setup(r => r.GetMaxFlaggedMeterEventId()).Returns(1);
      emailRecipientRepository.Setup(r => r.GetEmailRecipientsMeterEventPosition())
                              .Returns(new Dictionary<EmailRecipient, long?>());

      // Act
      target.NotifyEmailRecipients();

      // Assert
      mailMediator.Verify(m => m.SendEmail(It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void NotifyEmailRecipientsOneEmailRecipient()
    {
      // Arrange
      var target = CreateTarget();
      meterEventRepository.Setup(r => r.GetMaxFlaggedMeterEventId()).Returns(1);
      var emailRecipient = new EmailRecipient("TheName", "a@b.com");
      emailRecipientRepository.Setup(r => r.GetEmailRecipientsMeterEventPosition())
                              .Returns(new Dictionary<EmailRecipient, long?>{ {emailRecipient, null} });
      var url = new Uri("http://stuff");
      urlProvider.Setup(u => u.GetEventsUrl()).Returns(url);
      const string subject = "sub";
      const string msg = "The Message";
      translation.Setup(t => t.Get(ResId.MailSubjectNewEvents)).Returns(subject).Verifiable();
      translation.Setup(t => t.Get(ResId.MailMessageNewEvents, url)).Returns(msg).Verifiable();

      // Act
      target.NotifyEmailRecipients();

      // Assert
      urlProvider.Verify(u => u.GetEventsUrl());
      translation.Verify();
      mailMediator.Verify(m => m.SendEmail(It.Is<EmailRecipient>(x => x == emailRecipient), 
                                           It.Is<string>(x => x == subject), It.Is<string>(x => x == msg)));
      emailRecipientRepository.Verify(r => r.SetEmailRecipientMeterEventPosition(It.Is<string>(x => x == emailRecipient.EmailAddress),
                                                                                It.IsAny<long>()));
    }

    [Test]
    public void NotifyEmailRecipientsSendMailThrows()
    {
      // Arrange
      var target = CreateTarget();
      meterEventRepository.Setup(r => r.GetMaxFlaggedMeterEventId()).Returns(1);
      var emailRecipient = new EmailRecipient("TheName", "a@b.com");
      emailRecipientRepository.Setup(r => r.GetEmailRecipientsMeterEventPosition())
                              .Returns(new Dictionary<EmailRecipient, long?> { { emailRecipient, null } });
      mailMediator.Setup(m => m.SendEmail(It.IsAny<EmailRecipient>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Throws(new MailerException());

      // Act
      target.NotifyEmailRecipients();

      // Assert

      emailRecipientRepository.Verify(r => r.SetEmailRecipientMeterEventPosition(It.IsAny<string>(), It.IsAny<long>()), Times.Never());
    }

    [Test]
    public void NotifyEmailRecipientsNewPosition()
    {
      // Arrange
      var target = CreateTarget();
      const long maxPosition = 6;
      meterEventRepository.Setup(r => r.GetMaxFlaggedMeterEventId()).Returns(maxPosition);
      var emailRecipient = new EmailRecipient("TheName", "a@b.com");
      emailRecipientRepository.Setup(r => r.GetEmailRecipientsMeterEventPosition())
                              .Returns(new Dictionary<EmailRecipient, long?> { { emailRecipient, null } });
 
      // Act
      target.NotifyEmailRecipients();

      // Assert
      emailRecipientRepository.Verify(r => r.SetEmailRecipientMeterEventPosition(It.IsAny<string>(),
                                                                                It.Is<long>(x => x == maxPosition)));
    }

    [Test]
    public void NotifyEmailRecipientsUpdatedPosition()
    {
      // Arrange
      var target = CreateTarget();
      const long maxPosition = 6;
      meterEventRepository.Setup(r => r.GetMaxFlaggedMeterEventId()).Returns(maxPosition);
      const long receipientPosition = 5;
      var emailRecipient = new EmailRecipient("TheName", "a@b.com");
      emailRecipientRepository.Setup(r => r.GetEmailRecipientsMeterEventPosition())
                              .Returns(new Dictionary<EmailRecipient, long?> { { emailRecipient, receipientPosition } });

      // Act
      target.NotifyEmailRecipients();

      // Assert
      emailRecipientRepository.Verify(r => r.SetEmailRecipientMeterEventPosition(It.IsAny<string>(),
                                                                                It.Is<long>(x => x == maxPosition)));
    }


    private MeterEventNotifier CreateTarget()
    {
      emailRecipientRepository = new Mock<IEmailRecipientRepository>();
      meterEventRepository = new Mock<IMeterEventRepository>();
      translation = new Mock<ITranslation>();
      urlProvider = new Mock<IUrlProvider>();
      mailMediator = new Mock<IMailMediator>();
      return new MeterEventNotifier(new NullLogger<MeterEventNotifier>(), emailRecipientRepository.Object, meterEventRepository.Object, translation.Object,
                                    urlProvider.Object, mailMediator.Object);

    }
  }
}
