using PowerView.Model;
using PowerView.Service.Dtos;

namespace PowerView.Service.Mappers
{
  public interface IDisconnectRuleMapper
  {
    DisconnectRuleDto MapToDto(IDisconnectRule disconnectRule);

    DisconnectRule MapFromDto(DisconnectRuleDto dto);
  }
}
