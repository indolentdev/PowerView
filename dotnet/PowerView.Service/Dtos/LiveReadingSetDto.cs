using Newtonsoft.Json;

namespace PowerView.Service.Dtos
{
  public class LiveReadingSetDto
  {
    public LiveReadingSetDto()
    {
      Items = new LiveReadingDto[0];
    }

    [JsonProperty(Required = Required.Always)]
    public LiveReadingDto[] Items { get; set; }
  }
}

