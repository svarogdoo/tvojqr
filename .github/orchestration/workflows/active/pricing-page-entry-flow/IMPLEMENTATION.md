# IMPLEMENTATION

## Planned Changes

- Create `frontend/src/routes/pricing/+page.svelte`.
- Update homepage CTA links from `/create-new` to `/pricing`.
- Add a pricing link to the navigation.
- Remove `frontend/src/routes/create-new/+page.svelte` and `+page.server.ts`.
- Update README task markers for the pricing work.
- Simplify the pricing page copy and card layout.
- Add a monthly/annual toggle that updates the displayed price.
- Switch pricing to USD and rename the middle plan to Standard, then the top paid plan to Plus.

## Files Touched

- `frontend/src/routes/pricing/+page.svelte`
- `frontend/src/lib/components/Hero.svelte`
- `frontend/src/lib/components/Services.svelte`
- `frontend/src/lib/components/HowItWorks.svelte`
- `frontend/src/lib/components/Examples.svelte`
- `frontend/src/lib/components/Navigation.svelte`
- `frontend/src/lib/translations.ts`
- `frontend/src/routes/create-new/+page.svelte`
- `frontend/src/routes/create-new/+page.server.ts`
- `README.md`

## Verification

- Confirm `/pricing` renders.
- Confirm no in-app links still point to `/create-new`.
- Confirm the removed route files are gone.
- `npm run check`
- `npm run build`

## Notes

- Polar integration stays visual-only for now.
- Keep the page more visual than explanatory.
- The paid plans should read like a soft, color-accented SaaS pricing table.
