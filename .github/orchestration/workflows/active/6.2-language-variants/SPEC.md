# SPEC

## Scope

Implement language variants for project editor uploads and public page display.

## Affected Areas

- Database migrations
- Backend project/assets contracts, repositories, services, endpoints
- Project editor UI
- Public slug page UI
- README task list

## Technical Approach

- Add `project_language_variants` with `language_code`, `display_name`, `is_default`, and `sort_order`.
- Seed/ensure a default English language for existing projects.
- Return `languages` on project/public responses.
- Accept `languageCode` in asset upload form data.
- Allow image ordering per language subset.
- Add add/remove language endpoints.
- Filter public assets by selected language.

## Assumptions

- `en` is the default language code for existing projects.
- Existing `default` assets are treated as `en` through migration.

## Risks

- Removing a language deletes its asset metadata and storage objects through the app path for explicit language deletion.
