# Case Builder development

This is a preproduction content and mechanics tool. Prioritize quick, reversible iteration over production-style infrastructure.

- Do not add tests by default. Routine labels, layout, hover, and other visual changes usually need only a quick manual spot check.
- Retain useful checks, and add one when a consequential, non-obvious bookkeeping bug or sufficiently stable behavior makes the benefit outweigh maintenance.
- Keep automated fixtures independent of authored cases, investigator catalog contents, and provisional balance choices. Editing creative content should not break bookkeeping checks.
- Run the affected focused checks once; broaden only when a failure or unresolved concern warrants it.
- The user is comfortable using Git and copy/paste during iteration. Do not add elaborate data-loss safeguards or recovery infrastructure by default. Preserve existing save/draft behavior unless the task calls for changing it.
