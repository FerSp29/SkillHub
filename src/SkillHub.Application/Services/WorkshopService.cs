using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Interfaces;

namespace SkillHub.Application.Services;

public class WorkshopService : IWorkshopService
{
    private readonly IWorkshopRepository _workshopRepository;
    private readonly IUserService _userService;

    public WorkshopService(IWorkshopRepository workshopRepository, IUserService userService)
    {
        _workshopRepository = workshopRepository;
        _userService = userService;
    }

    public async Task<IEnumerable<WorkshopResponseDto>> GetUpcomingWorkshopsAsync()
    {
        var workshops = await _workshopRepository.GetUpcomingWorkshopsAsync();
        var dtos = new List<WorkshopResponseDto>();

        foreach (var w in workshops)
        {
            var instructor = await _userService.GetByIdAsync(w.InstructorId);
            dtos.Add(new WorkshopResponseDto(
                w.Id,
                w.Code,
                w.Title,
                w.Description,
                instructor?.FullName ?? "Unknown",
                instructor?.Email ?? "Unknown",
                w.StartDateTime,
                w.EndDateTime,
                w.Capacity,
                w.AvailableSpots
            ));
        }

        return dtos;
    }

    public async Task<WorkshopResponseDto?> GetWorkshopByIdAsync(Guid id)
    {
        var workshop = await _workshopRepository.GetByIdWithEnrollmentsAsync(id);
        if (workshop == null) return null;

        var instructor = await _userService.GetByIdAsync(workshop.InstructorId);

        return new WorkshopResponseDto(
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.Description,
            instructor?.FullName ?? "Unknown",
            instructor?.Email ?? "Unknown",
            workshop.StartDateTime,
            workshop.EndDateTime,
            workshop.Capacity,
            workshop.AvailableSpots
        );
    }

    public async Task<WorkshopResponseDto> CreateWorkshopAsync(CreateWorkshopRequestDto request)
    {
        // Rule 1: Validate existing instructor
        var instructor = await _userService.GetByEmailAsync(request.InstructorEmail);
        if (instructor == null)
        {
            throw new ArgumentException($"Instructor with email '{request.InstructorEmail}' does not exist.");
        }

        // Rule 2: Instructor must be an employee
        if (!instructor.IsEmployee)
        {
            throw new ArgumentException("The specified instructor must have the 'Employee' role.");
        }

        // Rule 3: Workshop must always be created with a future start date
        var startUtc = DateTime.SpecifyKind(request.StartDateTime, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(request.EndDateTime, DateTimeKind.Utc);

        if (startUtc <= DateTime.UtcNow)
        {
            throw new ArgumentException("Workshop start date and time must be in the future.");
        }

        // Rule 4: End date and time must be after start date and time
        if (endUtc <= startUtc)
        {
            throw new ArgumentException("Workshop end date and time must be after start date and time.");
        }

        // Rule 5: Unique workshop code
        var existingCode = await _workshopRepository.GetByCodeAsync(request.Code);
        if (existingCode != null)
        {
            throw new ArgumentException($"A workshop with code '{request.Code}' already exists.");
        }

        if (request.Capacity <= 0)
        {
            throw new ArgumentException("Workshop capacity must be greater than zero.");
        }

        var workshop = new Workshop
        {
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            InstructorId = instructor.Id,
            StartDateTime = startUtc,
            EndDateTime = endUtc,
            Capacity = request.Capacity
        };

        await _workshopRepository.AddAsync(workshop);
        await _workshopRepository.SaveChangesAsync();

        return new WorkshopResponseDto(
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.Description,
            instructor.FullName,
            instructor.Email,
            workshop.StartDateTime,
            workshop.EndDateTime,
            workshop.Capacity,
            workshop.Capacity
        );
    }

    public async Task<IEnumerable<WorkshopEnrollmentDetailDto>> GetWorkshopEnrollmentsAsync(Guid workshopId)
    {
        var workshop = await _workshopRepository.GetByIdWithEnrollmentsAsync(workshopId);
        if (workshop == null)
        {
            throw new KeyNotFoundException("Workshop not found.");
        }

        var list = new List<WorkshopEnrollmentDetailDto>();
        foreach (var enrollment in workshop.Enrollments)
        {
            var participant = await _userService.GetByIdAsync(enrollment.ParticipantId);
            list.Add(new WorkshopEnrollmentDetailDto(
                enrollment.Id,
                participant?.FullName ?? "Unknown",
                participant?.Email ?? "Unknown",
                enrollment.Status.ToString(),
                enrollment.CreatedAt
            ));
        }

        return list;
    }
}