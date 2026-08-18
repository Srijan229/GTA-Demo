# Assumptions and open decisions

| ID | State | Assumption or question | Current handling |
|---|---|---|---|
| A-001 | Accepted | Dataverse storage shape is not a compatibility requirement. | Preserve behavior and traceability while normalizing SQL. |
| A-002 | Pending policy confirmation | Interview must precede hire. | Preserve the Canvas rule initially and make the transition explicit in the domain. |
| A-003 | Pending policy confirmation | Part-time permits one assignment and full-time permits two. | Preserve and enforce transactionally; do not store mutable counters. |
| A-004 | Pending source analysis | Education and experience may currently be flattened into Profile. | Introduce normalized child records while retaining all confirmed fields. |
| A-005 | Partially resolved | Whether applicants may withdraw, replace documents after submission, or edit submitted data. | Applicant withdrawal is now allowed only before active interview/hiring/placement activity. Document replacement and submitted-data editing remain pending policy confirmation. |
| A-006 | Accepted | Email addresses can change and are not durable relational keys. | Use internal immutable IDs; email remains a unique login/contact attribute where required. |
| A-007 | Accepted | Local authentication is for Development only. | Startup must fail closed if the mock handler is enabled elsewhere. |
| A-008 | Pending policy confirmation | Faculty assignment and applicant placement are distinct concepts. | Model reviewer-to-section assignment separately from applicant-to-section placement. |
