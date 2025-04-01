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
[Route("api/settings/mqtt")]
public class SettingsMqttController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ISettingRepository settingRepository;
    private readonly IMqttPublisher mqttPublisher;

    public SettingsMqttController(ILogger<SettingsMqttController> logger, ISettingRepository settingRepository, IMqttPublisher mqttPublisher)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
        this.mqttPublisher = mqttPublisher ?? throw new ArgumentNullException(nameof(mqttPublisher));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetSettings()
    {
        var mqttConfig = settingRepository.GetMqttConfig();

        var r = new
        {
            Server = mqttConfig.Server,
            Port = mqttConfig.Port,
            PublishEnabled = mqttConfig.PublishEnabled,
            ClientId = mqttConfig.ClientId
        };
        return Ok(r);
    }

    [HttpPut("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult PutSettings([BindRequired, FromBody] MqttConfigDto mqttConfigDto)
    {
        var mqttConfig = new MqttConfig(mqttConfigDto.Server, mqttConfigDto.Port.Value, mqttConfigDto.PublishEnabled.Value, mqttConfigDto.ClientId);

        settingRepository.UpsertMqttConfig(mqttConfig);
        return NoContent();
    }

    [HttpPut("test")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult TestSettings([BindRequired, FromBody] MqttConfigDto mqttConfigDto)
    {
        var mqttConfig = new MqttConfig(mqttConfigDto.Server, mqttConfigDto.Port.Value, mqttConfigDto.PublishEnabled.Value, mqttConfigDto.ClientId);

        try
        {
            mqttPublisher.Publish(mqttConfig, Array.Empty<Reading>());
            return NoContent();
        }
        catch (MqttException e)
        {
            logger.LogInformation(e, "Test mqtt configuration failed. Server:{Server}, Port:{Port}", mqttConfig.Server, mqttConfig.Port);
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }
}
