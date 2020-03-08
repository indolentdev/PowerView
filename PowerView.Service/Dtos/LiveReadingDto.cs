using System;
using Newtonsoft.Json;

namespace PowerView.Service.Dtos
{
  public class LiveReadingDto
  {
    public LiveReadingDto()
    {
      RegisterValues = new RegisterValueDto[0];
    }

    [JsonProperty(Required = Required.Always)]
    public string Label { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string SerialNumber { get; set; }

    [JsonProperty(Required = Required.Always)]
    public DateTime Timestamp { get; set; }

    [JsonProperty(Required = Required.Always)]
    public RegisterValueDto[] RegisterValues { get; set; }
  }
}