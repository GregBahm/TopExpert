# Top Expert design development

The design document, research library, case proposals, 50 case seeds, and 500 ranked pitches live here. The review tool is plain HTML/CSS/JavaScript served by a dependency-free local Node server.

## Review pitches

With Node.js 22.13 or newer, run from `DesignDevelopment`:

```sh
node outputs/payoff_exploration/review-server.cjs
```

Open **http://127.0.0.1:8765**. Keep the server running. No package installation, account, framework build, or database setup is needed.

Each pitch has **Unreviewed**, **Reviewed and rejected**, or **Reviewed and accepted**, plus notes. Statuses and notes save to `outputs/payoff_exploration/reviews.json`. Wait for **Saved to reviews.json** before closing. Save failures show a retry action; unsaved edits remain only in the open page, so do not close it until saved.

`reviews.json` is the only review store. There is no cloud sync or browser-local review database. To share reviews between computers, commit/push that file normally, then pull it on the other computer. It is intentionally tracked; do not put private information there if repository readers should not see it. Round 1 retains reviewed original pitches. Round 2 - Opus and Round 2 - Astra each contain 50 pitches, five each for premises 6–15, with separate review IDs. The Astra tab opens by default. Status checkbox counts apply to the selected round; notes appear on reviewed pitches.

## Edit the tool

The frontend has one copy of each file under `outputs/payoff_exploration/`:

- `report.html`: page shell and controls.
- `review.css`: styling.
- `review.js`: pitch rendering, status/notes, round tabs, filtering and saving.
- `review-server.cjs`: loopback HTTP server and atomic JSON persistence.
- `review-schema.cjs`: the current review format.
- `pitches.json`: the active pitches with round metadata loaded by the page. Original 500-pitch drafts and `ranked_pitches.json` remain archival design artifacts.

Refresh the browser after frontend edits; restart Node after server/schema edits. The server binds only to `127.0.0.1`. Opening the HTML file directly is not a separate application: use the server URL.

## Design artifacts

- `Top Expert Base Design Doc.md`: original source, including embedded character images.
- `outputs/researcher-brief.md` and `outputs/three-new-cases.md`: the earlier design trial.
- `outputs/paranormal_plot_library/`: research data, coverage, provenance and source maps.
- `outputs/research_work/`: curated lane datasets and manifests.
- `outputs/payoff_exploration/`: preserved generated pitches, separate editorial rankings, combined JSON, readable Markdown, and review tool.

To regenerate the combined data and Markdown report from preserved drafts/rankings, run from the repository root:

```sh
node DesignDevelopment/outputs/payoff_exploration/build-report.cjs
```

This validates exactly 50×10 pitches, original seeds, IDs, ranks and winner correspondence. It never overwrites reviews or frontend files. Edit the frontend once; there are no generated runtime copies.

A focused persistence smoke test is available:

```sh
node --test DesignDevelopment/outputs/payoff_exploration/review-server.test.cjs
```

It uses its own temporary reviews, not your real notes.
