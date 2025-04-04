﻿using System.ComponentModel.DataAnnotations;
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult AddEmailRecipient([BindRequired, FromBody] EmailRecipientDto emailRecipientDto)
    {
        var emailRecipient = new EmailRecipient(emailRecipientDto.Name, emailRecipientDto.EmailAddress);
        try
        {
            emailRecipientRepository.AddEmailRecipient(emailRecipient);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, "Add email recipient failed. Email address already exists. Name:{Name}. EmailAddress:{EmailAddress}", emailRecipientDto.Name, emailRecipientDto.EmailAddress);
            return Conflict(new { Description = "EmailAddress already exists" });
        }

        return NoContent();
    }

    [HttpDelete("{emailAddress}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult DeleteEmailRecipient([BindRequired, FromRoute, StringLength(255, MinimumLength = 1), EmailAddress] string emailAddress)
    {
        emailRecipientRepository.DeleteEmailRecipient(emailAddress);

        return NoContent();
    }

    [HttpPut("{emailAddress}/test")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public ActionResult TestEmailRecipient([BindRequired, FromRoute, StringLength(255, MinimumLength = 1), EmailAddress] string emailAddress)
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
            logger.LogWarning(e, "Test email recipient failed. SMTP connection failed. EmailAddress:{EmailAddress}", emailAddress);
            return StatusCode(StatusCodes.Status504GatewayTimeout);
        }
        catch (AuthenticateMailerException e)
        {
            logger.LogWarning(e, "Test email recipient failed. SMTP user authentication failed. EmailAddress:{EmailAddress}", emailAddress);
            return StatusCode(567); // GatewayUnauthorized
        }
        catch (MailerException e)
        {
            logger.LogWarning(e, "Test email recipient failed. EmailAddress:{EmailAddress}", emailAddress);
            return BadRequest();
        }
    }
}
