using System;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Service.Mappers;
using PowerView.Service.Dtos;

namespace PowerView.Service.Test.Mappers
{
  [TestFixture]
  public class DisconnectRuleMapperTest
  {
    [Test]
    public void MapToDto()
    {
      // Arrange
      var disconnectRule = new DisconnectRule(new SeriesName("Lbl", "0.1.96.3.10.255"), new SeriesName("EvalLabel", ObisCode.ElectrActualPowerP23L2),
                                              TimeSpan.FromMinutes(30), 1500, 200, Unit.Watt);
      var target = new DisconnectRuleMapper();

      // Act
      var disconnectRuleDto = target.MapToDto(disconnectRule);

      // Assert
      Assert.That(disconnectRuleDto.NameLabel, Is.EqualTo(disconnectRule.Name.Label));
      Assert.That(disconnectRuleDto.NameObisCode, Is.EqualTo(disconnectRule.Name.ObisCode.ToString()));
      Assert.That(disconnectRuleDto.EvaluationLabel, Is.EqualTo(disconnectRule.EvaluationName.Label));
      Assert.That(disconnectRuleDto.EvaluationObisCode, Is.EqualTo(disconnectRule.EvaluationName.ObisCode.ToString()));
      Assert.That((double)disconnectRuleDto.DurationMinutes, Is.EqualTo(disconnectRule.Duration.TotalMinutes));
      Assert.That(disconnectRuleDto.DisconnectToConnectValue, Is.EqualTo(disconnectRule.DisconnectToConnectValue));
      Assert.That(disconnectRuleDto.ConnectToDisconnectValue, Is.EqualTo(disconnectRule.ConnectToDisconnectValue));
      Assert.That(disconnectRuleDto.Unit, Is.EqualTo("W"));
    }

    [Test]
    public void MapFromDto()
    {
      // Arrange
      var disconnectRuleDto = new DisconnectRuleDto { NameLabel = "NameLabel", NameObisCode = "1.2.3.4.5.6", EvaluationLabel = "EvalLabel",
        EvaluationObisCode = "6.5.4.3.2.1", DurationMinutes = 60, DisconnectToConnectValue = 1500, ConnectToDisconnectValue = 300,
        Unit = "W" };
      var target = new DisconnectRuleMapper();

      // Act
      var disconnectRule = target.MapFromDto(disconnectRuleDto);

      // Assert
      Assert.That(disconnectRule.Name.Label, Is.EqualTo(disconnectRuleDto.NameLabel));
      Assert.That(disconnectRule.Name.ObisCode.ToString(), Is.EqualTo(disconnectRuleDto.NameObisCode));
      Assert.That(disconnectRule.EvaluationName.Label, Is.EqualTo(disconnectRuleDto.EvaluationLabel));
      Assert.That(disconnectRule.EvaluationName.ObisCode.ToString(), Is.EqualTo(disconnectRuleDto.EvaluationObisCode));
      Assert.That((int)disconnectRule.Duration.TotalMinutes, Is.EqualTo(disconnectRuleDto.DurationMinutes));
      Assert.That(disconnectRule.DisconnectToConnectValue, Is.EqualTo(disconnectRuleDto.DisconnectToConnectValue));
      Assert.That(disconnectRule.ConnectToDisconnectValue, Is.EqualTo(disconnectRuleDto.ConnectToDisconnectValue));
      Assert.That(disconnectRule.Unit, Is.EqualTo(Unit.Watt));
    }


  }
}
