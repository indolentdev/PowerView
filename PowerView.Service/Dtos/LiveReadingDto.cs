using System;

namespace PowerView.Service.Dtos
{
  public class LiveReadingDto
  {
    public LiveReadingDto()
    {
      RegisterValues = new RegisterValueDto[0];
    }

    public string Label { get; set; }
    public string SerialNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public RegisterValueDto[] RegisterValues { get; set; }
  }
}