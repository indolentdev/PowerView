using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mqtt;

namespace PowerView.Service.EventHub
{
  internal class MqttPublisherFactory : IMqttPublisherFactory
  {
    public void Publish(IServiceScope serviceScope, IList<LiveReading> liveReadings)
    {
      if (serviceScope == null) throw new ArgumentNullException(nameof(serviceScope));
      if (liveReadings == null) throw new ArgumentNullException("liveReadings");

      if (liveReadings.Count == 0)
      {
        return;
      }

      MqttConfig mqttConfig;
      var settingsRepository = serviceScope.ServiceProvider.GetRequiredService<ISettingRepository>();
      mqttConfig = settingsRepository.GetMqttConfig();

      if (!mqttConfig.PublishEnabled)
      {
        return;
      }

      var mqttPublisher = serviceScope.ServiceProvider.GetRequiredService<IMqttPublisher>();
      mqttPublisher.Publish(mqttConfig, liveReadings);
    }
  }
}
