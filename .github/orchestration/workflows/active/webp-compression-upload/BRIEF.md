# BRIEF

## User-Facing Goal

Users upload images the same way as before, but the product stores and serves a more optimized format automatically.

## Success Criteria

- Uploaded raster images are saved as WebP.
- Stored asset metadata reflects the new file type and filename.
- Existing upload UI keeps working.

## UX Notes

- This should be invisible to users beyond faster/lighter pages.

## Risks Or Tradeoffs

- Image conversion introduces a new backend dependency and processing step.
