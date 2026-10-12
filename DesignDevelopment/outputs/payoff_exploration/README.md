# Payoff exploration

50 original numbered concepts from the base design document, ten spoiler-complete pitches per concept, and independent editorial ranks 1–10 with one winner per concept. Titles and initial descriptions are preserved; premise expansions are named per pitch.

`generated_01_17.json`, `generated_18_34.json`, and `generated_35_50.json` retain the drafts. Matching `rankings_*.json` files refer to stable pitch IDs. `ranked_pitches.json` combines the preserved seeds, drafts, ranks and rationales. `report.md` and `report.html` are readable exports; the winner is repeated in each expandable ten-pitch list for comparison.

`build-report.cjs` validates counts, IDs, seed fidelity, ranks and winner correspondence. It invokes `render-review.cjs`, which generates the standalone report, `site/app/report.ts`, and report downloads under `site/public`. No research fetching or AI generation is needed to rebuild these artifacts.

The website source in `site/` implements selects, notes, filtering and export/import. Hosted reviews persist in private Sites D1 by signed-in user and pitch ID; account review records are not part of this repository. The standalone HTML instead stores reviews in the current browser and labels that limitation.

`validation.json`, `browser-validation.json`, and `review-validation.json` preserve historical verification evidence. Some original visual/verification limits remain documented; these are not guarantees about every browser or story mechanism. `review-preview.jpg` shows the simplified review interface without personal notes.

See the parent `DesignDevelopment/README.md` for local build, database setup, private Site continuation, and excluded material.
