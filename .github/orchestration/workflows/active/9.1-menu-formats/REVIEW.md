# REVIEW

## Findings

- Fixed: blocked menu mutation/save after a failed initial load to prevent empty replacement.
- Fixed: included Digital Menu dirty state in route navigation and unload guards.
- Fixed: prevented project settings saves from discarding unsaved Digital Menu changes.
- Fixed: made Digital Menu language codes immutable after project creation and warned that removing a language deletes its translations.
- Fixed: carried authoritative language mutation responses through the multi-step project save so deleted languages do not reappear from stale client state.
- Fixed: avoided counting a Digital Menu public view twice.
- Fixed: enforced Digital Menu entitlement on owner read/write/availability operations.
- Fixed: rejected equal schedule start/end times and added normal/overnight schedule tests.
- Fixed: removed unavailable items and resulting empty sections from public Digital Menu responses.
- No unresolved implementation-specific high-severity finding remains.
- No unresolved finding in the Digital Menu cover implementation: cover assets are owner-scoped, limited to Digital Menu projects, excluded from menu-page ordering, and replaced only after the new asset is persisted.
- Added service and endpoint coverage for replacement cleanup, removal isolation, owner checks, format checks, upload routing, and the public response contract.

## Residual Risks

- The editor and public page have compile/build coverage but no browser automation or visual-regression suite.
- Cover-image cropping uses responsive `object-cover`; focal-point selection is not part of this version.
- Digital menu persistence was migration-smoke-tested locally; repository behavior does not yet have a PostgreSQL integration-test fixture.
- Browser timezone detection and numeric currency formatting do not yet have browser automation coverage.
- A pre-existing project deletion flaw can delete dependent assets/slugs before owner scope is checked. It was outside this approved feature scope and is recorded as README task `12.2.d`.

## Verification Notes

- Frontend type check: passed with 0 errors and 0 warnings.
- Frontend production build: passed.
- Backend tests: 41 passed.
- Latest backend tests: 47 passed.
- Diff whitespace check: passed.
- PostgreSQL migration: applied; `menu_type`, `time_zone`, and five Digital Menu tables verified.
- Pricing refinement frontend check and production build: passed.
- Monochrome card and included-service refinement frontend check and production build: passed.
- Digital Menu tabs preserve the mounted editor and its dirty state while switching views.
- The stale local backend was restarted; the protected Digital Menu endpoint now returns `401` without authentication instead of the previous route-level `404`.
- Fixed PostgreSQL `smallint` weekday materialization by returning schedule weekdays as API-compatible integers.
- Currency migration applied locally and `projects.currency_code` was verified.
- Frontend no longer contains bright `emerald-*` utility colors.
- Confirmed category translations remain separate in persistence and the editor now refreshes section-name fields explicitly by selected language.
- Timezone options use browser-supported IANA zones with a fallback list; selected city time refreshes once per minute.
- No backend process was started by the assistant during this follow-up; the listener on port `5115` belongs to the user's existing `npm run dev` session.
- Fixed review findings around default-language changes, stale deleted-language translations, language mutation races, browser timezone configuration state, public field-level fallback, and image touch ordering.
- Local timezone configuration-state schema was applied directly without starting another backend process.
- Fixed the public language selector's hidden reactive dependency so it updates after Digital Menu content loads.
- Fixed translated render helpers to receive the selected language explicitly so category, item, description, and price output refresh when switching languages.
- Simplified public Digital Menu presentation by removing the format label, owner name, and section item counts.
- Digital Menu cover verification passed: frontend check/build, backend tests, diff whitespace check, and local migration application.
- Asset-purpose migration applied locally and the non-null `assets.purpose` column/default were verified.
- Refined the cover layout so the photo fills the complete header and the restaurant name sits directly over the image.
- Expanded the cover to the viewport edges and top edge, with the language selector overlaid on the photo while menu content remains constrained.
- Removed the cover's bottom corner radius so the full-width image has straight edges on every side.
- Reduced the full-width cover and restaurant title to a compact decorative banner so menu content remains visually primary.
- Tightened public section cards, headings, inter-section gaps, item rows, and description spacing for a more compact menu scan.
- Replaced section bubbles with flat content, removed duplicate horizontal padding, and changed category navigation to a full-width strip with a bottom shadow.
- Restored all sections as a stacked page and kept the category strip as sticky, horizontally scrollable anchor navigation directly below the cover.
- Fixed the sticky navigation's horizontal offset by replacing transform positioning with a viewport-margin calculation.
- Clipped page-level horizontal overflow on mobile while preserving horizontal scrolling inside the section navigation.
