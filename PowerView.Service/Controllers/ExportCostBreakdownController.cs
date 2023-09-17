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
    public ActionResult GetHourlyCostBreakdownExport(
        [BindRequired, FromQuery, UtcDateTime] DateTime from, 
        [BindRequired, FromQuery, UtcDateTime] DateTime to, 
        [BindRequired, FromQuery] string title)
    {
        if (from >= to) return BadRequest("to must be greater than from");
        
        // TOOD: Check for max length?

        var costBreakdown = costBreakdownRepository.GetCostBreakdown(title);
        // TODO NULL

        var periods = GetPeriods(from, to);

        var exportCostBreakdown = GetExportCostBreakdown(periods, costBreakdown.Entries);

        var r = new { Title = costBreakdown.Title, Currency = costBreakdown.Currency.ToString().ToUpperInvariant(), Vat = costBreakdown.Vat,
          Periods = periods, Entries = exportCostBreakdown };
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

    private IList<object> GetExportCostBreakdown(IList<ExportPeriod> periods, IReadOnlyList<CostBreakdownEntry> costBreakdownEntries)
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
                            From = (DateTime?)p.From,
                            To = (DateTime?)p.To,
                            Value = (double?)entry.Amount,
                        };
                    }

                    return new
                    {
                        From = (DateTime?)null,
                        To = (DateTime?)null,
                        Value = (double?)null
                    };
                }).ToList()
            };
            exportCostBreakdownEntries.Add(completeHourlyValues);
        }

        return exportCostBreakdownEntries;
    }

}
