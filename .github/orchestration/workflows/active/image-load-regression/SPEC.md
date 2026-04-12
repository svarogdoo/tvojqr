# SPEC

## Scope

Diagnose and fix the cross-project image load regression introduced by recent upload/storage changes.

## Affected Areas

- backend asset storage path and naming
- backend asset metadata
- frontend asset rendering path

## Technical Approach

- Inspect how old and new asset files are named and stored.
- Check whether recent upload changes invalidate existing asset URLs or stored file references.
- Apply the smallest fix that preserves both existing and future uploads.

## Assumptions

- The regression is likely related to the recent WebP conversion change or local file storage behavior.

## Risks

- Existing stored asset metadata may need compatibility handling if the bug stems from already-saved rows.
