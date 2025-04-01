using System.ComponentModel.DataAnnotations;

namespace PowerView.Service.Dtos
{
    public class DisconnectRuleSetDto
    {
        public DisconnectRuleSetDto()
        {
            Items = Array.Empty<DisconnectRuleDto>();
        }

        [Required]
        public DisconnectRuleDto[] Items { get; set; }
    }
}
