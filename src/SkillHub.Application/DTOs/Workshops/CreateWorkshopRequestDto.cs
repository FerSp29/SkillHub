namespace SkillHub.Application.DTOs.Workshops;

public record CreateWorkshopRequestDto(
    string Code,
    string Title,
    string Description,
    string InstructorEmail,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int Capacity
);    