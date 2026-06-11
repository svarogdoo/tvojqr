# BRIEF

## User-Facing Goal

Uploaded images should survive backend restarts/deployments and be served directly from Cloudflare R2.

## Success Criteria

- R2 can be enabled with environment variables only.
- Existing local uploads still work without R2 credentials.
- Uploaded images are converted to webp and uploaded to R2.
- Public/project API responses return R2 public URLs when R2 storage is enabled.
- Asset deletion removes the R2 object.

## UX Notes

No frontend UX changes in this pass.

## Risks Or Tradeoffs

- R2 public URL/domain must be configured correctly in Cloudflare.
- Existing local uploads remain local and are not migrated to R2.
