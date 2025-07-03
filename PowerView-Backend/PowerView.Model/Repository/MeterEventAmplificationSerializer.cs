using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PowerView.Model.Repository
{
    public static class MeterEventAmplificationSerializer
    {

        public static string Serialize(IMeterEventAmplification amplification)
        {
            ArgumentNullException.ThrowIfNull(amplification);

            try
            {
                var amplificationJson = JsonSerializer.Serialize((object)amplification);
                var envelope = new Envelope { TypeName = amplification.GetType().Name, Content = amplificationJson };
                return JsonSerializer.Serialize(envelope);
            }
            catch (Exception e) when (e is JsonException || e is NotSupportedException)
            {
                throw new EntitySerializationException("Failed serializing meter event amplification. Type:" + amplification.GetType().Name, e);
            }
        }

        public static IMeterEventAmplification Deserialize(string value)
        {
            ArgCheck.ThrowIfNullOrEmpty(value);

            Envelope envelope;
            try
            {
                envelope = JsonSerializer.Deserialize<Envelope>(value/*, options*/);
            }
            catch (JsonException e)
            {
                throw new EntitySerializationException("Failed to deserialize envelope:" + value, e);
            }

            if (envelope == null)
            {
                throw new EntitySerializationException("Envelope deserialization returned null for value:" + value);
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

            JsonNode contentNode;
            try
            {
                contentNode = JsonNode.Parse(envelope.Content);
            }
            catch (JsonException e)
            {
                throw new EntitySerializationException("Failed to deserialize content for envelope:" + value, e);
            }
            if (contentNode == null)
            {
                throw new EntitySerializationException("Content deserialization returned null for envelope:" + value);
            }

            var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance,
                null, new[] { typeof(IEntityDeserializer) }, null);
            if (constructor == null)
            {
                throw new EntitySerializationException("Failed to find appropriate constructor for type:" + type.Name);
            }

            var serializer = new EntityDeserializer(contentNode);
            try
            {
                return (IMeterEventAmplification)constructor.Invoke(new object[] { serializer });
            }
            catch (TargetInvocationException e)
            {
                throw new EntitySerializationException($"Failed to invoke constructor for type:{envelope.TypeName}. Content:{envelope.Content}", e);
            }
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
