# IMPLEMENTATION

## Planned Changes

- Added crawl/index controls, canonical metadata, social previews, sitemap, and robots configuration.
- Added accurate JSON-LD and improved existing homepage FAQ answers and rendering.
- Added persisted, withdrawable GA4 consent and public-route client-navigation tracking.
- Updated privacy, environment, Search Console, and README task documentation.

## Files Touched

- `frontend/src/lib/components/Seo.svelte`
- `frontend/src/lib/components/AnalyticsConsent.svelte`
- Public, private, admin, dashboard, and customer route Svelte files
- `frontend/src/routes/sitemap.xml/+server.ts`
- `frontend/static/robots.txt`
- `frontend/src/lib/config.ts`, `frontend/.env.example`, and `frontend/src/app.d.ts`
- Homepage copy/components, privacy policy, `README.md`, and this workflow folder

## Verification

- `npm run check` passed with 0 errors and 0 warnings.
- `npm run build` passed.
- `git diff --check` passed.
- Production preview responses confirmed the sitemap, robots file, homepage structured data/canonical metadata, and SSR `noindex` directives.
- Repository-wide `npm run lint` remains unusable as a task gate because it checks generated `.svelte-kit` output and existing unformatted files; no task-specific formatting defect was identified by type/build checks.

## Notes

Implementation approved by the user on 23 July 2026. Analytics is restricted to the five sitemap routes and strips query parameters other than recognized UTM campaign parameters.
