using System;

namespace PowerView.Service.Dtos
{
  public class SerieColorSetDto
  {
    public SerieColorSetDto()
    {
      Items = new SerieColorDto[0];
    }

    public SerieColorDto[] Items { get; set; }
  }
}
