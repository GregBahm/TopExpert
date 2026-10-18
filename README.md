# Top Expert

The authoritative Git checkout and local review tool are in this project root.

- `DesignDevelopment/Top Expert Base Design Doc.md`: original design document and embedded images.
- `DesignDevelopment/outputs/paranormal_plot_library/`: research library, coverage and provenance.
- `DesignDevelopment/outputs/research_work/`: lane datasets and unique preliminary source/catalog metadata.
- `DesignDevelopment/outputs/payoff_exploration/`: 50 concepts, 500 ranked pitches, the plain local review page/server, and current `reviews.json`.

Start the review server from this root:

```sh
node DesignDevelopment/outputs/payoff_exploration/review-server.cjs
```

Open **http://127.0.0.1:8765**. Statuses and notes save to the `reviews.json` beside the server. See `DesignDevelopment/README.md` for the local workflow. Review data is shared through ordinary Git file commits, not automatic cloud sync.

The separate [Case Builder](DesignDevelopment/CaseBuilder/README.md) prototypes editable case mechanics and player test runs:

```sh
node DesignDevelopment/CaseBuilder/server.cjs
```

Open **http://127.0.0.1:8766/**. Dev edits autosave to `DesignDevelopment/CaseBuilder/cases/*.json`; Test runtime overrides affect only the disposable run; Test Setup selections and skill overrides save per case.
