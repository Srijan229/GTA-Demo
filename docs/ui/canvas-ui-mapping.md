# Canvas UI consolidation

The web application will not reproduce Canvas screens one-for-one.

- Four applicant profile/completion screens become one persistent profile workspace with section navigation.
- Two applicant document screens become one documents page with separate resume and transcript cards.
- Faculty profile, resume, and transcript screens become one authorized review route with protected document actions.
- Application submission and confirmation remain distinct route states so a completed submission cannot look editable.
- Administrator system settings are split into phases and general settings because phase behavior is operationally distinct.
- Section reviewer assignment and applicant placement are separate workflows even where the Canvas UI mixes assignment terminology.

Visual details from Canvas are treated as evidence of content and behavior, not a requirement to retain inefficient layouts or inaccessible controls.

## Applicant recognition parity

The applicant shell retains the recognizable Canvas entry points `GTA Home`, `View GTA Profile`, `Apply to Course`, and `My Applications`. The GTA home page presents those three primary tasks prominently and derives profile completion, document readiness, open-course availability, application count, latest status, and next steps from the live API.

The underlying web routes remain consolidated: resume/transcript management is linked from the GTA profile and readiness prompts, while course discovery leads into the focused application form. This preserves the familiar task model without reproducing the Canvas screen fragmentation.
