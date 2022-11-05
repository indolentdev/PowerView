using System.Collections.Generic;
using PowerView.Model;
using MQTTnet;

namespace PowerView.Service.Mqtt
{
  public interface IMqttMapper
  {
    MqttApplicationMessage[] Map(ICollection<Reading> liveReadings);
  }
}
