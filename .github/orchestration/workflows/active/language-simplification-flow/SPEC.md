# SPEC

## Scope

- Update the language store to only expose English and Spanish.
- Add Spanish copy to the translation map.
- Update any hardcoded language option lists that still expose extra locales.

## Affected Areas

- Language store
- Translation map
- Navigation / language picker
- Dashboard language options

## Technical Approach

- Keep English as the base translation object.
- Add Spanish overrides for the current visible UI.
- Reduce the language picker to two options.

## Assumptions

- The existing English copy remains the source of truth.

## Risks

- Some legacy locale strings may remain in the translation file for now, even if they are no longer selectable.
