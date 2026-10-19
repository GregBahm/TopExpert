# Case Builder

Run `node DesignDevelopment/CaseBuilder/server.cjs` from the repository root, then open http://127.0.0.1:8766/. No install step. The pitch reviewer runs separately on 8765.

## Data ownership

Each tracked `cases/<id>.json` file uses case schema version 2:

- Case content: title, opening, implementation notes, initial scene flags, opportunities, requirements, costs, outcomes, and endings.
- `testSetup`: a separate namespace for this case's remembered test preset: selected investigator IDs, skill overrides, starting inventory and resources, and optional investigator overrides. These values configure testing; they are not case mechanics.

Investigator baseline skills come from the shared portrait catalog. Shared constants live in `engine.mjs` as `GAME_RULES`: a six-sided die, skill multiplier 1, starting health 3, and setup skill maximum 6. Cases cannot override the game rules or define a second investigator roster. Old version 1 definitions are not supported.

The engine receives case content and a separately assembled test roster and supplies when starting a run. It clones them into disposable runtime state. Case flags belong to the story; starting team and supplies belong to the test preset.

## Editing and testing

Dev has three secondary tabs:

- **Pitch:** source premise, pitch and review notes.
- **Case Settings:** case title, opening, implementation notes, initial scene flags and endings.
- **Implementation:** the complete opportunity dependency graph, including success, failure, expiry and ending connections. Select a card to edit its title, body, success/failure text, slot type, starting availability, repeatability and optional deadline with expiry text; click outside to return to its player presentation. Drag the stage to pan.

Valid edits autosave to the tracked case file after a 300 ms pause. Local browser drafts and revision-checked atomic server writes protect pending edits. The header stays clear when saving works and warns about save errors. No cloud persistence is used.

**Test Setup** contains investigator selection, effective skill stars (0–6), and starting inventory/resource quantities. Purple stars indicate overrides. Clicking the first star when the skill is 1 sets it to 0. Investigators without documented skills remain disabled. Presets are remembered per case; changing them applies on **Restart test**, preserving the current run until then. The optional **Current test overrides** panel changes only disposable runtime state.

**Test** shows one column per turn, with only the current column accepting cards from the inventory tray. Drag investigators or supplies into matching slots. Cards move physically between tray and slots; tray order has no gameplay effect. Complete turn resolves prepared actions. Past cards retain assigned card ghosts, results and inventory changes; unattempted checks and empty slots disappear. Unlock connections show the first unlocking opportunity, choosing the topmost card when several unlock the same opportunity. Matching opportunities in consecutive turns use dotted grey connections. Drag the stage to pan. Restart clears the run; runtime progress is not saved.

## Current mechanics

Checks use `1d6 + sum(relevant skills) + held-item bonuses >= target`. Bonuses are evaluated after costs. Missing skills count as zero. Check stars show outlined difficulty, solid guaranteed totals and half-filled possible totals; resolved checks show the actual total.

Each investigator can occupy at most one opportunity per turn. Actions resolve in preparation order and recheck costs and requirements. Items/resources cannot fall below zero. Costs apply on either success or failure. Newly unlocked opportunities become assignable next turn. Nonrepeatable completed opportunities cannot be reactivated by an unlock.

A turn advances by the longest assigned action duration, or one turn when waiting. This scheduling rule remains provisional; multi-turn occupied investigators are not modeled. Deadlines expire when the next turn exceeds the specified deadline, applying the opportunity's expiry effect.

Outcome inventory/resources are numeric deltas; skill and health changes affect the assigned investigator. Flags represent case clues/state. Unlock/remove fields refer to opportunity IDs; endings refer to ending IDs. Requirements support flags, forbidden flags, supplies, minimum skills and investigator traits. Editor notes do not affect gameplay.

## Focused checks

Run `node --test DesignDevelopment/CaseBuilder/smoke.test.mjs DesignDevelopment/CaseBuilder/turn-board.test.mjs DesignDevelopment/CaseBuilder/turn-history.test.mjs`. These use disposable fixtures and deterministic rolls to check persistence/conflicts, checks, branching, costs, runtime isolation and turn snapshots. They do not write authored cases or pitch reviews. Routine presentation changes use a focused browser check.

## Interaction editor mockup

Deadline and Skill check are independent toggles. Deadline reveals its turn count and deadline failed text and retains its existing runtime behavior. Skill check reveals attribute/difficulty rows, Any/All when several rows exist, required/optional investigator counts (defaults 1/0), and success/failure text. Settings persist in each opportunity’s `interactionDesign` field. Disabling a section hides it without erasing its configuration. This is the authoring UX phase: individual checks, Any/All, and multi-investigator requirements do not yet change Test mechanics. The existing runtime check remains in place until the planned mechanics overhaul. Item and outcome editing are the next UI steps.
