using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.Mappers;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/data")]
public class DataController : ControllerBase
{
    private readonly ILogger logger;
    private readonly IReadingHistoryRepository readingHistoryRepository;

    public DataController(ILogger<DataController> logger, IReadingHistoryRepository readingHistoryRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.readingHistoryRepository = readingHistoryRepository ?? throw new ArgumentNullException(nameof(readingHistoryRepository));
    }

    [HttpGet("history/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetHistoryStatus()
    {
        var readingHistoryStatus = readingHistoryRepository.GetReadingHistoryStatus();

        var r = new
        {
            Items = readingHistoryStatus.Select(x => new
            {
                Interval = x.Interval,
                LabelTimestamps = x.Status.Select(y => new
                {
                    Label = y.Label,
                    LatestTimestamp = y.LatestTimestamp.ToString("o")
                }).ToList()
            }).ToList()
        };

        return Ok(r);
    }

}
