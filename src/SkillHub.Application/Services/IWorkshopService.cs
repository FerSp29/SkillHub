using SkillHub.Application.DTOs.Workshops;

namespace SkillHub.Application.Services;

public interface IWorkshopService
{
    Task<IEnumerable<WorkshopResponseDto>> GetUpcomingWorkshopsAsync();
    Task<WorkshopResponseDto?> GetWorkshopByIdAsync(Guid id);
    Task<WorkshopResponseDto> CreateWorkshopAsync(CreateWorkshopRequestDto request);
    Task<IEnumerable<WorkshopEnrollmentDetailDto>> GetWorkshopEnrollmentsAsync(Guid workshopId);
}