using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics;
using MQTTnet.Exceptions;
using PowerView.Model;

namespace PowerView.Service.Mqtt
{
  public class MqttPublisher : IMqttPublisher
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IMqttMapper mqttMapper;

    public MqttPublisher(IMqttMapper mqttMapper)
    {
      if (mqttMapper == null) throw new ArgumentNullException("mqttMapper");

      this.mqttMapper = mqttMapper;
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

    private async Task PublishInner(MqttConfig config, ICollection<LiveReading> liveReadings)
    {
      var opts = new MqttClientOptionsBuilder()
        .WithClientId(string.Empty)
        .WithCleanSession(true)
        .WithTcpServer(config.Server, config.Port)
        .WithCommunicationTimeout(TimeSpan.FromSeconds(1))
        .Build();

      var mqttNetLogger = new MqttNetLogger();
      mqttNetLogger.LogMessagePublished += (sender, e) => log.Debug(e.LogMessage);
      using (var mqttClient = new MqttFactory().CreateMqttClient(mqttNetLogger))
      {
        mqttClient.UseConnectedHandler(e => { log.DebugFormat("Connected to MQTT server {0}:{1}.", config.Server, config.Port); });
        mqttClient.UseDisconnectedHandler(e => { log.Debug("Disconnected MQTT server" + (e.Exception == null ? string.Empty : " with error"), e.Exception); });

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
          await mqttClient.PublishAsync(mqttMessages);
          log.DebugFormat("Published {0} MQTT messages", mqttMessages.Length);
//        System.Threading.Thread.Sleep(postPublishSleepMs); // Allow MQTTnet crap library timing to complete before disconnecting.
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
