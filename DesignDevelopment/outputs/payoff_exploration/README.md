# Local pitch review

Run `node outputs/payoff_exploration/review-server.cjs` from `DesignDevelopment`, then open **http://127.0.0.1:8765**. No dependencies or accounts are required.

The single frontend is `report.html`, `review.css` and `review.js`. It loads the active pitches from `pitches.json`, with separate tabs for Round 1, Round 2 - Opus, and Round 2 - Astra. Each Round 2 set contains five pitches for each of premises 6–15. The local server atomically saves statuses and notes in `reviews.json`; no cloud or browser database is used. The current format is `top-expert-reviews`, version 2, with records containing `id`, `status` (`unreviewed`, `rejected` or `accepted`) and `notes`. Notes alone do not change status. Only this format is accepted.

Wait for **Saved to reviews.json** before closing. If saving fails, retry or export the open page's reviews before closing. Full backups include rejected reviews and notes on unreviewed pitches. Imports merge included entries by pitch ID. Git sharing requires committing/pushing `reviews.json` and pulling it on the other computer; there is no automatic remote sync.

The `generated_*.json` drafts, `rankings_*.json` editorial judgments, original `seeds.json`, combined `ranked_pitches.json`, and readable `report.md` are preserved. The Round 2 sources are `round2_06_10.json` and `round2_11_15.json` for Opus, and `round2_astra.json` for Astra. Astra uses internal round ID 3 and `R2A-` pitch IDs so existing Opus reviews remain attached to their original `R2-` IDs. `build-report.cjs` rebuilds/validates only the original combined data and Markdown, never reviews, active pitches, or the frontend. See `DesignDevelopment/README.md` for commands and artifact layout.
