# INTENT

## User Request

Add Cloudflare R2 support so uploaded images can be stored persistently outside Railway/local disk.

## Requested Outcome

After R2 credentials are configured, backend uploads should store image files in R2, save metadata in Postgres, and return public image URLs that load directly from R2.

## Constraints

- Do not commit secrets.
- Keep local disk storage as the default local development path.
- Use the existing backend storage abstraction where possible.

## Open Questions

- None for this implementation pass.
