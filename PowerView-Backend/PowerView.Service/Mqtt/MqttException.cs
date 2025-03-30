using System;
using System.Runtime.Serialization;
//using System.Security.Permissions;

namespace PowerView.Service.Mqtt
{
    [Serializable]
    public class MqttException : Exception
    {
        public MqttException()
        {
        }

        public MqttException(string message)
          : base(message)
        {
        }

        public MqttException(string message, Exception inner)
          : base(message, inner)
        {
        }
    }
}

