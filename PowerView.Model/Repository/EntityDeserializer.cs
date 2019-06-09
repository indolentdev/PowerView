using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PowerView.Model.Repository
{
  internal class EntityDeserializer : IEntityDeserializer
  {
    private readonly JContainer jContainer;

    public EntityDeserializer(JContainer jContainer)
    {
      if (jContainer == null) throw new ArgumentNullException("jContainer");

      this.jContainer = jContainer;
    }

    public TType GetValue<TType>(params string[] path)
    {
      return GetValue<TType>(jContainer, 0, path);
    }

    private static TType GetValue<TType>(JContainer jContainer, int position, string[] path)
    {
      if (path == null) throw new ArgumentNullException("path");
      if (path.Length == 0) throw new EntitySerializationException("No valid path found. Path:" + string.Join(",", path) + ", Position:" + position + ". Object:" + jContainer);

      var propertyName = path[position];
      var property = jContainer.OfType<JProperty>().Where(p => string.Equals(propertyName, p.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      if (property == null)
      {
        throw new EntitySerializationException("No valid path found. Path:" + string.Join(",", path) + ", Position:" + position + ". Object:" + jContainer);
      }
      var value = property.Value as JValue;
      if (value != null)
      {
        return (TType)value.Value;
      }

      var obj = property.Value as JObject;
      return GetValue<TType>(obj, position+1, path);
    }

  }
}

