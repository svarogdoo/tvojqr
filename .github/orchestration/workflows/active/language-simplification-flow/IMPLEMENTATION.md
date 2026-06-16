# IMPLEMENTATION

## Planned Changes

- Update `frontend/src/lib/stores/language.ts` to `en` and `es` only.
- Add Spanish translations in `frontend/src/lib/translations.ts`.
- Update hardcoded language option lists.

## Files Touched

- `frontend/src/lib/stores/language.ts`
- `frontend/src/lib/translations.ts`
- `frontend/src/lib/components/*.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `README.md`

## Verification

- `npm run check`
- `npm run build`

## Notes

- Keep the locale change narrow and low-risk.
