using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Interfaces;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetByIdWithWorkshopAsync(Guid id)
    {
        return await _context.Enrollments
            .Include(e => e.Workshop)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Enrollment>> GetByParticipantIdAsync(string participantId)
    {
        return await _context.Enrollments
            .Include(e => e.Workshop)
            .Where(e => e.ParticipantId == participantId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasActiveEnrollmentInWorkshopAsync(string participantId, Guid workshopId)
    {
        return await _context.Enrollments.AnyAsync(e =>
            e.ParticipantId == participantId &&
            e.WorkshopId == workshopId &&
            e.Status == EnrollmentStatus.Active);
    }

    public async Task<bool> HasOverlappingActiveEnrollmentAsync(string participantId, DateTime start, DateTime end)
    {
        return await _context.Enrollments
            .Include(e => e.Workshop)
            .AnyAsync(e =>
                e.ParticipantId == participantId &&
                e.Status == EnrollmentStatus.Active &&
                e.Workshop.StartDateTime < end &&
                start < e.Workshop.EndDateTime);
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await _context.Enrollments.AddAsync(enrollment);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<(Workshop Workshop, int ActiveCount)>> GetTopWorkshopsAsync()
    {
        var workshops = await _context.Workshops
            .Include(w => w.Enrollments)
            .ToListAsync();

        return workshops
            .Select(w => (Workshop: w, ActiveCount: w.Enrollments.Count(e => e.Status == EnrollmentStatus.Active)))
            .OrderByDescending(x => x.ActiveCount)
            .ToList();
    }
}