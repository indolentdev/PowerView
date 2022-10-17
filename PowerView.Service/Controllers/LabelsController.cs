using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PowerView.Model.Repository;

namespace PowerView.Service.Controllers;

[ApiController]
[Route("api/labels")]
public class LabelsController : ControllerBase
{
    private readonly ILabelRepository labelRepository;

    public LabelsController(ILabelRepository labelRepository)
    {
        this.labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
    }

    [HttpGet("names")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetLabelNames()
    {
        var labels = labelRepository.GetLabelsByTimestamp();

        return Ok(labels);
    }

}
