# INTENT

## User Request

Replace the current pricing tiers with Image Menu and Digital Menu, add a two-week free-trial option, and then build both menu types into project creation and editing. Digital menus need translations, quick out-of-stock controls, timed sections, and a clean mobile-first editing experience.

## Requested Outcome

- Pricing presents two clear paid products: Image Menu at EUR 7 monthly/EUR 70 annually and Digital Menu at EUR 10 monthly/EUR 100 annually.
- A separate two-week free-trial prompt remains visible.
- New projects begin with an Image Menu or Digital Menu choice.
- Existing projects continue as Image Menus.
- Digital Menus support translated categories and items, prices, availability, ordering, and scheduled sections.
- Owners can make common changes quickly from a phone.

## Constraints

- Preserve the current Image Menu upload and public-page behavior.
- Keep internal billing IDs `standard` and `plus` for this pass.
- Keep the current visual language and make the smallest maintainable structural changes.
- Initial translations remain manually entered rather than machine generated.

## Open Questions

- None blocking implementation. The initial schedule model will use recurring weekly time windows and a project timezone.
