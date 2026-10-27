namespace SkillHub.Application.DTOs.Enrollments;

public record ImportErrorDetail(
    int RowNumber,
    string ParticipantEmail,
    string WorkshopCode,
    string Reason
);

public record ImportExcelResultDto(
    int TotalProcessed,
    int TotalSuccess,
    int TotalFailed,
    List<ImportErrorDetail> Errors
);