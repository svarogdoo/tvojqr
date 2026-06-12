# SPEC

## Scope

- Keep the `/pricing` route and refine its content blocks.
- Restore a Custom plan section.
- Add a separate translation/redesign service card below the main pricing grid.
- Keep Polar-ready visual plan markup only.

## Affected Areas

- Pricing page layout
- README task tracking
- Workflow docs

## Technical Approach

- Build the pricing page as a static Svelte route with reusable sections.
- Use buttons and `data-*` hooks to keep the plan cards ready for Polar wiring later.
- Keep the custom plan visually close to the existing pricing cards.
- Make the translation/redesign card a separate promotional block.

## Assumptions

- Pricing amounts are not final yet.
- The example/service sections can be purely visual.

## Risks

- Existing pricing copy may need another pass if the service cards change again.
