/*
using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Nancy;
using Nancy.ModelBinding;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using PowerView.Service.Mqtt;

namespace PowerView.Service.Modules
{
  public class SettingsMqttModule : CommonNancyModule
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ISettingRepository settingRepository;
    private readonly IMqttPublisher mqttPublisher;

    public SettingsMqttModule(ISettingRepository settingRepository, IMqttPublisher mqttPublisher)
      : base("/api/settings/mqtt")
    {
      if (settingRepository == null) throw new ArgumentNullException("settingRepository");
      if (mqttPublisher == null) throw new ArgumentNullException("mqttPublisher");

      this.settingRepository = settingRepository;
      this.mqttPublisher = mqttPublisher;

      Get[""] = GetSettings;
      Put[""] = PutSettings;
      Put["test"] = TestSettings;
    }

    private dynamic GetSettings(dynamic param)
    {
      var mqttConfig = settingRepository.GetMqttConfig();

      var r = new { Server = mqttConfig.Server, Port = mqttConfig.Port.ToString(CultureInfo.InvariantCulture),
        PublishEnabled = mqttConfig.PublishEnabled, ClientId = mqttConfig.ClientId };

      return Response.AsJson(r);
    }

    private dynamic PutSettings(dynamic param)
    {
      var mqttConfig = GetMqttConfig();
      if (mqttConfig == null)
      {
        var description = new { Description = "PublishEnabled, Server, Port or ClientId properties absent or empty" };
        return Response.AsJson(description, HttpStatusCode.UnsupportedMediaType);
      }

      settingRepository.UpsertMqttConfig(mqttConfig);
      return HttpStatusCode.NoContent;
    }

    private MqttConfig GetMqttConfig()
    {
      var mqttConfigDto = this.Bind<MqttConfigDto>();
        
      if (mqttConfigDto.PublishEnabled == null || string.IsNullOrEmpty(mqttConfigDto.Server) || mqttConfigDto.Port == null || mqttConfigDto.ClientId == null)
      {
        log.WarnFormat("Set MQTT configuration failed. Properties are null or empty. PublishEnabled:{0}, Server:{1}, Port:{2}, ClientId:{3}",
                       mqttConfigDto.PublishEnabled, mqttConfigDto.Server, mqttConfigDto.Port, mqttConfigDto.ClientId);
        return null;
      }

      ushort port;
      if (!ushort.TryParse(mqttConfigDto.Port, NumberStyles.Integer, CultureInfo.InvariantCulture, out port)) return null;

      return new MqttConfig(mqttConfigDto.Server, port, mqttConfigDto.PublishEnabled.Value, mqttConfigDto.ClientId);
    }

    private dynamic TestSettings(dynamic param)
    {
      var mqttConfig = GetMqttConfig();
      if (mqttConfig == null)
      {
        var description = new { Description = "PublishEnabled, Server, Port or ClientId properties absent or empty" };
        return Response.AsJson(description, HttpStatusCode.UnsupportedMediaType);
      }

      try
      {
        mqttPublisher.Publish(mqttConfig, new LiveReading[0]);
        return HttpStatusCode.NoContent;
      }
      catch (MqttException e)
      {
        log.Warn("Test mqtt configuration. Server:" + mqttConfig.Server + ", Port:" + mqttConfig.Port, e);
        return HttpStatusCode.ServiceUnavailable;
      }
    }
  }
}
*/