using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using log4net;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mailer;
using PowerView.Service.Translations;

namespace PowerView.Service.EventHub
{
  public class MeterEventNotifier : IMeterEventNotifier
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IEmailRecipientRepository emailRecipientRepository;
    private readonly IMeterEventRepository meterEventRepository;
    private readonly ITranslation translation;
    private readonly IUrlProvider urlProvider;
    private readonly IMailMediator mailMediator;

    public MeterEventNotifier(IEmailRecipientRepository emailRecipientRepository, IMeterEventRepository meterEventRepository, ITranslation translation, IUrlProvider urlProvider, IMailMediator mailMediator)
    {
      this.emailRecipientRepository = emailRecipientRepository;
      this.meterEventRepository = meterEventRepository;
      this.translation = translation;
      this.urlProvider = urlProvider;
      this.mailMediator = mailMediator;
    }

    public void NotifyEmailRecipients()
    {
      var maxMeterEventId = meterEventRepository.GetMaxFlaggedMeterEventId();
      if (maxMeterEventId == null)
      {
        return;
      }
      foreach (var emailRecipient in GetEmailRecipientsWithNewMeterEvents(maxMeterEventId.Value))
      {
        var subject = translation.Get(ResId.MailSubjectNewEvents);
        var eventsUrl = urlProvider.GetEventsUrl();
        var message = translation.Get(ResId.MailMessageNewEvents, eventsUrl);

        try
        {
          mailMediator.SendEmail(emailRecipient, subject, message);
          log.InfoFormat("Sent new events to email recipient. Name:{0}, EmailAddress:{1}. Subject:{2}", 
                         emailRecipient.Name, emailRecipient.EmailAddress, subject);
          emailRecipientRepository.SetEmailRecipientMeterEventPosition(emailRecipient.EmailAddress, maxMeterEventId.Value);
        }
        catch (MailerException e)
        {
          var msg = string.Format(CultureInfo.InvariantCulture, 
                                  "Failed sending email for new events to email recipient. Name:{0}, EmailAddress:{1}",
                                  emailRecipient.Name, emailRecipient.EmailAddress);
          log.Warn(msg, e);
        }
      }
    }

    private IEnumerable<EmailRecipient> GetEmailRecipientsWithNewMeterEvents(long maxMeterEventId)
    {
      var emailRecipientsMeterEventPosition = emailRecipientRepository.GetEmailRecipientsMeterEventPosition();

      foreach (var item in emailRecipientsMeterEventPosition)
      {
        if (item.Value == null || item.Value.Value < maxMeterEventId)
        {
          yield return item.Key;
        }
      }
    }
  }
}
