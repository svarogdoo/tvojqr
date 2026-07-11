# INTENT

## User Request

Avoid recompressing uploaded images when they are already small/low quality.

## Requested Outcome

Uploads should skip WebP compression for small files and only store WebP when it meaningfully reduces size.

## Constraints

- Apply to both local and R2 storage.
- Preserve current image upload behavior otherwise.
- MVP thresholds: skip under 250 KB, require 15% savings.

## Open Questions

- None for this pass.
