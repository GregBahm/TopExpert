# Top Expert design development

Portable design artifacts and source for Top Expert. The existing Unity project remains in the repository's `TopExpertUnity` folder.

## Contents

- `Top Expert Base Design Doc.md`: original design document, including its embedded character images.
- `outputs/researcher-brief.md` and `outputs/three-new-cases.md`: the earlier researcher-to-designer trial and its three case proposals.
- `outputs/paranormal_plot_library/`: consolidated research records in JSON/CSV/Markdown, source maps, coverage manifest, collection queue, and quality review. Coverage and evidence limitations are documented alongside the data.
- `outputs/research_work/`: curated lane datasets, lane manifests, and research review metadata. Raw page/text evidence caches and one-off harvesting scripts are intentionally omitted; provenance remains in the source maps and manifests.
- `outputs/payoff_exploration/`: exactly 50 original case seeds and 500 generated pitches, independent rankings, combined ranked JSON, readable reports, report generators, verification records, and the website source under `site/`.

The three `generated_*.json` files preserve original generation, and the three `rankings_*.json` files preserve independent editorial ordering. Stable pitch IDs connect drafts, rankings, and personal reviews. Rankings are subjective editorial choices, not objective scores. The research library was not a restriction on payoff exploration.

## Read or regenerate the reports

Open `outputs/payoff_exploration/report.html` for a standalone reading/review tool, or read `report.md`. The standalone file saves selects and notes only in that browser; export/import transfers them. It does not sync with the hosted Site. The standalone storage mode was not browser-tested because the automation tool blocks file URLs.

To validate the 50×10 dataset and regenerate the report and website HTML source, run from the repository root:

```sh
node DesignDevelopment/outputs/payoff_exploration/build-report.cjs
```

The generator uses relative paths and no external services. It updates `ranked_pitches.json`, `report.md`, `report.html`, `validation.json`, `site/app/report.ts`, and the report downloads under `site/public/`.

## Develop the website on another computer

Requirements: Node.js 22.13 or newer (Node 24 was used for verification), npm, and Git. The lockfile is included. From the repository root:

```sh
cd DesignDevelopment/outputs/payoff_exploration/site
npm ci --workspaces=false --include=dev --include=optional
npm run build
```

For the first local database setup, apply the generated schema migration after that build:

```sh
node --import ./scripts/sites-env.mjs ./node_modules/wrangler/bin/wrangler.js d1 execute DB --local --config dist/server/wrangler.json --persist-to .wrangler/state --file drizzle/0000_wet_ulik.sql
```

Run this once for a fresh local database. Applied migrations are immutable; generate additional migrations for future schema changes using `npm run db:generate`.

```sh
npm run dev
```

Open the printed loopback URL (normally `http://localhost:5173`). Visit `/signin-with-chatgpt?return_to=/` on that local server to use the starter's simulated development identity. Mock sign-in is confined to loopback development and is absent from production builds. Local D1 and hosted D1 are separate; the repository contains no local database state or saved personal reviews.

Edit the review renderer in `outputs/payoff_exploration/render-review.cjs`, then regenerate the report. Review API/auth code lives in `site/app/`, and the D1 schema/migrations live in `site/db/` and `site/drizzle/`. `npm run build` creates deployable framework output; `npm start` previews the built Worker locally. The source defaults to the portable profile when ignored checkout-local settings are absent.

If Windows tooling misresolves the npm shell shim, invoke npm's official `npm-cli.js` with Node from your own installation. No machine-specific shim or hardcoded tool installation path is committed.

## Hosted Site and private review data

Live private page: [Top Expert Pitch Review](https://top-expert-payoff-lab.adoredcivet6.chatgpt.site).

The non-secret `.openai/hosting.json` retains the existing Site project ID and logical `DB` binding so authorized future work can update the same Site. The project ID is an identifier, not a credential. This repository push does not redeploy or alter that Site. Do not create a duplicate Site or change its audience when continuing existing hosted work.

Hosted selects and notes are external records in Sites D1, scoped to the authenticated Site user and stable pitch ID. They are **not** committed here. Signing into the existing private Site on another computer loads the same account reviews; refresh an already-open page for current saved state. Export/import backups can be made from the Site when desired.

Publishing requires the authorized Sites workflow and fresh short-lived source credentials. Those credentials, authentication tokens, private database files, and personal review exports are not included. Preserve the existing owner-private access policy. GitHub source pushes and Sites deployments are separate operations.

## Excluded local material

Dependency installations/caches, nested Git metadata, generated framework builds, Wrangler/Miniflare databases and logs, machine-specific runtime settings, environment secrets, deployment archives, copied full-text/page evidence caches, duplicate text drafts, QA import fixtures, and throwaway research/generation scripts were excluded. Substantive datasets, coverage/provenance metadata, original seeds, all pitches/rankings, reproducible report generators, current website source, schema, and lockfile are retained.
