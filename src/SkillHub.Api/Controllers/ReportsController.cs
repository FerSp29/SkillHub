using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Services;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = "Employee")]
public class ReportsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public ReportsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet("top-workshops")]
    public async Task<IActionResult> GetTopWorkshops()
    {
        var report = await _enrollmentService.GetTopWorkshopsReportAsync();
        return Ok(report);
    }
}