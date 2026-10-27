namespace SkillHub.Application.DTOs.Workshops;

public record WorkshopEnrollmentDetailDto(
    Guid EnrollmentId,
    string ParticipantName,
    string ParticipantEmail,
    string Status,
    DateTime EnrolledAt
);    