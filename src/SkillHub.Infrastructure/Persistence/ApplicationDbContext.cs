using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Workshop configurations
        builder.Entity<Workshop>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.HasIndex(w => w.Code).IsUnique();
            entity.Property(w => w.Code).HasMaxLength(20).IsRequired();
            entity.Property(w => w.Title).HasMaxLength(150).IsRequired();
            entity.Property(w => w.InstructorId).IsRequired();
        });

        // Enrollment configurations 
        builder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Workshop)
                .WithMany(w => w.Enrollments)
                .HasForeignKey(e => e.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.ParticipantId).IsRequired();
        });
    }
}    