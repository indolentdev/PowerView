using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/settings/smtp")]
public class SettingsSmtpController : ControllerBase
{
    private readonly ILogger logger;

    private readonly ISettingRepository settingRepository;

    public SettingsSmtpController(ILogger<SettingsSmtpController> logger, ISettingRepository settingRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetSettings()
    {
        SmtpConfig smtpConfig;
        try
        {
            smtpConfig = settingRepository.GetSmtpConfig();
        }
        catch (DomainConstraintException)
        {
            return Ok(new { });
        }

        var r = new
        {
            Server = smtpConfig.Server,
            Port = smtpConfig.Port,
            User = smtpConfig.User,
            Auth = smtpConfig.Auth,
            Email = smtpConfig.Email
        };
        return Ok(r);
    }

    [HttpPut("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public ActionResult PutSettings([FromBody] SmtpConfigDto smtpConfigDto)
    {
        var smtpConfig = GetSmtpConfig(smtpConfigDto);
        if (smtpConfig == null)
        {
            var description = new { Description = "Server, Port, User or Auth properties absent, empty or invalid" };
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, description);
        }

        settingRepository.UpsertSmtpConfig(smtpConfig);
        return NoContent();
    }

    private SmtpConfig GetSmtpConfig(SmtpConfigDto smtpConfigDto)
    {
        if (string.IsNullOrEmpty(smtpConfigDto.Server) || string.IsNullOrEmpty(smtpConfigDto.User) || string.IsNullOrEmpty(smtpConfigDto.Auth) || string.IsNullOrEmpty(smtpConfigDto.Email))
        {
            logger.LogWarning($"Set SMTP configuration failed. Properties are null or empty. Server:{smtpConfigDto.Server}, Port:{smtpConfigDto.Port}, User:{smtpConfigDto.User}, Auth:********, Email:{smtpConfigDto.Email}");
            return null;
        }

        return new SmtpConfig(smtpConfigDto.Server, smtpConfigDto.Port, smtpConfigDto.User, smtpConfigDto.Auth, smtpConfigDto.Email);
    }

}
