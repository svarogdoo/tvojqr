# SPEC

## Scope

- Update `frontend/src/routes/+page.svelte`.
- Simplify `frontend/src/lib/components/Services.svelte`.
- Add a new who-we-are section component.
- Remove the homepage examples and contact sections from the route.

## Affected Areas

- Homepage layout
- Services section
- Navigation flow
- README task tracking

## Technical Approach

- Keep the hero as-is.
- Replace the heavier homepage sections with a lighter services block and a short story section.
- Preserve the existing contact page and nav routing.

## Assumptions

- The existing contact route is sufficient for inbound leads.

## Risks

- The homepage may feel too sparse if the new services block is not visually strong enough.
