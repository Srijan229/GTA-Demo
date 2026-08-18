# Current-state assessment

## Source inspected

`CEC IST GTA Application.msapp`, last modified August 14, 2026, was inspected directly as an archive. It contains Power Fx YAML, control JSON, Dataverse metadata, embedded assets, and an App Checker SARIF report.

The source contains 23 screens, 44 `Patch()` calls, 68 `Navigate()` calls, and 4 `SubmitForm()` calls. The App Checker report contains 459 results; severity and migration relevance still require classification.

## Data sources

The Canvas application references ten native Dataverse sources:

1. Profiles
2. Resumes
3. Applications
4. ApplicationPerCourses
5. ApplicationPhases
6. UnofficialTranscripts
7. GTAStatuses
8. FacultyActions
9. ImportedSections
10. Users

It also references Office 365 Groups and Office 365 Outlook connectors. Connector presence does not by itself prove every advertised operation is used; call sites will be mapped separately.

## Current structural concerns

- Applicant education and experience are presented as separate screens but no dedicated Dataverse tables are referenced.
- Application-level `HiredCount`, `AssignmentCount`, and textual assignment state appear to duplicate facts that should be derived from normalized assignment records.
- Faculty actions use active/inactive toggles to represent undo behavior, making current state dependent on mutable historical rows.
- Applicant identity is frequently joined through email text rather than an immutable user key.
- Program-specific phases are hard-coded as Master's, PhD, and Postdoctoral settings.
- Important authorization appears intertwined with UI filtering and therefore needs explicit API policies.
- Applicant, course, section, and assignment concepts are mixed in `ApplicationPerCourses` and `ImportedSections`.

## Confirmed behavioral rules

- An applicant selects one or more course/section opportunities when applying.
- A per-course record links an application to a selected opportunity.
- A faculty interview action can be applied and undone.
- A hire action can be applied and undone until assignment rules prevent it.
- The current UI requires an interview mark before a hire mark.
- A part-time 10-hour applicant is limited to one assignment.
- A full-time 20-hour applicant is limited to two assignments.
- Assignment state is shown as unassigned, partially assigned, or fully assigned.
- Graduated applicants are excluded from parts of the hiring workflow.

These rules are confirmed as current behavior, not automatically endorsed as final policy. Ambiguities are tracked in `docs/assumptions.md`.

## Migration direction

The replacement will be a modular monolith. SQL tables will model durable business facts, while labels, counts, and completion percentages will generally be derived. All Canvas writes will be mapped to explicit application use cases with API authorization and transactional enforcement.

