# SPEC

## Scope

- Keep the `/pricing` route and route contact CTAs to `/contact`.
- Add a `/contact` route with an email submission form.
- Support optional file uploads, optional language details, and a free-form message.
- Keep Polar-ready visual plan markup only.

## Affected Areas

- Pricing page layout
- Contact page route
- Navigation links
- README task tracking
- Workflow docs

## Technical Approach

- Build the pricing page as a static Svelte route with reusable sections.
- Add a new contact route with a SvelteKit action that sends email via nodemailer.
- Use multipart form data for optional uploads and normal fields for request details.
- Use buttons and `data-*` hooks to keep the plan cards ready for Polar wiring later.

## Assumptions

- Pricing amounts are not final yet.
- Email credentials are available in `EMAIL_USER` and `EMAIL_PASS`.

## Risks

- Email delivery depends on environment variables and Gmail transport compatibility.
