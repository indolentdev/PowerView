using System;
using System.Linq;
using System.Reflection;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using PowerView.Service.Mailer;
using PowerView.Service.Translations;

namespace PowerView.Service.Modules
{
  public class SettingsEmailRecipientsModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IEmailRecipientRepository emailRecipientRepository;
    private readonly ITranslation translation;
    private readonly IMailMediator mailMediator;

    public SettingsEmailRecipientsModule(IEmailRecipientRepository emailRecipientRepository, ITranslation translation, IMailMediator mailMediator)
      : base("/api/settings/emailrecipients")
    {
      if (emailRecipientRepository == null) throw new ArgumentNullException("emailRecipientRepository");
      if (translation == null) throw new ArgumentNullException("translation");
      if (mailMediator == null) throw new ArgumentNullException("mailMediator");

      this.emailRecipientRepository = emailRecipientRepository;
      this.translation = translation;
      this.mailMediator = mailMediator;

      Get[""] = GetEmailRecipients;
      Post[""] = AddEmailRecipient;
      Delete["{emailAddress}"] = DeleteEmailRecipient;
      Put["{emailAddress}/test"] = TestEmailRecipient;
    }

    private dynamic GetEmailRecipients(dynamic param)
    {
      var emailRecipients = emailRecipientRepository.GetEmailRecipients();

      var emailRecipientsDto = emailRecipients.Select(er => new { er.Name, er.EmailAddress }).ToArray();
      var r = new { Items = emailRecipientsDto };

      return Response.AsJson(r);
    }

    private dynamic AddEmailRecipient(dynamic param)
    {
      var emailRecipientDto = this.Bind<EmailRecipientDto>();
      if (string.IsNullOrEmpty(emailRecipientDto.Name) || string.IsNullOrEmpty(emailRecipientDto.EmailAddress))
      {
        log.WarnFormat("Add email recipient failed. Properties are null or empty. Name:" +
                       emailRecipientDto.Name + ". EmailAddress:" + emailRecipientDto.EmailAddress);
        var description = new { Description = "Name or EmailAddress properties absent or empty" };
        return Response.AsJson(description, HttpStatusCode.UnsupportedMediaType);
      }
      try
      {
        var emailRecipient = new EmailRecipient(emailRecipientDto.Name, emailRecipientDto.EmailAddress);
        emailRecipientRepository.AddEmailRecipient(emailRecipient);
      }
      catch (FormatException e)
      {
        log.Warn("Add email recipient failed. EmailAddress:" + emailRecipientDto.EmailAddress, e);
        return Response.AsJson(new { Description = "EmailAddress has invalid format" }, HttpStatusCode.UnsupportedMediaType);
      }
      catch (DataStoreUniqueConstraintException e)
      {
        log.Warn("Add email recipient failed. EmailAddress:" + emailRecipientDto.EmailAddress, e);
        return Response.AsJson(new { Description = "EmailAddress already exists" }, HttpStatusCode.Conflict);
      }

      return HttpStatusCode.NoContent;
    }

    private dynamic DeleteEmailRecipient(dynamic param)
    {
      string emailAddress = param.emailAddress;

      emailRecipientRepository.DeleteEmailRecipient(emailAddress);

      return HttpStatusCode.NoContent;
    }

    private dynamic TestEmailRecipient(dynamic param)
    {
      string emailAddress = param.emailAddress;

      var emailRecipient = emailRecipientRepository.GetEmailRecipient(emailAddress);
      if (emailRecipient == null)
      {
        return HttpStatusCode.NotFound;
      }
      var subject = translation.Get(ResId.MailSubjectTest);
      var message = translation.Get(ResId.MailMessageTest);

      try
      {
        mailMediator.SendEmail(emailRecipient, subject, message);
        return HttpStatusCode.NoContent;
      }
      catch (ConnectMailerException e)
      {
        log.Warn("Test email recipient failed. EmailAddress:" + emailAddress, e);
        return HttpStatusCode.GatewayTimeout;
      }
      catch (AuthenticateMailerException e)
      {
        log.Warn("Test email recipient failed. EmailAddress:" + emailAddress, e);
        return 567; // GatewayUnauthorized
      }
      catch (MailerException e)
      {
        log.Warn("Test email recipient failed. EmailAddress:" + emailAddress, e);
        return HttpStatusCode.BadRequest;
      }
    }
  }
}
