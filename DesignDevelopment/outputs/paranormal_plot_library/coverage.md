# Coverage and research limitations

| Harvested lane | Complete | Incomplete | No ending |
|---|---:|---:|---:|
| The X-Files | 30 | 0 | 0 |
| Goosebumps | 32 | 0 | 0 |
| SCP Wiki | 20 | 0 | 12 |
| Classic fiction | 34 | 0 | 0 |
| The Twilight Zone | 10 | 0 | 0 |
| Radio drama | 3 | 0 | 0 |
| Attributed folklore | 5 | 0 | 0 |

**Actual record counts above are distinct from the source-map estimates.** The collection is an initial pass, not an exhaustive survey. The full machine-readable lane manifests record catalog slices, considered items, remaining items and access limitations.

## What was harvested

- X-Files: 30 selected standalone paranormal/monster episodes across seasons 1–3; 34 sources considered, 39 other early-season catalog IDs unprocessed. Later seasons remain unprocessed. Some cases use aliens, mutants, or anomalous science.
- Goosebumps: all original-series books 1–32. Original books 33–62 and later series remain. Cached detailed plot passages supply some endings when direct Fandom pages are blocked; individual evidence labels preserve this limitation.
- SCP: 60 primary pages considered within 002–199; 20 developed incident/reveal arcs retained as complete and 12 source records separated without endings. Other considered descriptions and unprocessed IDs are listed in the manifest; linked 087/093 logs remain unread.
- Classics: all eight stories in Ghost Stories of an Antiquary and all seven in More Ghost Stories of an Antiquary, selected later James stories, seven other classic short works, and three classic novels/novellas. The first eight were checked against primary Gutenberg text endings; the rest use detailed synopses.
- Twilight Zone: ten selected original-series episodes after discovery identified accessible detailed episode synopses; most original episodes and all revivals remain unprocessed.
- Radio: see lane manifest for exact scripts and completed units.
- Folklore: five specific published versions from two Folktexts thematic pages, with collector, volume, year and page provenance retained. Other versions and thematic collections remain unharvested.


## Source fidelity and limits

Each plot is a short original paraphrase with precise source links. Secondary synopses often provide endings absent from publisher descriptions. Evidence quality distinguishes full text, detailed synopsis, and cached synopsis excerpts. A verified ending can be bleak or ambiguous; no explanatory certainty is added to an ambiguous source. A static anomalous object description is not treated as a finished plot.

Summaries normally use two or three sentences and omit names, subplots, and scene order unless needed for the core outcome. Technical metadata and ending-evidence notes live in JSON/CSV. Similar tropes remain distinct; duplicates of the same catalog work were checked. Source versions and adaptations are not silently combined.

Primary viewing/reading remains the main verification gap for copyrighted TV and books. The review checks source-backed outcomes, not every incidental detail of every edition. Cached excerpts are weaker access evidence than an opened page, even when they explicitly contain resolution and twist.

## Broad discovery and next collection choices

The source map and collection queue cover a wider landscape than the harvested lanes. Catalog sizes and eligible volumes are estimates, never harvested-count claims. Serialized audio dramas need completed narrative arcs or independently complete embedded stories; arbitrary episodes must not be counted as finished plots. Folklore needs attributable versions, with multiple tellings kept separate and publication status recorded.

No adaptation, proposed game case, original plot, mechanic or story critique was generated. The previous three-story trial files remain separate and were not imported into this library.

## Deliverables

- plots.md — readable library with no-ending appendix.
- plots.json — structured records with per-source evidence notes.
- plots.csv — flat importable table.
- source_map.md / source_map.json — discovered collection landscape.
- collection_queue.json — prioritized harvesting queue.
- coverage_manifest.json — detailed catalog coverage and gaps.
- quality_review.json — count, deduplication, length and review audit.