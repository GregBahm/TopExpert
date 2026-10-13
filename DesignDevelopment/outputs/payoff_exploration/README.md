# Local pitch review

Run `node outputs/payoff_exploration/review-server.cjs` from `DesignDevelopment`, then open **http://127.0.0.1:8765**. No dependencies or accounts are required.

The single frontend is `report.html`, `review.css` and `review.js`. It loads the 500 canonical pitches from `ranked_pitches.json`. The local server atomically saves statuses and notes in `reviews.json`; no cloud or browser database is used. The current format is `top-expert-reviews`, version 2, with records containing `id`, `status` (`unreviewed`, `rejected` or `accepted`) and `notes`. Notes alone do not change status. Only this format is accepted.

Wait for **Saved to reviews.json** before closing. If saving fails, retry or export the open page's reviews before closing. Full backups include rejected reviews and notes on unreviewed pitches. Imports merge included entries by pitch ID. Git sharing requires committing/pushing `reviews.json` and pulling it on the other computer; there is no automatic remote sync.

The `generated_*.json` drafts, `rankings_*.json` editorial judgments, original `seeds.json`, combined `ranked_pitches.json`, and readable `report.md` are preserved. `build-report.cjs` rebuilds/validates only the combined data and Markdown, never reviews or the frontend. See `DesignDevelopment/README.md` for commands and artifact layout.
