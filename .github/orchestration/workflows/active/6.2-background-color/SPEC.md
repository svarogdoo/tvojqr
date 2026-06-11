# SPEC

## Scope

Add a per-project public page background color setting.

## Affected Areas

- Project database schema
- Backend project domain/contracts/repository/service
- Dashboard project editor
- Public slug page
- README task list

## Technical Approach

- Add `background_color` column to `projects` with a default matching the current visual background.
- Validate background color as a six-digit hex value.
- Include `backgroundColor` in project detail and public responses.
- Save `backgroundColor` through create/update project requests.
- Apply the saved value as an inline `background-color` style on the public page wrapper.

## Assumptions

- `#f8f7f3` is an acceptable default match for the current soft page background.

## Risks

- Existing local databases need migration execution on startup, which the dev script already enables.
