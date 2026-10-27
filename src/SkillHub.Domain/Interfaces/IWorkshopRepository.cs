using SkillHub.Domain.Entities;

namespace SkillHub.Domain.Interfaces;

public interface IWorkshopRepository
{
    Task<IEnumerable<Workshop>> GetUpcomingWorkshopsAsync();
    Task<Workshop?> GetByIdWithEnrollmentsAsync(Guid id);
    Task<Workshop?> GetByCodeAsync(string code);
    Task AddAsync(Workshop workshop);
    Task SaveChangesAsync();
}