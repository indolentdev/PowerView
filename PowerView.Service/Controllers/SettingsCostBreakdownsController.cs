using System.ComponentModel.DataAnnotations;
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

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/settings/costbreakdowns")]
public class SettingsCostBreakdownsController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ICostBreakdownRepository costBreakdownRepository;

    public SettingsCostBreakdownsController(ILogger<SettingsCostBreakdownsController> logger, ICostBreakdownRepository costBreakdownRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.costBreakdownRepository = costBreakdownRepository ?? throw new ArgumentNullException(nameof(costBreakdownRepository));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetCostBreakdowns()
    {
        var costBreakdowns = costBreakdownRepository.GetCostBreakdowns();

        var r = new
        {
            CostBreakdowns = costBreakdowns
                .Select(cb => new
                {
                    Title = cb.Title,
                    Currency = cb.Currency.ToString().ToUpperInvariant(),
                    Vat = cb.Vat,
                    EntryPeriods = cb.GetEntriesByPeriods().OrderByDescending(x => x.Key.FromDate).ThenByDescending(x => x.Key.ToDate).Select(cbeg => new
                    {
                        Period = new
                        {
                            FromDate = DateTimeMapper.Map(cbeg.Key.FromDate),
                            ToDate = DateTimeMapper.Map(cbeg.Key.ToDate),
                        },
                        Entries = cbeg.Value.Select(cbe => new
                        {
                            FromDate = DateTimeMapper.Map(cbe.FromDate),
                            ToDate = DateTimeMapper.Map(cbe.ToDate),
                            Name = cbe.Name,
                            StartTime = cbe.StartTime,
                            EndTime = cbe.EndTime,
                            Amount = cbe.Amount
                        }).ToList(),
                    }).ToList()
                })
                .ToList()
        };
        
        return Ok(r);
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PostCostBreakdown([BindRequired, FromBody] CostBreakdownDto dto)
    {
        var costBreakdown = new CostBreakdown(dto.Title, dto.Currency.Value, dto.Vat.Value, Array.Empty<CostBreakdownEntry>());

        try
        {
            costBreakdownRepository.AddCostBreakdown(costBreakdown);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, $"Add cost breakdown failed. Title:{dto.Title}");
            return StatusCode(StatusCodes.Status409Conflict, "CostBreakdown title already exists");
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("{title}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult DeleteCostBreakdown(
        [BindRequired, FromRoute, StringLength(25, MinimumLength = 1)] string title)
    {
        costBreakdownRepository.DeleteCostBreakdown(title);

        return NoContent();
    }

    [HttpPost("{title}/entries")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PostCostBreakdownEntry(
        [BindRequired, FromRoute, StringLength(25, MinimumLength = 1)] string title,
        [BindRequired, FromBody] CostBreakdownEntryDto dto)
    {
        var costBreakdownEntry = new CostBreakdownEntry(dto.FromDate.Value, dto.ToDate.Value, dto.Name, dto.StartTime.Value, dto.EndTime.Value, dto.Amount.Value);

        try
        {
            costBreakdownRepository.AddCostBreakdownEntry(title, costBreakdownEntry);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, $"Add cost breakdown entry failed. Duplicate. Title:{title}, FromDate:{dto.FromDate.Value.ToString("O")}, ToDate:{dto.ToDate.Value.ToString("O")}, Name:{dto.Name}");
            return StatusCode(StatusCodes.Status409Conflict, "CostBreakdownEntry already exists");
        }
        catch (DataStoreException e)
        {
            logger.LogWarning(e, $"Add cost breakdown entry failed. Title:{title}, FromDate:{dto.FromDate.Value.ToString("O")}, ToDate:{dto.ToDate.Value.ToString("O")}, Name:{dto.Name}");
            return StatusCode(StatusCodes.Status400BadRequest, "CostBreakdownEntry failed");
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPut("{title}/entries/{fromDate}/{toDate}/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PutCostBreakdownEntry(
        [BindRequired, FromRoute, StringLength(25, MinimumLength = 1)] string title,
        [BindRequired, FromRoute, UtcDateTime] DateTime? fromDate,
        [BindRequired, FromRoute, UtcDateTime] DateTime? toDate,
        [BindRequired, FromRoute, StringLength(25, MinimumLength = 1)] string name,
        [BindRequired, FromBody] CostBreakdownEntryDto dto)
    {
        var costBreakdownEntry = new CostBreakdownEntry(dto.FromDate.Value, dto.ToDate.Value, dto.Name, dto.StartTime.Value, dto.EndTime.Value, dto.Amount.Value);

        try
        {
            costBreakdownRepository.UpdateCostBreakdownEntry(title, fromDate.Value, toDate.Value, name, costBreakdownEntry);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, $"Add cost breakdown entry failed. Duplicate. Title:{title}, FromDate:{fromDate.Value.ToString("O")}, ToDate:{toDate.Value.ToString("O")}, Name:{name}");
            return StatusCode(StatusCodes.Status409Conflict, "CostBreakdownEntry already exists");
        }
        catch (DataStoreException e)
        {
            logger.LogWarning(e, $"Add cost breakdown entry failed. Title:{title}, FromDate:{fromDate.Value.ToString("O")}, ToDate:{toDate.Value.ToString("O")}, Name:{name}");
            return StatusCode(StatusCodes.Status400BadRequest, "CostBreakdownEntry failed");
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("{title}/entries/{fromDate}/{toDate}/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult DeleteCostBreakdownEntry(
        [BindRequired, FromRoute, StringLength(25, MinimumLength = 1)] string title,
        [BindRequired, FromRoute, UtcDateTime] DateTime? fromDate,
        [BindRequired, FromRoute, UtcDateTime] DateTime? toDate,
        [BindRequired, FromRoute, StringLength(25, MinimumLength = 1)] string name)
    {
        try
        {
            costBreakdownRepository.DeleteCostBreakdownEntry(title, fromDate.Value, toDate.Value, name);
        }
        catch (DataStoreException e)
        {
            logger.LogWarning(e, $"Delete cost breakdown entry failed. Title:{title}, FromDate:{fromDate.Value.ToString("O")}, ToDate:{toDate.Value.ToString("O")}, Name:{name}");
            return StatusCode(StatusCodes.Status400BadRequest, "CostBreakdownEntry failed");
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

}
