using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using PowerView.Model.Repository;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/obis")]
public class ObisController : ControllerBase
{
    private readonly ILiveReadingRepository liveReadingRepository;

    public ObisController(ILiveReadingRepository liveReadingRepository)
    {
        this.liveReadingRepository = liveReadingRepository ?? throw new ArgumentNullException(nameof(liveReadingRepository));
    }

    [HttpGet("codes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetObisCodes([BindRequired, FromQuery] string label)
    {
        var obisCodes = liveReadingRepository.GetObisCodes(label, DateTime.UtcNow - TimeSpan.FromDays(365));

        var r = new { ObisCodes = obisCodes.Select(x => x.ToString()).ToList() };
        return Ok(r);
    }

}
