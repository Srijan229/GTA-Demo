# Page inventory

All collection and detail pages require loading, empty, error, and access-denied handling. Tables become horizontally safe tables or structured cards on narrow viewports.

| Route | Roles | Purpose | Canvas source |
|---|---|---|---|
| `/login` | Anonymous, development only | Select an anonymized local identity. | New replacement for institutional identity dependency. |
| `/applicant` | Applicant | Show profile readiness, active phase, documents, and recent applications. | `GTA_HomePage_UI` |
| `/applicant/profile` | Applicant | Edit personal, academic, education, and experience data. | `GTA_EditMyProfile_UI`, `GTA_Edu_UI`, `GTA_Experience_UI`, `GTA_ProfileCompletion_UI` |
| `/applicant/documents` | Applicant | Upload, replace, and access resume/transcript. | `GTA_ViewMyResume`, `GTA_ViewMyTranscript` |
| `/applicant/sections` | Applicant | Discover eligible active sections. | Opportunity selection within `App_NewApplication_UI` |
| `/applicant/applications/new` | Applicant | Select sections and submit an application. | `App_NewApplication_UI` |
| `/applicant/applications` | Applicant | View submitted applications and statuses. | `GTA_MyApplications_UI` |
| `/applicant/applications/:id` | Owning applicant | View details and permitted actions. | `GTA_MyApplications_UI`, `App_ApplicationSubmitted_UI` |
| `/faculty` | Faculty | Show assigned sections and review workload. | `Faculty_HomePage_UI` |
| `/faculty/sections` | Faculty | View authorized sections. | `Faculty_ViewSections_UI` |
| `/faculty/applications` | Faculty | Search authorized applicants. | `Faculty_ApplicationsDashboard_UI` |
| `/faculty/applications/:choiceId` | Authorized faculty | Review applicant information and record actions. | `Faculty_ViewApplicantProfile_UI`, resume/transcript screens |
| `/faculty/interviews` | Faculty | Manage interview and decision workflow. | `Faculty_ManageInterviews_UI` |
| `/admin` | Administrator | Operational overview and warnings. | New consolidation of admin state. |
| `/admin/applications` | Administrator | Search and manage applications. | Admin/faculty application views |
| `/admin/applicants` | Administrator | Search and manage applicant records. | `Admin_ViewAllApplicants_UI` |
| `/admin/sections` | Administrator | Manage or import course sections. | `Admin_ModifyImportedSection_UI` |
| `/admin/sections/import` | Administrator | Validate, import, and review history for semester section CSV files. | `Admin_ModifyImportedSection_UI` |
| `/admin/sections` (faculty column) | Administrator | Manage faculty reviewer access to sections. | `Faculty_ViewSections_UI`, source role logic |
| `/admin/placements` | Administrator | Assign selected applicants to sections. | `Admin_Assign_Other_UI`, assignment popup behavior |
| `/admin/phases` | Administrator | Manage program/term application windows. | `Admin_SystemSettings_UI` |
| `/admin/users` | Administrator | Manage users and roles. | Source group/user behavior plus new authorization UI |
| `/admin/settings` | Administrator | Manage non-secret configuration. | `Admin_SystemSettings_UI` |
| `/admin/audit` | Administrator | Review important system activity. | New explicit replacement for Dataverse audit reliance |
| `/access-denied`, `/not-found`, `/error`, `/unavailable` | Any | Safe system feedback. | `ErrorScreen` plus required web states |
