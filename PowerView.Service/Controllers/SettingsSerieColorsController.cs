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
[Route("api/settings/seriecolors")]
public class SettingsSerieColorsController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ISeriesColorRepository serieColorRepository;
    private readonly ISeriesNameProvider serieNameProvider;
    private readonly IObisColorProvider obisColorProvider;

    public SettingsSerieColorsController(ILogger<SettingsSerieColorsController> logger, ISeriesColorRepository serieColorRepository, ISeriesNameProvider serieNameProvider, IObisColorProvider obisColorProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.serieColorRepository = serieColorRepository ?? throw new ArgumentNullException(nameof(serieColorRepository));
        this.serieNameProvider = serieNameProvider ?? throw new ArgumentNullException(nameof(serieNameProvider));
        this.obisColorProvider = obisColorProvider ?? throw new ArgumentNullException(nameof(obisColorProvider));
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetSeriesColors()
    {
        var seriesColorsDb = serieColorRepository.GetSeriesColors();
        var seriesColors = serieNameProvider.GetSeriesNames()
          .ToDictionary(sn => sn, sn => new SeriesColor(new SeriesName(sn.Label, sn.ObisCode), obisColorProvider.GetColor(sn.ObisCode)));

        foreach (var seriesColor in seriesColorsDb)
        {
            seriesColors[seriesColor.SeriesName] = seriesColor;
        }

        var items = seriesColors.Values.Select(sc => new SerieColorDto
        {
            Label = sc.SeriesName.Label,
            ObisCode = sc.SeriesName.ObisCode.ToString(),
            Color = sc.Color
        })
        .OrderBy(x => x.Label).ThenBy(x => x.ObisCode);
        var r = new SerieColorSetDto { Items = items.ToArray() };
        return Ok(r);
    }

    [HttpPut("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult PutSeriesColors([FromBody] SerieColorSetDto seriesColorSetDto)
    {
        var seriesColors = ToSeriesColors(seriesColorSetDto.Items).ToArray();
        if (seriesColors.Length > 0)
        {
            serieColorRepository.SetSeriesColors(seriesColors);
        }
        return StatusCode(StatusCodes.Status204NoContent);
    }

    private IEnumerable<SeriesColor> ToSeriesColors(IEnumerable<SerieColorDto> seriesColorDtos)
    {
        foreach (var seriesColorDto in seriesColorDtos)
        {
            if (!SeriesColor.IsColorValid(seriesColorDto.Color))
            {
                logger.LogInformation($"Skipping serie color item having invalid color format {seriesColorDto.Label} {seriesColorDto.ObisCode} {seriesColorDto.Color}");
                continue;
            }

            yield return new SeriesColor(new SeriesName(seriesColorDto.Label, seriesColorDto.ObisCode), seriesColorDto.Color);
        }
    }

}
