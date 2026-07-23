# SPEC

## Scope

Technical SEO for public marketing/legal routes, Google-compatible structured data and crawl controls, AI-friendly homepage copy, consent-aware GA4, Search Console setup documentation, and related README task tracking.

## Affected Areas

- Global SvelteKit layout and public page metadata
- Homepage FAQ copy
- Marketing, legal, account, dashboard, admin, and customer route indexing directives
- New `robots.txt` and `sitemap.xml` endpoints
- Public environment configuration and privacy disclosure
- Repository workflow and task documentation

## Technical Approach

- Use `PUBLIC_SITE_URL` as the canonical origin, with the request origin as a server endpoint fallback.
- Add canonical, Open Graph, Twitter, and robots metadata directly through Svelte heads.
- Add JSON-LD for `Organization`, `WebSite`, and `SoftwareApplication` on the homepage.
- List only approved marketing/legal URLs in the sitemap.
- Apply `noindex, nofollow` to private routes and `noindex, follow` to customer pages.
- Load GA4 only in the browser after a stored explicit consent choice.
- Track SvelteKit client navigation without duplicating the initial page view.

## Assumptions

- The production canonical origin is `https://hostingqr.com`.
- English remains the canonical marketing-page language because localized routes do not yet exist.
- Search Console domain verification is a manual DNS/account step.

## Risks

- Locale-switched content shares one URL, so full multilingual SEO and `hreflang` are out of scope.
- Marketing metadata rendered from an in-memory language store may change client-side; canonical metadata will use stable English positioning where practical.
