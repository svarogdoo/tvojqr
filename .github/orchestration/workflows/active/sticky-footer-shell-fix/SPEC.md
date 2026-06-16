# SPEC

## Scope

- Update the homepage shell.
- Update the pricing page shell.
- Update the contact page shell.
- Update shared layout/background behavior if needed.

## Affected Areas

- Route shells
- Footer positioning
- Global page background behavior

## Technical Approach

- Wrap page content in a flex column shell with a full-height container.
- Let the main content grow and keep the footer at the bottom.

## Assumptions

- The footer remains page-local instead of moving into the global layout.

## Risks

- A page with an unwrapped root may still need adjustment later.
