namespace SkillHub.Application.DTOs.Workshops;

public record WorkshopResponseDto(
    Guid Id,
    string Code,
    string Title,
    string Description,
    string InstructorName,
    string InstructorEmail,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int Capacity,
    int AvailableSpots
);    