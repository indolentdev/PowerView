using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/settings/profilegraphs")]
public class SettingsProfileGraphsController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ISeriesNameProvider serieNameProvider;
    private readonly IProfileGraphRepository profileGraphRepository;

    public SettingsProfileGraphsController(ILogger<SettingsProfileGraphsController> logger, ISeriesNameProvider serieNameProvider, IProfileGraphRepository profileGraphRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.serieNameProvider = serieNameProvider ?? throw new ArgumentNullException(nameof(serieNameProvider));
        this.profileGraphRepository = profileGraphRepository ?? throw new ArgumentNullException(nameof(profileGraphRepository));
    }

    [HttpGet("series")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetProfileGraphSeries()
    {
        var serieNames = serieNameProvider.GetSeriesNames();

        var day = serieNames.Where(sn => !sn.ObisCode.IsDelta || sn.ObisCode == ObisCode.ElectrActiveEnergyA14NetDelta || sn.ObisCode == ObisCode.ElectrActiveEnergyA23NetDelta)
          .Select(sn => new { Period = "day", sn.Label, ObisCode = sn.ObisCode.ToString() });
        var month = serieNames.Where(sn => sn.ObisCode.IsDelta || sn.ObisCode.IsPeriod)
          .Select(sn => new { Period = "month", sn.Label, ObisCode = sn.ObisCode.ToString() });
        var year = serieNames.Where(sn => sn.ObisCode.IsDelta || sn.ObisCode.IsPeriod)
          .Select(sn => new { Period = "year", sn.Label, ObisCode = sn.ObisCode.ToString() });
        var decade = serieNames.Where(sn => sn.ObisCode.IsDelta || sn.ObisCode.IsPeriod)
          .Select(sn => new { Period = "decade", sn.Label, ObisCode = sn.ObisCode.ToString() });

        var r = new { Items = day.Concat(month).Concat(year).Concat(decade).ToList() };
        return Ok(r);
    }

    [HttpGet("pages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult GetProfileGraphPages([BindRequired, FromQuery, StringLength(6, MinimumLength = 3)] string period)
    {
        var pages = profileGraphRepository.GetProfileGraphPages(period);

        var r = new { Items = pages };
        return Ok(r);
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetProfileGraphs()
    {
        var profileGraphs = profileGraphRepository.GetProfileGraphs();

        var r = new
        {
            Items = profileGraphs
                .Select(p => new
                {
                    Title = p.Title,
                    Page = p.Page,
                    Period = p.Period,
                    Interval = p.Interval,
                    Series = p.SerieNames.Select(sn => new { Label = sn.Label, ObisCode = sn.ObisCode.ToString() }).ToList()
                })
                .ToList()
        };
        return Ok(r);
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PostProfileGraph([BindRequired, FromBody] ProfileGraphDto dto)
    {
        var serieNames = dto.Series.Select(x => new SeriesName(x.Label, x.ObisCode)).ToList();
        var profileGraph = new ProfileGraph(dto.Period, dto.Page, dto.Title, dto.Interval, 0, serieNames);

        try
        {
            profileGraphRepository.AddProfileGraph(profileGraph);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, $"Add profile graph failed. Period:{dto.Period}, Page:{dto.Page}, Title:{dto.Title}");
            return StatusCode(StatusCodes.Status409Conflict, "ProfileGraph [period, page, title] or [period, page, rank] already exists");
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPut("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PutProfileGraph(
        [BindRequired, FromQuery, StringLength(20, MinimumLength = 1)] string period,
        [BindRequired, FromQuery, StringLength(32, MinimumLength = 0)] string page,
        [BindRequired, FromQuery, StringLength(32, MinimumLength = 1)] string title,
        [BindRequired, FromBody] ProfileGraphDto dto)
    {
        var serieNames = dto.Series.Select(x => new SeriesName(x.Label, x.ObisCode)).ToList();
        var profileGraph = new ProfileGraph(dto.Period, dto.Page, dto.Title, dto.Interval, 0, serieNames);

        var success = profileGraphRepository.UpdateProfileGraph(period, page, title, profileGraph);
        if (!success)
        {
            logger.LogWarning($"Update profile graph failed. Period:{period}, Page:{page}, Title:{title}. Does not exist.");
            return StatusCode(StatusCodes.Status409Conflict, new { Description = "ProfileGraph [period, page, title] does not exist" });
        }

        return NoContent();
    }

    [HttpDelete("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult DeleteProfileGraph(
        [BindRequired, FromQuery, StringLength(20, MinimumLength = 1)] string period,
        [BindRequired, FromQuery, StringLength(32, MinimumLength = 0)] string page,
        [BindRequired, FromQuery, StringLength(32, MinimumLength = 1)] string title)
    {
        profileGraphRepository.DeleteProfileGraph(period, page, title);

        return NoContent();
    }

    [HttpPut("swaprank")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult SwapProfileGraphRank(
        [BindRequired, FromQuery, StringLength(20, MinimumLength = 1)] string period,
        [BindRequired, FromQuery, StringLength(32, MinimumLength = 0)] string page,
        [BindRequired, FromQuery, StringLength(32, MinimumLength = 1)] string title1,
        [BindRequired, FromQuery, StringLength(32, MinimumLength = 1)] string title2)
    {
        try
        {
            profileGraphRepository.SwapProfileGraphRank(period, page, title1, title2);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, $"Swap profile graph rank. Period:{period}, Page:{page}, Title1:{title1}, Title2:{title2}");
            return StatusCode(StatusCodes.Status409Conflict, new { Description = "ProfileGraph [period, page, rank] already exists" });
        }

        return NoContent();
    }

}
