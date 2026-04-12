# BRIEF

## User-Facing Goal

Uploads should be additive and safe; a new upload must not break old images anywhere else in the app.

## Success Criteria

- Existing project images continue loading after any new upload.
- New uploads still save and render correctly.

## UX Notes

- None; storage/rendering bugfix.

## Risks Or Tradeoffs

- The regression may involve already-saved metadata from before the WebP conversion change.
