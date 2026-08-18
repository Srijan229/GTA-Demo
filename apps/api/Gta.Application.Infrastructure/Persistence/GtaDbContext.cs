using Gta.Application.Domain.Applications;
using Gta.Application.Domain.Auditing;
using Gta.Application.Domain.Common;
using Gta.Application.Domain.Documents;
using Gta.Application.Domain.Identity;
using Gta.Application.Domain.Profiles;
using Gta.Application.Domain.Configuration;
using Gta.Application.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Gta.Application.Infrastructure.Persistence;

public sealed class GtaDbContext(DbContextOptions<GtaDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ApplicantProfile> ApplicantProfiles => Set<ApplicantProfile>();
    public DbSet<EducationRecord> EducationRecords => Set<EducationRecord>();
    public DbSet<ExperienceRecord> ExperienceRecords => Set<ExperienceRecord>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AcademicTerm> AcademicTerms => Set<AcademicTerm>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<ApplicationPhase> ApplicationPhases => Set<ApplicationPhase>();
    public DbSet<ApplicantApplication> Applications => Set<ApplicantApplication>();
    public DbSet<ApplicationChoice> ApplicationChoices => Set<ApplicationChoice>();
    public DbSet<FacultySectionAssignment> FacultySectionAssignments => Set<FacultySectionAssignment>();
    public DbSet<FacultyReviewAction> FacultyReviewActions => Set<FacultyReviewAction>();
    public DbSet<Placement> Placements => Set<Placement>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistory => Set<ApplicationStatusHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<SectionImportBatch> SectionImportBatches => Set<SectionImportBatch>();
    public DbSet<EmailOutboxMessage> EmailOutboxMessages => Set<EmailOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GtaDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(type => !type.IsOwned() && type.GetTableName() is not null))
        {
            entityType.SetTableName($"gta_{entityType.GetTableName()}");
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(type => typeof(AuditableEntity).IsAssignableFrom(type.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(AuditableEntity.RowVersion))
                .IsConcurrencyToken()
                .ValueGeneratedNever()
                .HasMaxLength(16);
        }

        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(type => type.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampConcurrencyTokens();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampConcurrencyTokens();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void StampConcurrencyTokens()
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
        }
    }
}
