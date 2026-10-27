namespace SkillHub.Application.DTOs.Enrollments;

public record EnrollmentResponseDto(
    Guid Id,
    Guid WorkshopId,
    string WorkshopCode,
    string WorkshopTitle,
    string Status,
    DateTime CreatedAt,
    DateTime? CancelledAt,
    string? CancelledByUserId
);    