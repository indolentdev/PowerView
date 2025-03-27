using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PowerView.Model.Repository
{
  internal class EntityDeserializer : IEntityDeserializer
  {
    private readonly JsonNode jsonNode;

    public EntityDeserializer(JsonNode jsonNode)
    {
      if (jsonNode == null) throw new ArgumentNullException(nameof(jsonNode));

      this.jsonNode = jsonNode;
    }

    public TType GetValue<TType>(params string[] path)
    {
      return GetValue<TType>(jsonNode, 0, path);
    }

    private static TType GetValue<TType>(JsonNode node, int position, string[] path)
    {
      if (path == null) throw new ArgumentNullException(nameof(path));
      if (path.Length == 0) throw new EntitySerializationException("No valid path found. Path:" + string.Join(",", path) + ", Position:" + position + ". Object:" + node);

      var propertyName = path[position];
      var property = node[propertyName];
      if (property == null)
      {
        throw new EntitySerializationException("No valid path found. Path:" + string.Join(",", path) + ", Position:" + position + ". Object:" + node);
      }

      if (property is JsonValue jsonValue)
      {
        return jsonValue.GetValue<TType>();
      }

      if (property is JsonObject jsonObject)
      {
        return GetValue<TType>(jsonObject, position + 1, path);
      }

      throw new EntitySerializationException("Unexpected node type encountered. Path:" + string.Join(",", path) + ", Position:" + position + ". Object:" + node);
    }
  }
}

