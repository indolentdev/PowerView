using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using PowerView.Service.Mailer;
using PowerView.Service.Translations;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/settings/emailrecipients")]
public class SettingsEmailRecipientsController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IEmailRecipientRepository emailRecipientRepository;
    private readonly ITranslation translation;
    private readonly IMailMediator mailMediator;

    public SettingsEmailRecipientsController(ILogger<SettingsEmailRecipientsController> logger, IEmailRecipientRepository emailRecipientRepository, ITranslation translation, IMailMediator mailMediator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.emailRecipientRepository = emailRecipientRepository ?? throw new ArgumentNullException(nameof(emailRecipientRepository));
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.mailMediator = mailMediator ?? throw new ArgumentNullException(nameof(mailMediator));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetEmailRecipients()
    {
        var emailRecipients = emailRecipientRepository.GetEmailRecipients();

        var emailRecipientsDto = emailRecipients.Select(er => new { er.Name, er.EmailAddress }).ToArray();
        var r = new { Items = emailRecipientsDto };
        return Ok(r);
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public ActionResult AddEmailRecipient([FromBody] EmailRecipientDto emailRecipientDto)
    {
        if (string.IsNullOrEmpty(emailRecipientDto.Name) || string.IsNullOrEmpty(emailRecipientDto.EmailAddress))
        {
            logger.LogWarning($"Add email recipient failed. Properties are null or empty. Name:{emailRecipientDto.Name}. EmailAddress:{emailRecipientDto.EmailAddress}");
            var description = new { Description = "Name or EmailAddress properties absent or empty" };
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, description);
        }

        try
        {
            var emailRecipient = new EmailRecipient(emailRecipientDto.Name, emailRecipientDto.EmailAddress);
            emailRecipientRepository.AddEmailRecipient(emailRecipient);
        }
        catch (FormatException e)
        {
            logger.LogWarning(e, $"Add email recipient failed. Invalid format. Name:{emailRecipientDto.Name}. EmailAddress:{emailRecipientDto.EmailAddress}");
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { Description = "EmailAddress has invalid format" });
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, $"Add email recipient failed. Email address already exists. Name:{emailRecipientDto.Name}. EmailAddress:{emailRecipientDto.EmailAddress}");
            return Conflict(new { Description = "EmailAddress already exists" });
        }

        return NoContent();
    }

    [HttpDelete("{emailAddress}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult DeleteEmailRecipient([BindRequired] string emailAddress)
    {
        emailRecipientRepository.DeleteEmailRecipient(emailAddress);

        return NoContent();
    }

    [HttpPut("{emailAddress}/test")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public ActionResult TestEmailRecipient([BindRequired] string emailAddress)
    {
        var emailRecipient = emailRecipientRepository.GetEmailRecipient(emailAddress);
        if (emailRecipient == null)
        {
            return NotFound();
        }
        var subject = translation.Get(ResId.MailSubjectTest);
        var message = translation.Get(ResId.MailMessageTest);

        try
        {
            mailMediator.SendEmail(emailRecipient, subject, message);
            return NoContent();
        }
        catch (ConnectMailerException e)
        {
            logger.LogWarning(e, $"Test email recipient failed. SMTP connection failed. EmailAddress:{emailAddress}");
            return StatusCode(StatusCodes.Status504GatewayTimeout);
        }
        catch (AuthenticateMailerException e)
        {
            logger.LogWarning(e, $"Test email recipient failed. SMTP user authentication failed. EmailAddress:{emailAddress}");
            return StatusCode(567); // GatewayUnauthorized
        }
        catch (MailerException e)
        {
            logger.LogWarning(e, $"Test email recipient failed. EmailAddress:{emailAddress}");
            return BadRequest();
        }
    }
}
