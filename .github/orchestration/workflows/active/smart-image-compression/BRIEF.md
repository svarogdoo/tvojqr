# BRIEF

## Goal

Reduce unnecessary quality loss from repeated image compression while still optimizing large uploads.

## Success Criteria

- Images under 500 KB are stored as original files.
- Larger images are compressed to WebP only when the candidate is at least 15% smaller.
- Otherwise, original file bytes and content type are stored.
- Backend tests pass.

## Non-Goals

- Image dimension resizing rules.
- Per-tier file storage limits.
- PDF support.
