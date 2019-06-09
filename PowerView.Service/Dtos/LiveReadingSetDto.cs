using System;

namespace PowerView.Service.Dtos
{
  public class LiveReadingSetDto
  {
    public LiveReadingSetDto()
    {
      Items = new LiveReadingDto[0];
    }

    public LiveReadingDto[] Items { get; set; }
  }
}

