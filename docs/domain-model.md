# Domain model

## Identity and authorization

- `User`: immutable internal identity, university identifier, normalized email, display name, account state.
- `Role` and `UserRole`: explicit many-to-many authorization assignments.
- Production identity mapping is isolated behind authentication and current-user abstractions.

## Applicant profile

- `ApplicantProfile`: one per applicant; personal and academic fields confirmed by source.
- `EducationRecord`: repeatable education history owned by a profile.
- `ExperienceRecord`: repeatable employment/GTA/industry experience owned by a profile.
- Profile completion is calculated from configured required fields rather than stored as a mutable percentage.

## Documents

- `Document`: owner, type, original safe filename, server storage key, media type, byte length, checksum, version, timestamps, and lifecycle state.
- Resume and unofficial transcript are document types, not separate storage implementations.
- Superseded versions remain auditable and are not publicly addressable.

## Opportunities and applications

- `AcademicTerm`: explicit term identity and date context.
- `Course`: stable catalog identity.
- `CourseSection`: term-specific offering, schedule, capacity, and active state.
- `ApplicationPhase`: program/term window controlling application behavior.
- `Application`: applicant submission for a phase, employment basis, status, and timestamps.
- `ApplicationChoice`: selected course section and optional preference order; replaces `ApplicationPerCourses`.
- A unique constraint prevents duplicate active choices for the same application and section. Use-case validation prevents duplicate submissions across the applicable phase.

## Review and placement

- `FacultySectionAssignment`: faculty reviewer authorization for a section.
- `FacultyReviewAction`: append-oriented review event associated with an application choice and faculty user.
- `Interview`: interview lifecycle and scheduling details when confirmed.
- `Placement`: the durable fact that an applicant was assigned to a section.
- Assignment counts and labels are projections derived from active placements.

## Configuration and audit

- `ApplicationStatus` and `ApplicationStatusHistory`: controlled workflow state and append-only transitions.
- `SystemSetting`: non-secret controlled settings.
- `AuditLog`: actor, action, entity reference, outcome, timestamp, and correlation ID with redacted structured details.

## Critical database invariants

- Normalized email and university identifier are unique when present.
- A user has at most one applicant profile.
- A faculty-section authorization pair is unique while active.
- An application choice cannot repeat a section.
- A placement cannot exceed the applicant's approved workload limit.
- Document storage keys are unique and never supplied by clients.
- Status transitions and placement changes use optimistic concurrency.

