# Security model

## Authorization boundaries

- Applicants may access only their own profile, applications, and documents.
- Faculty may access an applicant only through an active assignment to a section connected to the relevant application choice.
- Administrators receive only the policies required for their operational role.
- Document content endpoints repeat resource authorization; possession of an identifier is never sufficient.

## Local authentication

Development authentication presents anonymized users without fake passwords and creates an HTTP-only, SameSite=Strict cookie session. It is explicitly enabled only in `appsettings.Development.json`. Startup refuses to enable it outside Development and also refuses to start without an explicitly configured authentication provider.

Role policies are enforced by the API for Applicant, Faculty, and Administrator operations. The frontend route boundary improves usability but is not treated as an authorization control.

Faculty access is resource-scoped: an active `FacultySectionAssignment` must connect the current faculty user to the application choice's section. Applicant profile review and faculty document downloads independently repeat this relationship check. A Faculty role alone does not grant access to every applicant.

Placement management is administrator-only. Placement creation rechecks the active hire recommendation, employment workload, section state, and section capacity inside a serializable transaction. Placement changes update application history and append correlation-linked audit records in the same transaction.

Application details are owner-scoped by immutable applicant user ID. Withdrawal rechecks ownership, state, and active interview/hiring/placement activity inside a serializable transaction; a successful withdrawal appends status history and a correlation-linked audit record atomically.

Email notifications use a transactional SQL outbox. Workflow transactions store only the intended recipient, non-sensitive subject/body, and correlation metadata; delivery happens afterward through a replaceable SMTP adapter. Internal faculty notes and document content are never included. Delivery errors are reduced to exception type before persistence or display.

## Documents

Files are stored outside the web root under server-generated names. Uploads are size-limited and checked for allowed extension, declared media type, and file signature. Responses never disclose filesystem paths. Downloads use safe content-disposition filenames and authorization-aware streaming.

Local document keys are random server-generated values and are resolved only after filename and storage-root containment checks. Resume uploads accept validated PDF or DOCX packages; unofficial transcripts accept validated PDF only. Replacement creates a new version and marks the prior metadata as superseded rather than exposing or overwriting a client-selected path.

## Data handling

- No applicant document content or sensitive profile fields in logs.
- Secrets come from local secret storage or environment configuration and are never committed.
- Audit details are structured and redacted.
- UTC is used for persistence.
- API errors use Problem Details without stack traces or sensitive internals.
