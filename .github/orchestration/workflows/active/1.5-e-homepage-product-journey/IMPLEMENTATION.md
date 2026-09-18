# IMPLEMENTATION

## Planned Changes

- Added localized product-journey copy for English, Spanish, Italian, and Croatian.
- Built a responsive branching journey with CSS-only connector lines and inline icons.
- Replaced homepage Hero and Services imports/usages with the journey.
- Preserved the existing `#services` navigation target and untouched lower homepage sections.
- Removed the small HostingQr label and reduced the opening question size.
- Replaced the example buttons with linked static phone previews: existing menu artwork for Image Menu and lightweight menu markup for Web Page Menu.
- Added a four-language sample panel to Translations.
- Added a real styled QR code targeting `https://hostingqr.com/example` using the existing lazy-loaded QR library.
- Added a static Google Maps-style business preview with a location pin, rating, and Menu action.
- Made the shared visual cards stack on narrow screens and align horizontally from the small breakpoint.
- Refined all shared cards to keep their visual on the left and copy on the right at every viewport size.
- Removed redundant icons from Translations, QR Code, and Google Maps.
- Replaced the translation preview with a compact static panel showing English, Spanish, Italian, and Croatian flag/wording rows.
- Applied the approved reference-led art direction with Georgia-based editorial headings, italic accent text, localized introductory subtitles, and warm ivory/sage background shapes.
- Restyled the journey cards with softer translucent surfaces, restrained shadows, and circular arrow details.
- Added curved connector joints and leaf, globe, and settings milestone nodes.
- Reworked product cards to place copy beside the phone previews on wider screens while retaining vertical internals inside the narrow mobile branches.
- Reworked setup and final cards into compact icon/copy/flow-arrow compositions.
- Removed all remaining small icon boxes from product and setup cards.
- Removed all decorative milestone icons from the connector line while preserving its curved path.
- Merged Image Menu replacement and Web Page Menu editing into one localized Easy changes card.
- Added a localized final contact callout with direct WhatsApp and `support@hostingqr.com` email actions.
- Compacted the upper journey with reduced section padding, header spacing, connector heights, card padding, and desktop phone width so the free setup row appears sooner.
- Changed the accented final headline word from italic to regular styling.
- Added localized keyword metadata and highlighted only the appropriate free term in both setup titles across all four languages.
- Thinned the phone-preview bezel, speaker, padding, and corner radii without changing preview dimensions.
- Further reduced the phone top padding and made the speaker shorter, narrower, and closer to the outer edge.
- Restyled the FAQ accordion with serif questions, translucent surfaces, sage circular toggles, clearer open-state separation, and responsive touch-friendly spacing without changing its content.
- Replaced the first FAQ's outdated self-service steps with localized assisted-setup copy explaining that HostingQr performs the initial setup and the customer controls the account afterward.
- Replaced the Image Menu phone artwork with Paname's first live Italian menu asset while retaining `/paname` as the full-example destination.
- Removed the dining-experience claim from all localized introductory subtitles.
- Added a compact localized Pricing link directly below the subtitle so mobile visitors can reach `/pricing` without relying on the desktop navigation.
- Removed the circular flow arrows from Free redesign/setup and added only the requested inline design and gear icons before their highlighted titles.
- Reduced the Google Maps preview dimensions, padding, and typography below 420px to prevent awkward wrapping while leaving larger breakpoints unchanged.
- Updated README task status.

## Files Touched

- `frontend/src/lib/components/ProductJourney.svelte`
- `frontend/src/lib/homepageCopy.ts`
- `frontend/src/routes/+page.svelte`
- `README.md`
- `.github/orchestration/workflows/active/1.5-e-homepage-product-journey/*`
- `frontend/src/lib/components/WhoAreWe.svelte`

## Verification

- `npm run check`: passed with 0 errors and 0 warnings.
- `npm run build`: passed.
- `git diff --check`: passed.
- Targeted Prettier check for `ProductJourney.svelte`: passed.
- `npm run lint`: not clean because the repository-wide command includes generated `.svelte-kit` output and existing unformatted source files; the new component was not reported.

## Notes

The Image Menu phone opens `https://hostingqr.com/paname`; the Web Page Menu phone opens `/example`. Static previews avoid loading both public pages and incrementing their analytics whenever the homepage loads.

The QR code is generated locally in the browser and does not call an external QR service. The translation and map previews are static and make no API requests.

The shared cards intentionally preserve the latest approved visual-left/no-icon composition instead of copying the reference's shared-card ordering.

The previously separate final editing branches are now represented by one shared message, so the journey no longer splits after Google Maps.
