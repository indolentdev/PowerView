using System;
using PowerView.Model;
using PowerView.Model.Repository;

namespace PowerView.Service.Mailer
{
  public class MailMediator : IMailMediator
  {
    private readonly IMailer mailer;
    private readonly ISettingRepository settingRepository;
    private readonly IEmailMessageRepository emailMessageRepository;

    public MailMediator(IMailer mailer, ISettingRepository settingRepository, IEmailMessageRepository emailMessageRepository)
    {
      if (mailer == null) throw new ArgumentNullException("mailer");
      if (settingRepository == null) throw new ArgumentNullException("settingRepository");
      if (emailMessageRepository == null) throw new ArgumentNullException("emailMessageRepository");

      this.mailer = mailer;
      this.settingRepository = settingRepository;
      this.emailMessageRepository = emailMessageRepository;
    }

    public void SendEmail(EmailRecipient emailRecipient, string subject, string message)
    {
      SmtpConfig smtpConfig;
      try
      {
        smtpConfig = settingRepository.GetSmtpConfig();
      }
      catch (DomainConstraintException e)
      {
        throw new MailerException("SendEmail failed. SMTP configuration incomplete", e);
      }
      var sender = mailer.Send(smtpConfig, emailRecipient, subject, message);
      emailMessageRepository.AddEmailMessage(sender, emailRecipient, subject, message);
    }
  }
}
