using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using PowerView.Service.Mqtt;

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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult PutSettings([BindRequired, FromBody] SmtpConfigDto smtpConfigDto)
    {
        var smtpConfig = new SmtpConfig(smtpConfigDto.Server, smtpConfigDto.Port.Value, smtpConfigDto.User, smtpConfigDto.Auth, smtpConfigDto.Email);
        settingRepository.UpsertSmtpConfig(smtpConfig);
        return NoContent();
    }

}
