# SPEC

## Scope

- Replace pricing presentation with Image Menu and Digital Menu cards and a 14-day free-trial prompt.
- Add an immutable `image` or `digital` project menu type, defaulting existing rows to `image`.
- Add a pre-editor menu-type selection screen for new projects.
- Add structured Digital Menu categories and items with per-language text.
- Add item out-of-stock state and recurring category schedules.
- Add mobile-first owner editing and public Digital Menu rendering.
- Store a project-level currency code and format numeric public prices while preserving non-numeric price text.
- Detect the initial restaurant timezone from the editor browser when the project still uses the default UTC value.
- Remove out-of-stock items and resulting empty sections from public responses.
- Move language add/remove/select controls into a reusable editor toolbar and add General Settings/Menu Editor tabs to Image Menu projects.
- Keep Digital Menu structure shared while newly added language translations start blank and remain unavailable publicly until populated.
- Add one optional, language-independent cover asset to Digital Menu projects and expose it separately from Image Menu content assets.

## Affected Areas

- Frontend pricing copy and layout.
- Frontend project list, new-project flow, editor, project contracts, and public slug page.
- Backend project contracts/domain/persistence.
- New Digital Menu domain, application service, persistence, endpoints, and migration.
- Backend endpoint and service tests.
- Existing asset storage/repository contracts and a migration that distinguishes asset purpose.
- README product direction and task status.

## Technical Approach

- Preserve `standard` and `plus` as billing tier identifiers while relabeling them publicly.
- Store menu type on projects and backfill existing data to `image`.
- Store Digital Menu currency on projects with EUR as the existing-project default.
- Store category/item identity and ordering independently from translations.
- Reference project language-variant IDs from translation rows.
- Save a complete Digital Menu document transactionally through a dedicated endpoint.
- Represent schedules as weekday, start time, and end time rows and evaluate them using the project IANA timezone.
- Return structured content in authenticated and public contracts; filter timed categories consistently on the backend.
- Split format-specific editor UI into focused components instead of expanding the existing project editor indefinitely.
- Add an asset purpose with existing rows backfilled as menu content; use owner-only cover upload/removal endpoints that replace the prior cover and clean up stored files.
- Include the optional cover in owner and public project responses while excluding it from regular project asset lists and ordering.

## Assumptions

- One project represents one hosted menu.
- Menu type cannot be changed after project creation in this version.
- Prices are editable display text in the first version to support local formats and entries such as market price.
- Out-of-stock items remain visible but are clearly unavailable.
- Category schedules recur weekly and may span midnight.
- The free trial continues through the current contact-based acquisition flow.

## Risks

- Existing project contract tests and fake repositories will need coordinated updates.
- Daylight-saving and overnight schedules need explicit tests.
- The existing multi-request image editor save flow remains non-transactional, while Digital Menu content will use a transactional document save.
- Storage and database replacement cannot be fully atomic; replacement should retain the old cover until the new file and row are saved successfully.
