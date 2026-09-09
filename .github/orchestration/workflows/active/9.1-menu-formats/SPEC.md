# SPEC

## Scope

- Replace pricing presentation with Image Menu and Digital Menu cards and a 14-day free-trial prompt.
- Add an immutable `image` or `digital` project menu type, defaulting existing rows to `image`.
- Add a pre-editor menu-type selection screen for new projects.
- Add structured Digital Menu categories and items with per-language text.
- Add item out-of-stock state and recurring category schedules.
- Add mobile-first owner editing and public Digital Menu rendering.

## Affected Areas

- Frontend pricing copy and layout.
- Frontend project list, new-project flow, editor, project contracts, and public slug page.
- Backend project contracts/domain/persistence.
- New Digital Menu domain, application service, persistence, endpoints, and migration.
- Backend endpoint and service tests.
- README product direction and task status.

## Technical Approach

- Preserve `standard` and `plus` as billing tier identifiers while relabeling them publicly.
- Store menu type on projects and backfill existing data to `image`.
- Store category/item identity and ordering independently from translations.
- Reference project language-variant IDs from translation rows.
- Save a complete Digital Menu document transactionally through a dedicated endpoint.
- Represent schedules as weekday, start time, and end time rows and evaluate them using the project IANA timezone.
- Return structured content in authenticated and public contracts; filter timed categories consistently on the backend.
- Split format-specific editor UI into focused components instead of expanding the existing project editor indefinitely.

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
