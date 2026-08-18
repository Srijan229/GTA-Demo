using Gta.Application.Domain.Applications;
using Gta.Application.Domain.Auditing;
using Gta.Application.Domain.Documents;
using Gta.Application.Domain.Identity;
using Gta.Application.Domain.Profiles;
using Gta.Application.Domain.Configuration;
using Gta.Application.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gta.Application.Infrastructure.Persistence;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(x => x.Email).HasMaxLength(320);
        b.Property(x => x.NormalizedEmail).HasMaxLength(320);
        b.Property(x => x.DisplayName).HasMaxLength(200);
        b.Property(x => x.UniversityId).HasMaxLength(50);
        b.HasIndex(x => x.NormalizedEmail).IsUnique();
        // MySQL unique indexes allow multiple NULL values, so no filtered index is required.
        b.HasIndex(x => x.UniversityId).IsUnique();
    }
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.Property(x => x.Name).HasMaxLength(80);
        b.Property(x => x.NormalizedName).HasMaxLength(80);
        b.HasIndex(x => x.NormalizedName).IsUnique();
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.HasKey(x => new { x.UserId, x.RoleId });
        b.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId);
        b.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId);
    }
}

public sealed class ProfileConfiguration : IEntityTypeConfiguration<ApplicantProfile>
{
    public void Configure(EntityTypeBuilder<ApplicantProfile> b)
    {
        b.HasIndex(x => x.UserId).IsUnique();
        b.HasOne(x => x.User).WithOne(x => x.ApplicantProfile).HasForeignKey<ApplicantProfile>(x => x.UserId);
        b.Property(x => x.Gpa).HasPrecision(3, 2);
        b.Property(x => x.Program).HasMaxLength(100);
    }
}

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.Property(x => x.OriginalFileName).HasMaxLength(255);
        b.Property(x => x.StorageKey).HasMaxLength(150);
        b.Property(x => x.MediaType).HasMaxLength(150);
        b.Property(x => x.Sha256).HasMaxLength(64).IsFixedLength();
        b.HasIndex(x => x.StorageKey).IsUnique();
        b.HasIndex(x => new { x.OwnerUserId, x.Type, x.State });
        b.Property<int?>("ActiveSlot")
            .HasComputedColumnSql("CASE WHEN `State` = 1 THEN 1 ELSE NULL END", stored: true);
        b.HasIndex("OwnerUserId", "Type", "ActiveSlot").IsUnique();
    }
}

public sealed class ApplicationConfiguration : IEntityTypeConfiguration<ApplicantApplication>
{
    public void Configure(EntityTypeBuilder<ApplicantApplication> b)
    {
        b.Property(x => x.Reference).HasMaxLength(40);
        b.HasIndex(x => x.Reference).IsUnique();
        b.HasIndex(x => new { x.ApplicantUserId, x.ApplicationPhaseId }).IsUnique();
    }
}

public sealed class ApplicationChoiceConfiguration : IEntityTypeConfiguration<ApplicationChoice>
{
    public void Configure(EntityTypeBuilder<ApplicationChoice> b) =>
        b.HasIndex(x => new { x.ApplicationId, x.CourseSectionId }).IsUnique();
}

public sealed class SectionConfiguration : IEntityTypeConfiguration<CourseSection>
{
    public void Configure(EntityTypeBuilder<CourseSection> b)
    {
        b.Property(x => x.SectionNumber).HasMaxLength(30);
        b.HasIndex(x => new { x.CourseId, x.AcademicTermId, x.SectionNumber }).IsUnique();
    }
}

public sealed class FacultyAssignmentConfiguration : IEntityTypeConfiguration<FacultySectionAssignment>
{
    public void Configure(EntityTypeBuilder<FacultySectionAssignment> b) =>
        b.HasIndex(x => new { x.FacultyUserId, x.CourseSectionId, x.IsActive }).IsUnique();
}

public sealed class PlacementConfiguration : IEntityTypeConfiguration<Placement>
{
    public void Configure(EntityTypeBuilder<Placement> b)
    {
        b.Property<int?>("ActiveSlot")
            .HasComputedColumnSql("CASE WHEN `IsActive` = 1 THEN 1 ELSE NULL END", stored: true);
        b.HasIndex("ApplicationChoiceId", "CourseSectionId", "ActiveSlot").IsUnique();
        b.HasIndex(x => new { x.CourseSectionId, x.IsActive });
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.Property(x => x.Action).HasMaxLength(120);
        b.Property(x => x.EntityType).HasMaxLength(120);
        b.Property(x => x.EntityReference).HasMaxLength(100);
        b.Property(x => x.Result).HasMaxLength(40);
        b.Property(x => x.CorrelationId).HasMaxLength(100);
        b.HasIndex(x => x.OccurredAtUtc);
        b.HasIndex(x => x.CorrelationId);
    }
}

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> b)
    {
        b.Property(x => x.Key).HasMaxLength(150);
        b.Property(x => x.Value).HasMaxLength(2000);
        b.Property(x => x.Description).HasMaxLength(500);
        b.HasIndex(x => x.Key).IsUnique();
    }
}

public sealed class SectionImportBatchConfiguration : IEntityTypeConfiguration<SectionImportBatch>
{
    public void Configure(EntityTypeBuilder<SectionImportBatch> b) { b.Property(x => x.FileName).HasMaxLength(255); b.HasIndex(x => x.ImportedAtUtc); }
}
public sealed class EmailOutboxConfiguration : IEntityTypeConfiguration<EmailOutboxMessage>
{
    public void Configure(EntityTypeBuilder<EmailOutboxMessage> b) { b.Property(x => x.Recipient).HasMaxLength(320); b.Property(x => x.Subject).HasMaxLength(300); b.Property(x => x.LastError).HasMaxLength(1000); b.Property(x => x.CorrelationId).HasMaxLength(100); b.HasIndex(x => new { x.State, x.NextAttemptAtUtc }); }
}
