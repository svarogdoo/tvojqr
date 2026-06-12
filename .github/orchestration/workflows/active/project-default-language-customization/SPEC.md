# SPEC

## Scope

Add editable default-language support to project creation and project settings.

## Affected Areas

- Backend project contracts, service, and language-variant repository
- Project editor UI and form payloads
- README task tracking

## Technical Approach

- Extend project create/update requests with default-language fields.
- Seed new projects with the selected default language instead of hardcoded English.
- Add repository support to rename the default language variant and move its assets when the default changes.
- Add a project-editor control for choosing the default language.

## Assumptions

- Existing projects already have a default language row from the prior migration.

## Risks

- If the selected default language collides with an existing non-default variant, the save must reject it cleanly.
