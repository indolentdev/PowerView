using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace PowerView.Model.Repository
{
  public static class MeterEventAmplificationSerializer
  {
    public static string Serialize(IMeterEventAmplification amplification)
    {
      if (amplification == null) throw new ArgumentNullException("amplification");

      try
      {
        var amplificationJson = JsonConvert.SerializeObject(amplification);
        var envelope = new Envelope { TypeName = amplification.GetType().Name, Content = amplificationJson };
        return JsonConvert.SerializeObject(envelope);
      }
      catch (JsonException e)
      {
        throw new EntitySerializationException("Failed serializing meter event amplification. Type:" + amplification.GetType().Name, e);
      }
    }

    public static IMeterEventAmplification Deserialize(string value)
    {
      if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");

      Envelope envelope;
      try
      {
        envelope = JsonConvert.DeserializeObject<Envelope>(value);
      }
      catch (JsonException e)
      {
        throw new EntitySerializationException("Failed to deserialize envelope:" + value, e);
      }

      var type = GetType(envelope.TypeName);
      if (type == null)
      {
        throw new EntitySerializationException("Failed to resolve type for envelope:" + value);
      }

      if (string.IsNullOrEmpty(envelope.Content))
      {
        throw new EntitySerializationException("Content absent for envelope:" + value);
      }
      object obj;
      try 
      {
        obj = JsonConvert.DeserializeObject(envelope.Content);
      }
      catch (JsonException e)
      {
        throw new EntitySerializationException("Failed to deserialize content for envelope:" + value, e);
      }
      var jContainer = obj as Newtonsoft.Json.Linq.JContainer;
      if (jContainer == null)
      {
        throw new EntitySerializationException("Unable to handle content object:" + (obj != null ? obj.GetType().Name : "null") + " for envelope:" + value);
      }

      var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance, 
        null, new[] { typeof(IEntityDeserializer) }, null);
      if (constructor == null)
      {
        throw new EntitySerializationException("Failed to find appropriate constructor for type:" + type.Name);
      }
    
      var serializer = new EntityDeserializer(jContainer);
      return (IMeterEventAmplification)constructor.Invoke(new object[] { serializer });
    }

    private static Type GetType(string name)
    {
      var interfaceType = typeof(IMeterEventAmplification);

      var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetTypes()).SelectMany(t => t)
        .Where(t => t.Name == name)
        .Where(t => t.GetInterfaces().Contains(interfaceType)).FirstOrDefault();

      return type;
    }

    private class Envelope
    {
      public string TypeName { get; set; }
      public string Content { get; set; }
    }
  }
}
