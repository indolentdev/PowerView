using System;
using PowerView.Model;
using PowerView.Service.Dtos;

namespace PowerView.Service.Mappers
{
  internal class DisconnectRuleMapper : IDisconnectRuleMapper
  {
    public DisconnectRuleDto MapToDto(IDisconnectRule disconnectRule)
    {
      return new DisconnectRuleDto
      {
        NameLabel = disconnectRule.Name.Label,
        NameObisCode = disconnectRule.Name.ObisCode.ToString(),
        EvaluationLabel = disconnectRule.EvaluationName.Label,
        EvaluationObisCode = disconnectRule.EvaluationName.ObisCode.ToString(),
        DurationMinutes = (int)disconnectRule.Duration.TotalMinutes,
        DisconnectToConnectValue = disconnectRule.DisconnectToConnectValue,
        ConnectToDisconnectValue = disconnectRule.ConnectToDisconnectValue,
        Unit = ValueAndUnitMapper.Map(disconnectRule.Unit, false)
      };
    }

    public DisconnectRule MapFromDto(DisconnectRuleDto dto)
    {
      try
      {
        var name = new SeriesName(dto.NameLabel, dto.NameObisCode);
        var evaluation = new SeriesName(dto.EvaluationLabel, dto.EvaluationObisCode);
        var duration = TimeSpan.FromMinutes(dto.DurationMinutes);
        return new DisconnectRule(name, evaluation, duration, dto.DisconnectToConnectValue, dto.ConnectToDisconnectValue,
                                  ValueAndUnitMapper.Map(dto.Unit));
      }
      catch (ArgumentException e)
      {
        throw new ArgumentException("Unable to map dto to DisconnectRule", "dto", e);
      }
    }
  }
}

