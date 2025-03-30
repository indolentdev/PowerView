using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mailer;
using PowerView.Service.Translations;

namespace PowerView.Service.EventHub
{
    public class MeterEventNotifier : IMeterEventNotifier
    {
        private readonly ILogger logger;
        private readonly IEmailRecipientRepository emailRecipientRepository;
        private readonly IMeterEventRepository meterEventRepository;
        private readonly ITranslation translation;
        private readonly IUrlProvider urlProvider;
        private readonly IMailMediator mailMediator;

        public MeterEventNotifier(ILogger<MeterEventNotifier> logger, IEmailRecipientRepository emailRecipientRepository, IMeterEventRepository meterEventRepository, ITranslation translation, IUrlProvider urlProvider, IMailMediator mailMediator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.emailRecipientRepository = emailRecipientRepository ?? throw new ArgumentNullException(nameof(emailRecipientRepository));
            this.meterEventRepository = meterEventRepository ?? throw new ArgumentNullException(nameof(meterEventRepository));
            this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
            this.urlProvider = urlProvider ?? throw new ArgumentNullException(nameof(urlProvider));
            this.mailMediator = mailMediator ?? throw new ArgumentNullException(nameof(mailMediator));
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
                    logger.LogInformation($"Sent new events to email recipient. Name:{emailRecipient.Name}, EmailAddress:{emailRecipient.EmailAddress}. Subject:{subject}");
                    emailRecipientRepository.SetEmailRecipientMeterEventPosition(emailRecipient.EmailAddress, maxMeterEventId.Value);
                }
                catch (MailerException e)
                {
                    logger.LogWarning(e, $"Failed sending email for new events to email recipient. Name:{emailRecipient.Name}, EmailAddress:{emailRecipient.EmailAddress}");
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
