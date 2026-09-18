# SPEC

## Scope

Replace the homepage Hero and Services rendering with one localized product-journey section. Do not alter the preserved Who Are We/FAQ or Footer sections.

## Affected Areas

- Homepage component composition.
- Homepage localized copy.
- New responsive product-journey component.
- Front-page README task tracking.

## Technical Approach

- Add a focused Svelte component containing semantic headings, link actions, inline SVG icons, and CSS/Tailwind connector geometry.
- Store journey copy in `homepageCopy.ts` for all supported homepage languages.
- Keep the section id as `services` so the existing navigation link remains valid.
- Use responsive two-column branch grids and centered shared nodes without JavaScript layout behavior.

## Assumptions

- "Web Page Menu" describes the existing Digital Menu product.
- Image Menu example points to `https://hostingqr.com/paname`.
- Web Page Menu example points to `/example`.

## Risks

- Connector alignment can regress at very narrow or wide viewport sizes; verify the CSS structure and responsive limits.
