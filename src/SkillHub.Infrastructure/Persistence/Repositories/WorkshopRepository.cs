using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Interfaces;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public class WorkshopRepository : IWorkshopRepository
{
    private readonly ApplicationDbContext _context;

    public WorkshopRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Workshop>> GetUpcomingWorkshopsAsync()
    {
        return await _context.Workshops
            .Include(w => w.Enrollments)
            .Where(w => w.StartDateTime > DateTime.UtcNow)
            .OrderBy(w => w.StartDateTime)
            .ToListAsync();
    }

    public async Task<Workshop?> GetByIdWithEnrollmentsAsync(Guid id)
    {
        return await _context.Workshops
            .Include(w => w.Enrollments)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Workshop?> GetByCodeAsync(string code)
    {
        return await _context.Workshops
            .Include(w => w.Enrollments)
            .FirstOrDefaultAsync(w => w.Code == code);
    }

    public async Task AddAsync(Workshop workshop)
    {
        await _context.Workshops.AddAsync(workshop);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}