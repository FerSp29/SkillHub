using SkillHub.Application.DTOs.Enrollments;

namespace SkillHub.Application.Services;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> EnrollAsync(string participantId, EnrollRequestDto request);
    Task<IEnumerable<EnrollmentResponseDto>> GetMyEnrollmentsAsync(string participantId);
    Task CancelEnrollmentAsync(Guid enrollmentId, string userId, bool isEmployee);
    Task<ImportExcelResultDto> ImportFromExcelAsync(Stream excelStream, string employeeUserId);
    Task<byte[]> GenerateCertificateAsync(Guid enrollmentId, string userId, bool isEmployee);
    Task<IEnumerable<TopWorkshopReportDto>> GetTopWorkshopsReportAsync();
}