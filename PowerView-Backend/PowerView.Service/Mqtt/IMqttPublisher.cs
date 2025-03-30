using System.Collections.Generic;
using PowerView.Model;

namespace PowerView.Service.Mqtt
{
    public interface IMqttPublisher
    {
        void Publish(MqttConfig config, ICollection<Reading> liveReadings);
    }
}
