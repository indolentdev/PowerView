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
[Route("api/settings/generators")]
public class SettingsGeneratorsSeriesController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IGeneratorSeriesRepository generatorSeriesRepository;

    public SettingsGeneratorsSeriesController(ILogger<SettingsGeneratorsSeriesController> logger, IGeneratorSeriesRepository generatorSeriesRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.generatorSeriesRepository = generatorSeriesRepository ?? throw new ArgumentNullException(nameof(generatorSeriesRepository));
    }

    [HttpGet("series")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetGeneratorsSeries()
    {
        var generatorsSeries = generatorSeriesRepository.GetGeneratorSeries();

        var r = new
        {
            Items = generatorsSeries
                .Select(gs => new
                {
                    NameLabel = gs.Series.Label,
                    NameObisCode = gs.Series.ObisCode.ToString(),
                    BaseLabel = gs.BaseSeries.Label,
                    BaseObisCode = gs.BaseSeries.ObisCode.ToString(),
                    CostBreakdownTitle = gs.CostBreakdownTitle
                })
                .ToList()
        };

        return Ok(r);
    }

    [HttpPost("series")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PostGeneratorsSeries([BindRequired, FromBody] GeneratorSeriesDto dto)
    {
        var generatorSeries = new GeneratorSeries(new SeriesName(dto.NameLabel, dto.NameObisCode), new SeriesName(dto.BaseLabel, dto.BaseObisCode), dto.CostBreakdownTitle);

        try
        {
            generatorSeriesRepository.AddGeneratorSeries(generatorSeries);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, "Add generator series failed. Label:{Label}, ObisCode:{ObisCode}", dto.NameLabel, dto.NameObisCode);
            return StatusCode(StatusCodes.Status409Conflict, "Generator series label and obis code already exists");
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("series/{label}/{obisCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult DeleteGeneratorsSeries(
        [BindRequired, FromRoute, StringLength(25, MinimumLength = 1)] string label,
        [BindRequired, FromRoute, ObisCode] string obisCode)
    {
        generatorSeriesRepository.DeleteGeneratorSeries(new SeriesName(label, obisCode));

        return NoContent();
    }

    [HttpGet("bases/series")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetGeneratorsBasesSeries()
    {
        var baseSeries = generatorSeriesRepository.GetBaseSeries();

        var r = new
        {
            Items = baseSeries
                .Select(bs => new
                {
                    ObisCode = ObisCode.ElectrActiveEnergyKwhIncomeExpenseInclVat.ToString(),
                    BaseLabel = bs.BaseSeries.Label,
                    BaseObisCode = bs.BaseSeries.ObisCode.ToString(),
                    LatestTimestamp = bs.LatestTimestamp.ToString("o")
                })
                .ToList()
        };

        return Ok(r);
    }
}
