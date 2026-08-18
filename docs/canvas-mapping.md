# Canvas-to-web mapping

This document is the behavioral traceability index. Field-level and formula-level mappings will be expanded as each vertical slice enters implementation.

| Canvas screen | Proposed web destination | Primary domain area | Disposition |
|---|---|---|---|
| `GTA_HomePage_UI` | `/applicant` | Applicant dashboard | Consolidate navigation and actionable summaries. |
| `GTA_EditMyProfile_UI` | `/applicant/profile` | Applicant profile | Consolidate into a sectioned profile workspace. |
| `GTA_Edu_UI` | `/applicant/profile?section=education` | Education records | Normalize repeatable records. |
| `GTA_Experience_UI` | `/applicant/profile?section=experience` | Experience records | Normalize repeatable records. |
| `GTA_ProfileCompletion_UI` | `/applicant/profile?section=review` | Completion projection | Replace separate screen with derived completion review. |
| `GTA_ViewMyResume` | `/applicant/documents` | Documents | Consolidate into protected document cards. |
| `GTA_ViewMyTranscript` | `/applicant/documents` | Documents | Consolidate into protected document cards. |
| `GTA_Demo_UI` | Assessment pending | Applicant data/demo behavior | Retain only confirmed business behavior; do not ship demo-only UI accidentally. |
| `App_NewApplication_UI` | `/applicant/applications/new` | Application submission | Focused review-and-submit workflow. |
| `App_ApplicationSubmitted_UI` | `/applicant/applications/:id/confirmation` | Application submission | Route-bound confirmation using persisted data. |
| `GTA_MyApplications_UI` | `/applicant/applications` | Application history | Searchable history with status. |
| `Faculty_HomePage_UI` | `/faculty` | Faculty dashboard | Action-oriented dashboard. |
| `Faculty_ApplicationsDashboard_UI` | `/faculty/applications` | Faculty review | Authorized, filterable application list. |
| `Faculty_ViewApplicantProfile_UI` | `/faculty/applications/:choiceId` | Review/interview/hire | Consolidate profile and action panel; preserve authorization. |
| `Faculty_ViewApplicantResume_UI` | Faculty review document action | Documents | No separate raw-document page required. |
| `Faculty_ViewApplicantTranscript_UI` | Faculty review document action | Documents | No separate raw-document page required. |
| `Faculty_ManageInterviews_UI` | `/faculty/interviews` | Interviews | Dedicated queue and decision workflow. |
| `Faculty_ViewSections_UI` | `/faculty/sections` | Section authorization | Assigned sections and applicant access. |
| `Admin_ViewAllApplicants_UI` | `/admin/applicants` | Applicant administration | Search and authorized detail view. |
| `Admin_SystemSettings_UI` | `/admin/phases` and `/admin/settings` | Configuration | Split phase windows from general configuration. |
| `Admin_ModifyImportedSection_UI` | `/admin/sections` | Section administration | Validated table/editor and future import history. |
| `Admin_Assign_Other_UI` | `/admin/placements` | Applicant placement | Replace mutable counters with placement records. |
| `ErrorScreen` | `/error` plus route error boundaries | System | Safe error state with correlation ID. |

## Write-operation strategy

- Canvas `Patch()` calls become named application commands with validation and authorization.
- Multi-`Patch()` workflows become a single database transaction where atomicity is required.
- `SubmitForm()` becomes typed profile/document commands rather than generic entity submission.
- Undo actions append or transition auditable facts instead of erasing history.
- `Navigate()` calls become route links or post-command redirects, not business logic.

## Application submission mapping

The Canvas sequence that patches `Applications`, creates one or more `ApplicationPerCourses` rows, and updates `GTAStatuses` is implemented as one serializable SQL transaction. The normalized replacement creates an `Application`, its `ApplicationChoices`, and initial `ApplicationStatusHistory`. A unique `(ApplicantUserId, ApplicationPhaseId)` constraint and transactional check prevent duplicate phase applications. Mutable global GTA status is replaced by application-scoped status history.

Faculty `Patch(FacultyActions, ...)` formulas are represented by authorized `FacultyReviewAction` commands. Interview activation transitions the application to Interview and writes `ApplicationStatusHistory`. Hire is retained as an internal recommendation until a separate placement decision occurs. Undo rules prevent removing an interview while a hire recommendation is active and prevent removing a hire recommendation after placement.
