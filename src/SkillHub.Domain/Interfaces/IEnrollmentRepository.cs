using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;

namespace SkillHub.Domain.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdWithWorkshopAsync(Guid id);
    Task<IEnumerable<Enrollment>> GetByParticipantIdAsync(string participantId);
    Task<bool> HasActiveEnrollmentInWorkshopAsync(string participantId, Guid workshopId);
    Task<bool> HasOverlappingActiveEnrollmentAsync(string participantId, DateTime start, DateTime end);
    Task AddAsync(Enrollment enrollment);
    Task SaveChangesAsync();
    Task<IEnumerable<(Workshop Workshop, int ActiveCount)>> GetTopWorkshopsAsync();
}