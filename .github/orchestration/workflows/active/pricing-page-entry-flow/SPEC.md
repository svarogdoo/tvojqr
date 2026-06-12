# SPEC

## Scope

- Add a new `/pricing` route.
- Remove `frontend/src/routes/create-new`.
- Update landing-page CTAs that point to the removed route.
- Add Polar-ready visual plan markup only.

## Affected Areas

- Frontend routing
- Landing page CTA links
- Navigation labels
- README task tracking

## Technical Approach

- Build the pricing page as a static Svelte route with reusable sections.
- Use buttons and `data-*` hooks to keep the plan cards ready for Polar wiring later.
- Delete the old intake route files so the path no longer exists.

## Assumptions

- Pricing amounts are not final yet.
- The example sections can be purely visual.

## Risks

- Existing marketing links may still point at the removed route if missed.
