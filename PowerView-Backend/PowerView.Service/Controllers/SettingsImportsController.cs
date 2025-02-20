using System.ComponentModel.DataAnnotations;
using System.Text;
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
[Route("api/settings/imports")]
public class SettingsImportsController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IImportRepository importRepository;

    public SettingsImportsController(ILogger<SettingsImportsController> logger, IImportRepository importRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.importRepository = importRepository ?? throw new ArgumentNullException(nameof(importRepository));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetImports()
    {
        var imports = importRepository.GetImports();

        var r = new
        {
            Imports = imports
                .Select(i => new
                {
                    Label = i.Label,
                    Channel = i.Channel,
                    Currency = i.Currency.ToString().ToUpperInvariant(),
                    FromTimestamp = DateTimeMapper.Map(i.FromTimestamp),
                    CurrentTimestamp = DateTimeMapper.Map(i.CurrentTimestamp != null ? i.CurrentTimestamp.Value.AddHours(-1) : null),
                    Enabled = i.Enabled
                })
                .ToList()
        };
        
        return Ok(r);
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PostImport([BindRequired, FromBody] ImportCreateDto dto)
    {
        var import = new Import(dto.Label, dto.Channel, dto.Currency.Value, dto.FromTimestamp.Value, null, true);

        try
        {
            importRepository.AddImport(import);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, $"Add import failed. Label:{dto.Label}");
            return StatusCode(StatusCodes.Status409Conflict, "Import label already exists");
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("{label}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult DeleteImport(
        [BindRequired, FromRoute, StringLength(25, MinimumLength = 1)] string label)
    {
        importRepository.DeleteImport(label);

        return NoContent();
    }

    [HttpPatch("{label}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult EnableImport(
        [BindRequired, FromRoute, StringLength(25, MinimumLength = 1)] string label,
        [BindRequired, FromBody] ImportEnableDto dto)
    {
        importRepository.SetEnabled(label, dto.Enabled.Value);

        return NoContent();
    }

}
