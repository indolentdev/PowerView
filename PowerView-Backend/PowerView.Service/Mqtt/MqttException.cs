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

    protected MqttException(SerializationInfo info, StreamingContext context)
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

