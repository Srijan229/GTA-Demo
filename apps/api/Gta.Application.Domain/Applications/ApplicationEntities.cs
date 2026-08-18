using Gta.Application.Domain.Common;
using Gta.Application.Domain.Identity;

namespace Gta.Application.Domain.Applications;

public enum EmploymentBasis { PartTime10Hours = 1, FullTime20Hours = 2 }
public enum ApplicationState { Draft = 1, Submitted = 2, UnderReview = 3, Interview = 4, Selected = 5, NotSelected = 6, Withdrawn = 7 }
public enum ReviewActionType { Interview = 1, HireRecommendation = 2 }

public sealed class AcademicTerm : Entity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
}

public sealed class Course : Entity
{
    public required string SubjectCode { get; set; }
    public required string CatalogNumber { get; set; }
    public required string Title { get; set; }
}

public sealed class CourseSection : AuditableEntity
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public Guid AcademicTermId { get; set; }
    public AcademicTerm AcademicTerm { get; set; } = null!;
    public required string SectionNumber { get; set; }
    public string? Schedule { get; set; }
    public string? DeliveryMethod { get; set; }
    public int? AvailablePositions { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ApplicationPhase : AuditableEntity
{
    public Guid AcademicTermId { get; set; }
    public AcademicTerm AcademicTerm { get; set; } = null!;
    public required string Name { get; set; }
    public required string Program { get; set; }
    public DateTimeOffset OpensAtUtc { get; set; }
    public DateTimeOffset ClosesAtUtc { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ApplicantApplication : AuditableEntity
{
    public Guid ApplicantUserId { get; set; }
    public User ApplicantUser { get; set; } = null!;
    public Guid ApplicationPhaseId { get; set; }
    public ApplicationPhase ApplicationPhase { get; set; } = null!;
    public required string Reference { get; set; }
    public EmploymentBasis EmploymentBasis { get; set; }
    public ApplicationState State { get; set; } = ApplicationState.Draft;
    public DateTimeOffset? SubmittedAtUtc { get; set; }
    public ICollection<ApplicationChoice> Choices { get; set; } = [];
    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = [];
}

public sealed class ApplicationChoice : AuditableEntity
{
    public Guid ApplicationId { get; set; }
    public ApplicantApplication Application { get; set; } = null!;
    public Guid CourseSectionId { get; set; }
    public CourseSection CourseSection { get; set; } = null!;
    public int? PreferenceOrder { get; set; }
}

public sealed class FacultySectionAssignment : AuditableEntity
{
    public Guid FacultyUserId { get; set; }
    public User FacultyUser { get; set; } = null!;
    public Guid CourseSectionId { get; set; }
    public CourseSection CourseSection { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}

public sealed class FacultyReviewAction : AuditableEntity
{
    public Guid ApplicationChoiceId { get; set; }
    public ApplicationChoice ApplicationChoice { get; set; } = null!;
    public Guid FacultyUserId { get; set; }
    public User FacultyUser { get; set; } = null!;
    public ReviewActionType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public string? InternalNotes { get; set; }
}

public sealed class Placement : AuditableEntity
{
    public Guid ApplicationChoiceId { get; set; }
    public ApplicationChoice ApplicationChoice { get; set; } = null!;
    public Guid CourseSectionId { get; set; }
    public CourseSection CourseSection { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}

public sealed class ApplicationStatusHistory : Entity
{
    public Guid ApplicationId { get; set; }
    public ApplicantApplication Application { get; set; } = null!;
    public ApplicationState FromState { get; set; }
    public ApplicationState ToState { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public Guid ChangedByUserId { get; set; }
    public string? Reason { get; set; }
}
