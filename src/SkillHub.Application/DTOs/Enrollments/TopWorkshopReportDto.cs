namespace SkillHub.Application.DTOs.Enrollments;

public record TopWorkshopReportDto(
    Guid WorkshopId,
    string Code,
    string Title,
    int Capacity,
    int ActiveEnrollmentsCount
);