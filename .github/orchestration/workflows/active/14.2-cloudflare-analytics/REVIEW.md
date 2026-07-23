# REVIEW

## Findings

No unresolved high- or medium-severity findings. Review initially found that a globally loaded beacon could expose private paths. The implementation now renders only on approved public paths, disables SPA measurement, and uses a full document navigation before entering unmeasured internal routes.

## Residual Risks

- Browser privacy tools and ad blockers can block the Cloudflare beacon, so counts will not include every visit.
- Public-to-public client navigation is intentionally not counted; acquisition reporting focuses on landing pages and referring sites.
- Production collection depends on a valid token configured for the exact `hostingqr.com` hostname.
- Cloudflare Web Analytics does not currently report UTM query parameters or custom events.

## Verification Notes

- `npm run check`: passed, 0 errors and 0 warnings.
- `npm run build`: passed.
- `git diff --check`: passed.
- Token-configured preview verified beacon inclusion on the homepage and exclusion from dashboard/customer SSR output.
- Follow-up reviewer found no high- or medium-severity issues.
