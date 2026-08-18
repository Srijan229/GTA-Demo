# Repository working agreement

- Preserve behavior from the Canvas source, but do not reproduce inefficient Dataverse storage shapes.
- Implement work as tested vertical slices: database, domain behavior, API, authorization, UI, and documentation.
- Keep applicant data and documents out of logs, seed data, screenshots, and commits.
- Never expose local document paths through an API response.
- Development authentication must fail closed outside the Development environment.
- Store dates in UTC and format them explicitly at the UI boundary.
- Do not mark a checkpoint complete until its available builds, tests, and manual checks pass.
- Do not commit or push unless the user explicitly requests it.

