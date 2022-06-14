using System.Globalization;
using Microsoft.AspNetCore.Mvc;
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
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public ActionResult PutSettings([FromBody] MqttConfigDto mqttConfigDto)
    {
        var mqttConfig = GetMqttConfig(mqttConfigDto);
        if (mqttConfig == null)
        {
            var description = new { Description = "PublishEnabled, Server, Port or ClientId properties absent or empty" };
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, description);
        }

        settingRepository.UpsertMqttConfig(mqttConfig);
        return NoContent();
    }

    private MqttConfig GetMqttConfig(MqttConfigDto mqttConfigDto)
    {
        if (mqttConfigDto.PublishEnabled == null || string.IsNullOrEmpty(mqttConfigDto.Server) || mqttConfigDto.ClientId == null)
        {
            logger.LogWarning($"MQTT configuration failed. Properties are null or empty. PublishEnabled:{mqttConfigDto.PublishEnabled}, Server:{mqttConfigDto.Server}, Port:{mqttConfigDto.Port}, ClientId:{mqttConfigDto.ClientId}");
            return null;
        }

        return new MqttConfig(mqttConfigDto.Server, mqttConfigDto.Port, mqttConfigDto.PublishEnabled.Value, mqttConfigDto.ClientId);
    }

    [HttpPut("test")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult TestSettings([FromBody] MqttConfigDto mqttConfigDto)
    {
        var mqttConfig = GetMqttConfig(mqttConfigDto);
        if (mqttConfig == null)
        {
            var description = new { Description = "PublishEnabled, Server, Port or ClientId properties absent or empty" };
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, description);
        }

        try
        {
            mqttPublisher.Publish(mqttConfig, new LiveReading[0]);
            return NoContent();
        }
        catch (MqttException e)
        {
            logger.LogInformation(e, $"Test mqtt configuration failed. Server:{mqttConfig.Server}, Port:{mqttConfig.Port}");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }
}
