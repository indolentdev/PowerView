using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics.Logger;
using MQTTnet.Exceptions;
using PowerView.Model;

namespace PowerView.Service.Mqtt
{
  public class MqttPublisher : IMqttPublisher
  {
    private readonly ILogger logger;
    private readonly IMqttMapper mqttMapper;

    public MqttPublisher(ILogger<MqttPublisher> logger, IMqttMapper mqttMapper)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.mqttMapper = mqttMapper ?? throw new ArgumentNullException(nameof(mqttMapper));
    }

    public void Publish(MqttConfig config, ICollection<LiveReading> liveReadings)
    {
      if (config == null) throw new ArgumentNullException("config");
      if (liveReadings == null) throw new ArgumentNullException("liveReadings");

      try
      {
        PublishInner(config, liveReadings).Wait();
      }
      catch (AggregateException e)
      {
        if (e.InnerException is ConnectMqttException)
        {
          throw new ConnectMqttException("Publish failed. Could not connect", e);
        }
        throw new MqttException("Publish failed", e);
      }
    }

    private void LogMqtt(MqttNetLogMessage logMessage)
    {
      if (logMessage.Level == MqttNetLogLevel.Error)
      {
        logger.LogWarning(logMessage.ToString()); 
      }
      else 
      {
        logger.LogDebug(logMessage.ToString());
      }
    }

    private async Task PublishInner(MqttConfig config, ICollection<LiveReading> liveReadings)
    {
      var opts = new MqttClientOptionsBuilder()
        .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
        .WithClientId(config.ClientId)
        .WithCleanSession(true)
        .WithTcpServer(config.Server, config.Port)
        .WithCommunicationTimeout(config.Timeout)
        .WithNoKeepAlive()
        .Build();

      var mqttNetLogger = new MqttNetEventLogger();
      mqttNetLogger.LogMessagePublished += (sender, e) => LogMqtt(e.LogMessage);
      using (var mqttClient = new MqttFactory().CreateMqttClient(mqttNetLogger))
      {
        mqttClient.UseConnectedHandler(e => { logger.LogDebug("Connected to MQTT server {0}:{1}. ResultCode:{2}", config.Server, config.Port, e.ConnectResult.ResultCode ); });
        mqttClient.UseDisconnectedHandler(e => { logger.LogDebug(e.Exception, "Disconnected MQTT server" + (e.Exception == null ? string.Empty : " with error")); });

        try
        {
          await mqttClient.ConnectAsync(opts);
        }
        catch (MqttCommunicationException e)
        {
          throw new ConnectMqttException("MQTT connect Failed", e);
        }

        try
        {
          var mqttMessages = mqttMapper.Map(liveReadings);
          foreach (var mqttMessage in mqttMessages)
          {
            var publishResult = await mqttClient.PublishAsync(mqttMessage);
            logger.LogDebug("Published MQTT message. PacketIdentifier:{0}, ReasonCode:{1}", publishResult.PacketIdentifier, publishResult.ReasonCode);
          }
          await mqttClient.DisconnectAsync();
        }
        catch (MqttCommunicationException e)
        {
          throw new MqttException("MQTT publish or disconnect Failed", e);
        }
      }
    }


  }

}
