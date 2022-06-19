using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;
using PowerView.Service.Mqtt;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/settings/disconnectrules")]
public class SettingsDisconnectRulesController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IDisconnectRuleRepository disconnectRuleRepository;
    private readonly IDisconnectRuleMapper disconnectRuleMapper;

    public SettingsDisconnectRulesController(ILogger<SettingsSerieColorsController> logger, IDisconnectRuleRepository disconnectRuleRepository, IDisconnectRuleMapper disconnectRuleMapper)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.disconnectRuleRepository = disconnectRuleRepository ?? throw new ArgumentNullException(nameof(disconnectRuleRepository));
        this.disconnectRuleMapper = disconnectRuleMapper ?? throw new ArgumentNullException(nameof(disconnectRuleMapper));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetDisconnectRules()
    {
        var disconnectRules = disconnectRuleRepository.GetDisconnectRules();

        var disconnectRuleDtos = disconnectRules.Select(disconnectRuleMapper.MapToDto).ToArray();
        var disconnectRuleSetDto = new DisconnectRuleSetDto { Items = disconnectRuleDtos };

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
            Unit = ValueAndUnitMapper.Map(x.Value, false)
        });
        return Ok(new { DisconnectControlItems = disconnectControlNameDtos, EvaluationItems = evaluationItemDtos });
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public ActionResult AddDisconnectRule([FromBody] DisconnectRuleDto disconnectRuleDto)
    {
        DisconnectRule disconnectRule;
        try
        {
            disconnectRule = disconnectRuleMapper.MapFromDto(disconnectRuleDto);
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, $"Add disconnect rule failed. Invalid content.");
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { Description = "Disconnect rule content invalid" });
        }

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

    [HttpDelete("names/{label}/{obisCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    private dynamic DeleteDisconnectRule([BindRequired, FromQuery] string label, [BindRequired, FromQuery] string obisCode)
    {
        ISeriesName name;
        try
        {
            name = new SeriesName(label, obisCode);
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, $"Delete disconnect rule failed. Bad name. Label:{label}, ObisCode:{obisCode}");
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { Description = "Disconnect rule name invalid" });
        }

        disconnectRuleRepository.DeleteDisconnectRule(name);

        logger.LogInformation($"Deleted DisconnectRule; Label:{name.Label},ObisCode:{name.ObisCode}");

        return NoContent();
    }
}
