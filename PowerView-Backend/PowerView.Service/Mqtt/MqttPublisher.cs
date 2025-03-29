using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Diagnostics;
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

    // TODO: Change Publish to return Task.
    public void Publish(MqttConfig config, ICollection<Reading> liveReadings)
    {
      if (config == null) throw new ArgumentNullException("config");
      if (liveReadings == null) throw new ArgumentNullException("liveReadings");

      PublishInner(config, liveReadings).GetAwaiter().GetResult();
    }

    private async Task PublishInner(MqttConfig config, ICollection<Reading> liveReadings)
    {
      var opts = new MqttClientOptionsBuilder()
        .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
        .WithClientId(config.ClientId)
        .WithCleanSession(true)
        .WithTcpServer(config.Server, config.Port)
        .WithTimeout(config.Timeout)
        .WithNoKeepAlive()
        .Build();

      var mqttNetLogger = new MqttNetLogger(logger);
      using (var mqttClient = new MqttClientFactory().CreateMqttClient(mqttNetLogger))
      {
        mqttClient.ConnectedAsync += e => { logger.LogDebug($"Connected to MQTT server {config.Server}:{config.Port}. ResultCode:{e.ConnectResult.ResultCode}"); return Task.CompletedTask; };
        mqttClient.DisconnectedAsync += e => { logger.LogDebug(e.Exception, "Disconnected MQTT server" + (e.Exception == null ? string.Empty : " with error") + ". WasConnected:" + e.ClientWasConnected); return Task.CompletedTask; };

        try
        {
          await mqttClient.ConnectAsync(opts);
        }
        catch (MqttCommunicationException e)
        {
          throw new ConnectMqttException("MQTT connect Failed", e);
        }
        catch (OperationCanceledException e)
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
        catch (OperationCanceledException e)
        {
          throw new MqttException("MQTT publish or disconnect Failed", e);
        }
      }
    }

  }

}
