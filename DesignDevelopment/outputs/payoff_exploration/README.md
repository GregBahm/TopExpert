# Payoff exploration

50 original numbered concepts from the base design document, ten spoiler-complete pitches per concept, and independent editorial ranks 1–10 with one winner per concept. Titles and initial descriptions are preserved; premise expansions are named per pitch.

`generated_01_17.json`, `generated_18_34.json`, and `generated_35_50.json` retain the drafts. Matching `rankings_*.json` files refer to stable pitch IDs. `ranked_pitches.json` combines the preserved seeds, drafts, ranks and rationales. `report.md` and `report.html` are readable exports; the winner is repeated in each expandable ten-pitch list for comparison.

`build-report.cjs` validates counts, IDs, seed fidelity, ranks and winner correspondence. It invokes `render-review.cjs`, which generates the standalone report, `site/app/report.ts`, and report downloads under `site/public`. No research fetching or AI generation is needed to rebuild these artifacts.

For automatic local saves, run `node .\outputs\payoff_exploration\review-server.cjs` from `DesignDevelopment` and open http://127.0.0.1:8765. The dependency-free server loads and atomically updates `reviews.json` in this directory. Commit that file to preserve selections and notes in Git; wait for **Saved to reviews.json** before closing. Rebuilding the report does not overwrite reviews. Opening the HTML as `file:///` is read-only, but can still export old browser-only reviews for import on the server.

The website source in `site/` implements selects, notes, filtering and export/import. Hosted reviews still persist in private Sites D1 by signed-in user and pitch ID; account review records are separate from the local `reviews.json` and are not part of this repository.

`validation.json`, `browser-validation.json`, and `review-validation.json` preserve historical verification evidence. Some original visual/verification limits remain documented; these are not guarantees about every browser or story mechanism. `review-preview.jpg` shows the simplified review interface without personal notes.

See the parent `DesignDevelopment/README.md` for local build, database setup, private Site continuation, and excluded material.
