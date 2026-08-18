# Progress log

## 2026-08-15 — Source intake and initial assessment

- Inspected the supplied `.msapp` read-only.
- Confirmed 23 screens, 10 Dataverse tables, 44 `Patch()` calls, 68 `Navigate()` calls, and 4 `SubmitForm()` calls.
- Confirmed applicant, faculty, administration, document, application, interview/hire, and assignment workflows.
- Recorded the first normalized domain model and critical business rules.
- Identified the missing local .NET SDK as a foundation-build limitation.
- No production credentials or institutional services were accessed.

Known limitations: complete field-level mapping, connector call mapping, and App Checker classification remain in progress.

## 2026-08-15 — Repository and backend SDK foundation

- Initialized a Git repository using the `main` branch; no commit or remote was created.
- Installed .NET 10.0.400 LTS for the current user and added it to the user PATH.
- Pinned the repository SDK with `global.json`.
- Created the API, Application, Domain, Infrastructure, and Contracts projects.
- Created Domain, Application, and API integration test projects.
- Restored and built all eight projects in Release mode with zero warnings and zero errors.
- Ran the initial backend test suite: three tests passed.

## 2026-08-15 — SQL persistence foundation

- Added the normalized EF Core model for identity, profiles, repeatable education and experience, documents, terms, courses, sections, phases, applications, choices, faculty authorization/actions, placements, status history, and audit logs.
- Added SQL Server indexes, unique constraints, optimistic concurrency columns, and restricted deletion behavior.
- Added RFC Problem Details handling with correlation identifiers, JSON console logging, OpenAPI, and liveness/readiness endpoints.
- Generated and applied `InitialNormalizedSchema` to SQL Server 2022 in Docker.
- SQL Server surfaced a multiple-cascade-path defect during the first application attempt; deletion behavior was corrected and the migration regenerated before completion.
- Started SQL Server and Mailpit successfully.
- Verified `/health/live`, SQL-backed `/health/ready`, and `/api/v1/system/info` with HTTP 200 responses.
- Final verification: backend tests 3 passed, frontend test 1 passed, frontend build passed, and frontend lint passed.

## 2026-08-15 — Development authentication and seed data

- Added deterministic anonymized users for Applicant, Faculty, and Administrator roles.
- Added an applicant profile, academic term, course, section, active application phase, and faculty-section authorization seed records.
- Implemented development-only HTTP-only cookie authentication and current-user/session endpoints.
- Added API policies for Applicant, Faculty, and Administrator roles and frontend protected route boundaries.
- Connected the development login UI to real API-provided users and session creation.
- Verified the live authorization matrix: anonymous requests receive 401; each role receives 204 only for its own policy endpoint and 403 for the other roles.
- Added domain tests for the Canvas-derived part-time one-assignment and full-time two-assignment limits.

## 2026-08-15 — Applicant profile vertical slice

- Added applicant-owned profile retrieval, update, and derived completion endpoints.
- Added repeatable normalized education and experience persistence with ownership validation.
- Added GPA, graduation-year, required child-field, URL, and date-range validation.
- Implemented the responsive profile workspace with React Hook Form, Zod, completion status, save feedback, and repeatable editors.
- Verified faculty receives 403 for the applicant profile endpoint.
- SQL verification initially exposed incorrect state tracking for new child records; child inserts/removals were made explicit and the complete save was rerun successfully.
- Verified the saved SQL-backed profile contains one education and one experience record and reports 100% completion.

## 2026-08-15 — Protected applicant documents

- Added replaceable `IDocumentStorage` and local filesystem implementation outside the web root.
- Added server-generated storage keys, root-containment checks, SHA-256 metadata, versioning, and a filtered unique constraint for one active document per applicant/type.
- Added PDF signature validation and DOCX ZIP/package-entry validation with a 10 MB maximum.
- Added applicant-only list, upload/replace, and authorization-safe streaming endpoints.
- Added responsive resume and transcript cards with drag/drop, browse, client validation, progress state, replacement, metadata, and download actions.
- Corrected the configured relative storage root after the initial test harness showed it climbed one directory above the workspace.
- Live verification: resume version 1 uploaded, replacement became version 2, invalid PDF returned 400, download returned 200, and faculty access returned 403.

## 2026-08-15 — Sections and application submission

- Added program/term/phase-filtered available-section queries with applied-state projection.
- Added profile and resume/transcript readiness enforcement.
- Added multi-section choice selection, employment basis, review, explicit confirmation, and application history UI.
- Implemented submission as a serializable SQL transaction creating the application, ordered choices, and initial status-history event.
- Enforced one application per applicant/phase in both application logic and a database unique constraint.
- Live verification uploaded the required transcript, returned one eligible section, submitted with HTTP 201, and returned HTTP 409 for the duplicate attempt.
- Confirmed the persisted history contains one Submitted application with reference `GTA-2026FA-4648A300`.

## 2026-08-15 — Faculty review workflow

- Added assigned-section and authorized-application queries scoped through active faculty-section relationships.
- Added applicant review details, normalized education/experience, internal notes, and separately authorized document downloads.
- Added transactional interview and hire-recommendation actions.
- Enforced interview-before-hire, workload limits, duplicate-action conflicts, hire removal before interview removal, and placement protection.
- Added applicant-visible Interview/UnderReview transitions with status-history entries; hire remains an internal recommendation until placement.
- Added faculty dashboard, sections table, application queue, and responsive applicant review/action panel.
- Live verification: one assigned section, one authorized application, two documents, document download 200, and applicant access to faculty APIs 403.
- Verified invalid action order returns 409; interview and hire activation/removal return 200 in the permitted sequence.

## 2026-08-15 — Administration operations

- Added administrator-only dashboard, applications, applicants, sections, phases, users/roles, controlled settings, and audit APIs and workspaces.
- Added section capacity/activation editing and faculty assignment management, including active-faculty validation.
- Added phase date validation and overlap protection for active phases in the same term and program.
- Added user activation and role management with protection against removing or disabling the final active administrator.
- Added a normalized `SystemSettings` table, the `AddSystemSettings` migration, and idempotent development defaults for existing databases.
- Every administrative mutation writes a correlation-linked audit record in the same database transaction.
- Applied the migration to SQL Server and verified three settings were seeded into the pre-existing database.
- Live verification: administrator dashboard returned the SQL-backed application, Faculty received 403, a setting update returned 204, and the resulting `SystemSettingUpdated` audit record was returned.
- Final verification: backend build had zero warnings/errors, backend tests 9 passed, frontend build/lint passed, and frontend test passed.

## 2026-08-15 — Administrator placements

- Added the administrator placement candidate queue, limited to choices with an active faculty hire recommendation.
- Added transactional placement and removal operations with part-time/full-time workload enforcement, active-section validation, and section-capacity protection.
- Derived unassigned, partially assigned, and fully assigned states from active placement records rather than mutable counters.
- Added `Selected`/`Interview` application transitions with status-history entries and correlation-linked placement audit records.
- Replaced the placement uniqueness constraint with a filtered active-placement constraint so a removed placement can be assigned again while history remains intact.
- Added the `/admin/placements` workspace with capacity, workload, assignment-state, place, and remove controls.
- Applied the `HardenActivePlacements` migration to SQL Server.
- Live verification: one recommended candidate was returned, placement became fully assigned, duplicate placement returned 409, Applicant access returned 403, removal returned unassigned, and `PlacementRemoved` was the latest audit action.
- Restored the seeded application to its pre-verification review state and stopped the API verification process.
- Final verification: backend tests 9 passed, frontend test passed, and frontend build/lint passed.

## 2026-08-15 — Applicant application details and withdrawal

- Added owner-scoped application details with selected sections, employment basis, current status, and chronological status history.
- Added a reusable withdrawal policy allowing only Submitted/UnderReview applications before active interview, hire, or placement activity.
- Added transactional withdrawal with optional reason validation, final-state transition, status history, and correlation-linked audit logging.
- Added explicit conflict responses for repeated withdrawal and applications whose review/hiring lifecycle has progressed.
- Added the applicant detail route and final-confirmation withdrawal interface with blocked-reason guidance.
- Live verification: the owner received the SQL-backed detail and history, Faculty received 403, the application was initially eligible, active interview activity disabled withdrawal, and the guarded attempt returned 409.
- Removed the verification interview marker and restored the seeded application to UnderReview; the API verification process was stopped.
- Final verification: backend tests 16 passed (13 domain, 2 application, 1 API), frontend test passed, and frontend build/lint passed.

## 2026-08-15 — Applicant Canvas-recognition parity

- Replaced the placeholder applicant dashboard with a SQL-backed GTA home page containing welcome context, active-phase notice, profile completion, document readiness, application summary, latest status, and next steps.
- Restored the recognizable applicant entry labels: `GTA Home`, `View GTA Profile`, `Apply to Course`, and `My Applications`.
- Added prominent home actions matching the Canvas task model while retaining the consolidated, accessible web routes.
- Added profile section navigation for personal, academic, education, experience, resume, and transcript workflows.
- Added course search and result counts to Apply to Course.
- Reworked My Applications into a searchable, status-filterable application table with the expected reference, term, courses, submitted date, status, and detail action.
- Added route-aware page titles so applicant pages no longer all display `Dashboard`.
- Aligned the .NET launch-profile port with the documented frontend proxy port (`5080`).
- Frontend lint, production build, and test passed. Automated browser visual inspection was unavailable because no browser surface was connected; the local frontend remains available for manual review.

## 2026-08-15 — Validated section import

- Added an administrator-only CSV template, validation preview, transactional import, row-level errors, and import history workspace.
- Validated the exact header contract, required values, term dates, non-negative positions, Boolean activation, column counts, and duplicate term/course/section keys within a file.
- Added safe upsert/reactivation behavior for academic terms, courses, and sections while retaining existing section identifiers.
- Added `SectionImportBatch` persistence with accepted/rejected totals, redacted validation details, actor, filename, timestamp, and correlation-linked audit record.
- Added and applied the `AddSectionImportHistory` SQL Server migration.
- Verified the template endpoint, empty SQL-backed history, Faculty 403 authorization, API health 200, and web health 200.
- Backend build passed with zero warnings/errors and 16 tests passed; frontend lint/build passed.

## 2026-08-15 — Transactional email notifications

- Added replaceable `IEmailSender` and `IEmailOutbox` boundaries with a local SMTP adapter for Mailpit.
- Added a durable SQL outbox, asynchronous hosted delivery processor, bounded retry schedule, failure state, sanitized error tracking, sent timestamp, and correlation identifiers.
- Queued applicant submission, withdrawal, interview, and placement notifications inside the same transactions as their business changes.
- Email content excludes internal faculty notes and directs applicants back to the authenticated portal for details.
- Added administrator-only email delivery history and navigation.
- Added and applied the `AddEmailOutbox` migration.
- Live verification queued a reversible interview notification, delivered it to `alex.applicant@example.test`, confirmed SQL state `Sent`, and confirmed one Mailpit message; the interview marker was then removed.
- Backend build passed with zero warnings/errors, all 16 backend tests passed, and frontend lint/build/test passed.

## 2026-08-15 — Administration hook-order defect

- Diagnosed intermittent `Rendered more hooks than during the previous render` failures as stateful row components being invoked as ordinary functions inside collection maps.
- Refactored settings, sections, phases, and users into properly keyed React row components, keeping each row's hooks inside its own stable component boundary.
- Added route-level error handling with safe retry and sign-in recovery actions instead of the React Router developer crash screen.
- Confirmed no row components remain invoked as ordinary functions; frontend lint, production build, and test passed, and the running web/API health checks returned 200.

## 2026-08-15 — Administrator settings usability

- Replaced the raw technical key/value table with grouped Application, Document, and Local Development setting cards.
- Added friendly labels, explanations, numeric controls with units/ranges, an on/off development toggle, last-updated context, change-aware Save buttons, and responsive layout.
- Development-only controls are hidden outside frontend development builds.
- Added API validation for course selections (1–10), upload size (1–50 MB), and Boolean development switching.
- Connected application submission and its selection UI to the stored maximum-choice setting and document validation to the stored upload-size setting, retaining Canvas-compatible defaults of five choices and 10 MB.
- Live verification returned maximum choices `5`, rejected invalid value `0` with HTTP 400, and preserved all three settings; backend build/tests and frontend lint/build/test passed.

## 2026-08-15 — Faculty interview and decision queue

- Added a resource-scoped faculty interview queue limited to active interview candidates in sections assigned to the current faculty member.
- Added course grouping data, interview timestamp, application status, employment basis, hire-recommendation state, derived placement workload, and decision-state projections.
- Added `/faculty/interviews` with applicant search, course/section filter, decision filter, responsive results table, empty/loading/error states, and direct review/decision actions.
- Added `Interviews & decisions` to faculty navigation.
- Live verification returned one candidate as `AwaitingDecision` with workload `0/1`, Applicant access returned 403, and the temporary interview marker was removed.
- Backend build passed with zero warnings/errors, all 16 backend tests passed, and frontend lint/build/test passed.
