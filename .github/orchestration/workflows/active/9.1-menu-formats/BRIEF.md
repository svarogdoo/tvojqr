# BRIEF

## User-Facing Goal

Restaurants should immediately understand whether they want to host their existing designed menu or manage a true editable digital menu. Digital Menu owners should be able to update availability and content comfortably from a phone.

## Success Criteria

- Pricing has only two paid product cards and a separate 14-day trial prompt.
- Product descriptions explain outcomes rather than technical limits.
- New-project creation requires a clear menu-type choice before editing.
- The Digital Menu editor supports categories, translated item content, prices, stock state, ordering, and timed category visibility.
- Public digital menus are clean, responsive, translated, and respect stock/schedule settings.
- Existing Image Menu projects continue to load and edit normally.

## UX Notes

- Mark Digital Menu as the highlighted option.
- Use large touch targets and compact collapsible content cards in the editor.
- Make out-of-stock a direct item action rather than hiding it inside an edit form.
- Keep structural edits explicit and understandable; avoid a dense spreadsheet interface.
- Use language tabs and visible missing-translation cues.
- Pricing refinement: center each product name with a distinct icon, remove the popularity tag, link the Image Menu example, show a Digital Menu example placeholder, and keep the trial prompt compact and pastel.
- Pricing refinement: present both paid products with equal monochrome white-card styling and call out the included free redesign/setup and translation service.
- Digital Menu project pages use separate General Settings and Menu Editor tabs so mobile owners only see the controls relevant to the current task.
- Digital Menu editing starts with compact currency and automatically detected timezone controls, then moves directly into language and section management.
- Public menus omit unavailable items entirely and do not show sections left empty by availability filtering.
- Newly introduced Digital Menu surfaces use the established neutral and muted-sage palette rather than bright emerald accents.
- Section-name editing clearly follows the active content language, including missing-translation feedback.
- Timezone configuration uses a standard browser-supported zone selector with the selected city and live local time.
- Both menu types manage languages from a shared toolbar inside Menu Editor rather than from General Settings.
- Image Menu content is shown one selected language at a time, matching the Digital Menu editing flow.
- Digital Menu General Settings provides one cover-image upload with preview, replacement, and removal.
- A saved cover fills a full-viewport-width header from the top edge behind a centered restaurant name and readability overlay; menus without a cover retain the clean title-only header.

## Risks Or Tradeoffs

- A full digital menu is materially larger than the current image-only model.
- Timed visibility requires explicit timezone handling.
- Translation completeness must not block editing the default language.
- Pricing promises must avoid implying unlimited manual translation labor.
- Cover cropping must remain attractive across unknown source-image dimensions and narrow mobile screens.
