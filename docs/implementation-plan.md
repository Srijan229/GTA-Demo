# Implementation plan

## Checkpoint 1: Assessment and traceability

- Inventory screens, formulas, data sources, metadata, assets, and App Checker findings.
- Map every write operation and meaningful navigation path.
- Define the normalized domain model and record unresolved policy questions.
- Produce page inventory, navigation model, design system, and Canvas UI mapping.

Exit criteria: every Canvas screen and `Patch()`/`SubmitForm()` operation has a documented destination or explicit deprecation decision.

## Checkpoint 2: Repository and architecture foundation

- Create React/TypeScript/Vite frontend and ASP.NET Core solution boundaries.
- Configure SQL Server, Mailpit, protected document storage, health checks, OpenAPI, Problem Details, structured logging, linting, formatting, and tests.
- Add the first EF Core migration and anonymized seed data.

Exit criteria: the local stack starts, health checks pass, migrations apply, and frontend/backend test commands pass.

## Checkpoint 3: Authentication and application shell

- Implement development-only identity selection.
- Add policy and resource-based API authorization.
- Build responsive role-aware navigation and system pages.

## Checkpoint 4: Applicant profile and documents

- Deliver profile, education, experience, completion, resume, and transcript workflows.
- Enforce ownership and protected document access.

## Checkpoint 5: Sections and application submission

- Deliver active phases, eligible sections, application creation, selected courses, duplicate prevention, submission, history, status, and withdrawal.

## Checkpoint 6: Faculty review

- Deliver assigned sections, authorized applicant access, review actions, interview, hire recommendation, and assignment workflows.

## Checkpoint 7: Administration

- Deliver applicants, applications, sections, faculty assignments, phases, users/roles, settings, and audit history.

## Checkpoint 8: Transition readiness

- Add email events, migration utilities, accessibility and responsive verification, integration and end-to-end coverage, operational documentation, and replaceable production adapters.

