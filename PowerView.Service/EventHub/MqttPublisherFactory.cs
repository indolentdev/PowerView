using System;
using System.Collections.Generic;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mqtt;

namespace PowerView.Service.EventHub
{
  internal class MqttPublisherFactory :IMqttPublisherFactory
  {
    private readonly IFactory factory;

    public MqttPublisherFactory(IFactory factory)
    {
      if (factory == null) throw new ArgumentNullException("factory");

      this.factory = factory;
    }

    public void Publish(IList<LiveReading> liveReadings)
    {
      if (liveReadings == null) throw new ArgumentNullException("liveReadings");
      if (liveReadings.Count == 0)
      {
        return;
      }

      MqttConfig mqttConfig;
      using (var ownedRepo = factory.Create<ISettingRepository>())
      {
        mqttConfig = ownedRepo.Value.GetMqttConfig();
      }

      if (!mqttConfig.PublishEnabled)
      {
        return;
      }

      using (var ownedMqttPublisher = factory.Create<IMqttPublisher>())
      {
        ownedMqttPublisher.Value.Publish(mqttConfig, liveReadings);
      }
    }
  }
}
