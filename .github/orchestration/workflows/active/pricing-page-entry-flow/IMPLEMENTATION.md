# IMPLEMENTATION

## Planned Changes

- Update World Cup pricing to dollars.
- Route pricing contact CTAs to `/contact`.
- Add a `/contact` route with form fields for name, email, optional uploads, optional languages, and a message.
- Send submitted contact requests to email and show a success confirmation.
- Update README task markers for the contact request flow.

## Files Touched

- `frontend/src/routes/pricing/+page.svelte`
- `frontend/src/routes/contact/+page.svelte`
- `frontend/src/routes/contact/+page.server.ts`
- `frontend/src/lib/components/Navigation.svelte`
- `README.md`
- `.github/orchestration/workflows/active/pricing-page-entry-flow/*.md`

## Verification

- Confirm `/pricing` renders.
- Confirm `/contact` renders and submits successfully.
- `npm run check`
- `npm run build`

## Notes

- Polar integration stays visual-only for now.
- Keep the page more visual than explanatory.
- The contact request flow should keep the success message short and reassuring.
