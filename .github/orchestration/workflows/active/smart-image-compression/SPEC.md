# SPEC

## Scope

Add smart image compression decision logic to asset storage services.

## Affected Areas

- Local asset storage.
- R2 asset storage.
- Shared infrastructure helper.
- Backend docs/workflow.

## Technical Approach

- Buffer uploaded image once in memory.
- If original size is below 500 KB, store original bytes with a safe extension and original content type.
- Otherwise encode a WebP candidate in memory.
- Store WebP only if candidate size is at least 15% smaller than the original.
- Store original bytes when WebP is not meaningfully smaller.

## Assumptions

- Current upload validation already restricts accepted content types to images.
- Buffering uploads in memory is acceptable for current MVP file sizes.

## Risks

- Original GIFs are preserved rather than converted; this may retain larger animated files but avoids quality loss.
