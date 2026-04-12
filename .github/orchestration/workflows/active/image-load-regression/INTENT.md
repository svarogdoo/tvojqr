# INTENT

## User Request

Fix the issue where uploading a new image appears to break image loading for images that already existed on other projects.

## Requested Outcome

Previously uploaded images should continue loading normally after new uploads.

## Constraints

- Focus on the image-loading regression only.
- Verification is required.

## Open Questions

- Need to identify whether the regression is in file storage, file naming, metadata, or rendering.
