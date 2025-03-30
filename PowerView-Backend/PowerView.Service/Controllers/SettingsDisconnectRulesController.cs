using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/settings/disconnectrules")]
public class SettingsDisconnectRulesController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IDisconnectRuleRepository disconnectRuleRepository;

    public SettingsDisconnectRulesController(ILogger<SettingsSerieColorsController> logger, IDisconnectRuleRepository disconnectRuleRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.disconnectRuleRepository = disconnectRuleRepository ?? throw new ArgumentNullException(nameof(disconnectRuleRepository));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetDisconnectRules()
    {
        var disconnectRules = disconnectRuleRepository.GetDisconnectRules();

        var disconnectRuleDtos = disconnectRules.Select(disconnectRule => new
        {
            NameLabel = disconnectRule.Name.Label,
            NameObisCode = disconnectRule.Name.ObisCode.ToString(),
            EvaluationLabel = disconnectRule.EvaluationName.Label,
            EvaluationObisCode = disconnectRule.EvaluationName.ObisCode.ToString(),
            DurationMinutes = (int)disconnectRule.Duration.TotalMinutes,
            DisconnectToConnectValue = disconnectRule.DisconnectToConnectValue,
            ConnectToDisconnectValue = disconnectRule.ConnectToDisconnectValue,
            Unit = ValueAndUnitConverter.Convert(disconnectRule.Unit, false)
        })
            .ToArray();
        var disconnectRuleSetDto = new { Items = disconnectRuleDtos };

        return Ok(disconnectRuleSetDto);
    }

    [HttpGet("options")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetDisconnectControlOptions()
    {
        var latestSerieNames = disconnectRuleRepository.GetLatestSerieNames(DateTime.UtcNow);

        var disconnectRules = disconnectRuleRepository.GetDisconnectRules();
        var disconnectControlNames = latestSerieNames.Keys.Where(x => x.ObisCode.IsDisconnectControl)
                                                 .Except(disconnectRules.Select(x => x.Name)).ToList();

        var evaluationObisCodes = new[] { ObisCode.ElectrActualPowerP23, ObisCode.ElectrActualPowerP23L1, ObisCode.ElectrActualPowerP23L2, ObisCode.ElectrActualPowerP23L3 };
        var evaluationItems = latestSerieNames.Where(x => evaluationObisCodes.Contains(x.Key.ObisCode)).ToList();

        var disconnectControlNameDtos = disconnectControlNames.Select(x => new { Label = x.Label, ObisCode = x.ObisCode.ToString() }).ToList();
        var evaluationItemDtos = evaluationItems.Select(x => new
        {
            Label = x.Key.Label,
            ObisCode = x.Key.ObisCode.ToString(),
            Unit = ValueAndUnitConverter.Convert(x.Value, false)
        });
        return Ok(new { DisconnectControlItems = disconnectControlNameDtos, EvaluationItems = evaluationItemDtos });
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult AddDisconnectRule([BindRequired, FromBody] DisconnectRuleDto disconnectRuleDto)
    {
        var disconnectRule = MapFromDto(disconnectRuleDto);

        try
        {
            disconnectRuleRepository.AddDisconnectRule(disconnectRule);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, $"Add disconnect rule failed. Unique constraint violation.");
            return StatusCode(StatusCodes.Status409Conflict, new { Description = "Disconnect rule unique constraint violation" });
        }

        logger.LogInformation($"Inserted/Updated DisconnectRule; Label:{disconnectRule.Name.Label},ObisCode:{disconnectRule.Name.ObisCode}");

        return NoContent();
    }

    public DisconnectRule MapFromDto(DisconnectRuleDto dto)
    {
        var name = new SeriesName(dto.NameLabel, dto.NameObisCode);
        var evaluation = new SeriesName(dto.EvaluationLabel, dto.EvaluationObisCode);
        var duration = TimeSpan.FromMinutes(dto.DurationMinutes.Value);
        return new DisconnectRule(name, evaluation, duration, dto.DisconnectToConnectValue.Value, dto.ConnectToDisconnectValue.Value,
                                  ToUnit(dto.Unit.Value));
    }

    private Unit ToUnit(DisconnectRuleUnit disconnectRuleUnit)
    {
        switch (disconnectRuleUnit)
        {
            case DisconnectRuleUnit.W:
                return Unit.Watt;
            default:
                throw new NotImplementedException($"DisconnectRuleUnit value not supported:{disconnectRuleUnit}");
        }
    }


    [HttpDelete("names/{label}/{obisCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult DeleteDisconnectRule([BindRequired, FromRoute] string label, [BindRequired, FromRoute, ObisCode] string obisCode)
    {
        var name = new SeriesName(label, obisCode);

        disconnectRuleRepository.DeleteDisconnectRule(name);

        logger.LogInformation($"Deleted DisconnectRule; Label:{name.Label},ObisCode:{name.ObisCode}");

        return NoContent();
    }
}
