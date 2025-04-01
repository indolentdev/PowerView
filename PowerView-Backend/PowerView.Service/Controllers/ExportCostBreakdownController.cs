using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/export")]
public class ExportCostBreakdownController : ControllerBase
{
    private readonly ILogger logger;
    private readonly ICostBreakdownRepository costBreakdownRepository;
    private readonly ILocationContext locationContext;

    public ExportCostBreakdownController(ILogger<ExportController> logger, ICostBreakdownRepository costBreakdownRepository, ILocationContext locationContext)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.costBreakdownRepository = costBreakdownRepository ?? throw new ArgumentNullException(nameof(costBreakdownRepository));
        this.locationContext = locationContext ?? throw new ArgumentNullException(nameof(locationContext));
    }

    [HttpGet("costbreakdowntitles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetTitles()
    {
        var titles = costBreakdownRepository.GetCostBreakdownTitles();

        return Ok(titles);
    }

    [HttpGet("costbreakdown/hourly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult GetHourlyCostBreakdownExport(
        [BindRequired, FromQuery, UtcDateTime] DateTime from,
        [BindRequired, FromQuery, UtcDateTime] DateTime to,
        [BindRequired, FromQuery] string title)
    {
        if (from >= to) return BadRequest("to must be greater than from");

        var costBreakdown = costBreakdownRepository.GetCostBreakdown(title);
        if (costBreakdown == null) return NoContent();

        var periods = GetPeriods(from, to);
        var costBreakdownEntries = costBreakdown.Entries.Where(e => e.IntersectsWith(from, to)).ToList();
        var exportCostBreakdown = GetExportCostBreakdown(periods, costBreakdownEntries);

        var r = new
        {
            Title = costBreakdown.Title,
            Currency = costBreakdown.Currency.ToString().ToUpperInvariant(),
            Vat = costBreakdown.Vat,
            Periods = periods.Select(p => new { From = DateTimeMapper.Map(p.From), To = DateTimeMapper.Map(p.To) }).ToList(),
            Entries = exportCostBreakdown
        };
        return Ok(r);
    }

    private static List<ExportPeriod> GetPeriods(DateTime from, DateTime to)
    {
        var hours = new List<ExportPeriod>();
        var hour = TimeSpan.FromHours(1);
        var hourFrom = from;
        var hourTo = from + hour;
        while (hourTo <= to)
        {
            hours.Add(new ExportPeriod(hourFrom, hourTo));
            hourFrom += hour;
            hourTo += hour;
        }

        return hours;
    }

    private List<object> GetExportCostBreakdown(IList<ExportPeriod> periods, IReadOnlyList<CostBreakdownEntry> costBreakdownEntries)
    {
        var exportCostBreakdownEntries = new List<object>(); // matches dto

        foreach (var entry in costBreakdownEntries)
        {
            var completeHourlyValues = new
            {
                entry.Name,
                Values = periods.Select((p, i) =>
                {
                    if (entry.AppliesToDates(p.From, p.To) &&
                      entry.AppliesToTime(TimeOnly.FromDateTime(locationContext.ConvertTimeFromUtc(p.From))))
                    {
                        return new
                        {
                            From = DateTimeMapper.Map(p.From),
                            To = DateTimeMapper.Map(p.To),
                            Value = (double?)entry.Amount,
                        };
                    }

                    return new
                    {
                        From = DateTimeMapper.Map(null),
                        To = DateTimeMapper.Map(null),
                        Value = (double?)null
                    };
                }).ToList()
            };
            exportCostBreakdownEntries.Add(completeHourlyValues);
        }

        return exportCostBreakdownEntries;
    }

}
