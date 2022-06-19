using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Dtos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/settings/profilegraphs")]
public class SettingsProfileGraphsController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ISeriesNameRepository serieNameRepository;
    private readonly IProfileGraphRepository profileGraphRepository;
    private readonly ILocationContext locationContext;

    public SettingsProfileGraphsController(ILogger<SettingsProfileGraphsController> logger, ISeriesNameRepository serieNameRepository, IProfileGraphRepository profileGraphRepository, ILocationContext locationContext)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.serieNameRepository = serieNameRepository ?? throw new ArgumentNullException(nameof(serieNameRepository));
        this.profileGraphRepository = profileGraphRepository ?? throw new ArgumentNullException(nameof(profileGraphRepository));
        this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
    }

    [HttpGet("series")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetProfileGraphSeries()
    {
        var timeZoneInfo = locationContext.TimeZoneInfo;
        var serieNames = serieNameRepository.GetSeriesNames(timeZoneInfo);

        var day = serieNames.Where(sn => !sn.ObisCode.IsDelta || sn.ObisCode == ObisCode.ElectrActiveEnergyA14NetDelta || sn.ObisCode == ObisCode.ElectrActiveEnergyA23NetDelta)
          .Select(sn => new { Period = "day", sn.Label, ObisCode = sn.ObisCode.ToString() });
        var month = serieNames.Where(sn => sn.ObisCode.IsDelta || sn.ObisCode.IsPeriod)
          .Select(sn => new { Period = "month", sn.Label, ObisCode = sn.ObisCode.ToString() });
        var year = serieNames.Where(sn => sn.ObisCode.IsDelta || sn.ObisCode.IsPeriod)
          .Select(sn => new { Period = "year", sn.Label, ObisCode = sn.ObisCode.ToString() });

        var r = new { Items = day.Concat(month).Concat(year) };
        return Ok(r);
    }

    [HttpGet("pages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult GetProfileGraphPages([BindRequired, FromQuery, StringLength(20, MinimumLength = 1)] string period)
    {
        var pages = profileGraphRepository.GetProfileGraphPages("day");

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
            Items = profileGraphs.Select(ToProfileGraphDto).ToList()
        };
        return Ok(r);
    }

    private static ProfileGraphDto ToProfileGraphDto(ProfileGraph profileGraph)
    {
        return new ProfileGraphDto
        {
            Title = profileGraph.Title,
            Page = profileGraph.Page,
            Period = profileGraph.Period,
            Interval = profileGraph.Interval,
            Rank = profileGraph.Rank,
            Series = profileGraph.SerieNames.Select(sn =>
              new ProfileGraphSerieDto { Label = sn.Label, ObisCode = sn.ObisCode.ToString() }).ToArray()
        };
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public ActionResult PostProfileGraph([FromBody] ProfileGraphDto dto)
    {
        ProfileGraph profileGraph = null;
        try
        {
            var serieNames = dto.Series.Select(x => new SeriesName(x.Label, x.ObisCode)).ToList();
            profileGraph = new ProfileGraph(dto.Period, dto.Page, dto.Title, dto.Interval, 0, serieNames);
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Add profile graph failed");
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        try
        {
            profileGraphRepository.AddProfileGraph(profileGraph);
        }
        catch (DataStoreUniqueConstraintException e)
        {
            logger.LogWarning(e, $"Add profile graph failed. Period:{dto.Period}, Page:{dto.Page}, Title:{dto.Title}");
            return StatusCode(StatusCodes.Status409Conflict, new { Description = "ProfileGraph [period, page, title] or [period, page, rank] already exists" });
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPut("modify/{existingProfileGraphIdBase64}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult PutProfileGraph([BindRequired, FromQuery] string existingProfileGraphIdBase64, [FromBody] ProfileGraphDto dto)
    {
        UpdateProfileGraphId updateProfileGraphId;
        try
        {
            var existingProfileGraphIdJson = Encoding.ASCII.GetString(Convert.FromBase64String(existingProfileGraphIdBase64));
            updateProfileGraphId = JsonConvert.DeserializeObject<UpdateProfileGraphId>(existingProfileGraphIdJson);
        }
        catch (FormatException e)
        {
            logger.LogWarning(e, $"Update profile graph failed. Unable to decode existing profile graph id:{existingProfileGraphIdBase64}");
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { Description = "ProfileGraph [period, page, title] unknown" });
        }
        catch (JsonException e)
        {
            logger.LogWarning(e, $"Update profile graph failed. Unable to decode existing profile graph id:{existingProfileGraphIdBase64}");
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { Description = "ProfileGraph [period, page, title] unknown" });
        }

        ProfileGraph profileGraph = null;
        try
        {
            var serieNames = dto.Series.Select(x => new SeriesName(x.Label, x.ObisCode)).ToList();
            profileGraph = new ProfileGraph(dto.Period, dto.Page, dto.Title, dto.Interval, 0, serieNames);
        }
        catch (ArgumentException e)
        {
            logger.LogWarning(e, "Update profile graph failed.");
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        var success = profileGraphRepository.UpdateProfileGraph(updateProfileGraphId.Period, updateProfileGraphId.Page, updateProfileGraphId.Title, profileGraph);
        if (!success)
        {
            logger.LogWarning($"Update profile graph failed. Period:{updateProfileGraphId.Period}, Page:{updateProfileGraphId.Page}, Title:{updateProfileGraphId.Title}. Does not exist.");
            return StatusCode(StatusCodes.Status409Conflict, new { Description = "ProfileGraph [period, page, title] does not exist" });
        }

        return StatusCode(StatusCodes.Status409Conflict);
    }

    private class UpdateProfileGraphId
    {
        public string Period { get; set; }
        public string Page { get; set; }
        public string Title { get; set; }
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
