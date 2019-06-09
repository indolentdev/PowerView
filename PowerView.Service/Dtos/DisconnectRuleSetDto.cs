using System;

namespace PowerView.Service.Dtos
{
  public class DisconnectRuleSetDto
  {
    public DisconnectRuleSetDto()
    {
      Items = new DisconnectRuleDto[0];
    }

    public DisconnectRuleDto[] Items { get; set; }
  }
}
