using System.ComponentModel.DataAnnotations;

namespace PowerView.Service.Dtos
{
  public class DisconnectRuleSetDto
  {
    public DisconnectRuleSetDto()
    {
      Items = new DisconnectRuleDto[0];
    }

    [Required]
    public DisconnectRuleDto[] Items { get; set; }
  }
}
