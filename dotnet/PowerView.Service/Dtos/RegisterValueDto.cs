using Newtonsoft.Json;

namespace PowerView.Service.Dtos
{
  public class RegisterValueDto
  {
    [JsonProperty(Required = Required.Always)]
    public string ObisCode { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Value { get; set; }

    [JsonProperty(Required = Required.Always)]
    public short Scale { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Unit { get; set; }
  }
}
