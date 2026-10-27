using SkillHub.Domain.Enums;

namespace SkillHub.Domain.Entities;

public class Workshop
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int Capacity { get; set; }

    // Navigation properties
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    // Domain helper for available spots
    public int AvailableSpots => Capacity - Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
}