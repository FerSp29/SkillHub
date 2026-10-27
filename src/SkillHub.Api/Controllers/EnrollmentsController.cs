using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Services;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var response = await _enrollmentService.EnrollAsync(userId, request);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(userId);
        return Ok(enrollments);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isEmployee = User.IsInRole("Employee");
            await _enrollmentService.CancelEnrollmentAsync(id, userId, isEmployee);
            return Ok(new { message = "Enrollment cancelled successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("import")]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Please upload a valid Excel file (.xlsx)." });
        }

        try
        {
            var employeeUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            using var stream = file.OpenReadStream();
            var result = await _enrollmentService.ImportFromExcelAsync(stream, employeeUserId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Failed to process Excel: {ex.Message}" });
        }
    }

    [HttpGet("{id:guid}/certificate")]
    public async Task<IActionResult> DownloadCertificate(Guid id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isEmployee = User.IsInRole("Employee");
            var pdfBytes = await _enrollmentService.GenerateCertificateAsync(id, userId, isEmployee);

            return File(pdfBytes, "application/pdf", $"SkillHub_Certificate_{id}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}