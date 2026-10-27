using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Services;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/workshops")]
public class WorkshopsController : ControllerBase
{
    private readonly IWorkshopService _workshopService;

    public WorkshopsController(IWorkshopService workshopService)
    {
        _workshopService = workshopService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetUpcomingWorkshops()
    {
        var workshops = await _workshopService.GetUpcomingWorkshopsAsync();
        return Ok(workshops);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWorkshopById(Guid id)
    {
        var workshop = await _workshopService.GetWorkshopByIdAsync(id);
        if (workshop == null)
        {
            return NotFound(new { message = "Workshop not found." });
        }
        return Ok(workshop);
    }

    [HttpPost]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> CreateWorkshop([FromBody] CreateWorkshopRequestDto request)
    {
        try
        {
            var created = await _workshopService.CreateWorkshopAsync(request);
            return CreatedAtAction(nameof(GetWorkshopById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/enrollments")]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> GetWorkshopEnrollments(Guid id)
    {
        try
        {
            var enrollments = await _workshopService.GetWorkshopEnrollmentsAsync(id);
            return Ok(enrollments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}