# IMPLEMENTATION

## Planned Changes

- Restore the Custom plan presentation in the pricing area.
- Add a one-line Custom plan note below the main grid.
- Add a quote-style translation/redesign section below that.
- Update README task markers for the pricing work.

## Files Touched

- `frontend/src/routes/pricing/+page.svelte`
- `README.md`
- `.github/orchestration/workflows/active/pricing-page-entry-flow/*.md`

## Verification

- Confirm `/pricing` renders.
- `npm run check`
- `npm run build`

## Notes

- Polar integration stays visual-only for now.
- Keep the page more visual than explanatory.
- The lower cards should read as service-oriented add-ons rather than plan tiers.
