using System;
using System.Runtime.Serialization;
//using System.Security.Permissions;

namespace PowerView.Service.Mqtt
{
  [Serializable]
  public class ConnectMqttException : MqttException
  {
    public ConnectMqttException()
    {
    }

    public ConnectMqttException(string message)
      : base(message)
    {
    }

    public ConnectMqttException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected ConnectMqttException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    //    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    //    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    //    {
    //      base.GetObjectData(info, context);
    //    }
  }
}

