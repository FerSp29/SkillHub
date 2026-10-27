using SkillHub.Domain.Enums;

namespace SkillHub.Domain.Entities;

public class Enrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ParticipantId { get; set; } = string.Empty;
    public Guid WorkshopId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public string? CancelledByUserId { get; set; }

    // Navigation properties
    public Workshop Workshop { get; set; } = null!;
}