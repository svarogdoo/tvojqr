# SPEC

## Scope

Replace consent-gated GA4 with manually embedded Cloudflare Web Analytics, remove obsolete consent UI/configuration, and update privacy and setup documentation.

## Affected Areas

- Root SvelteKit layout
- Analytics component and public environment configuration
- Footer consent settings
- Privacy policy
- README analytics tasks and production setup

## Technical Approach

- Render Cloudflare's official module beacon when `PUBLIC_CLOUDFLARE_WEB_ANALYTICS_TOKEN` contains a valid 32-character site token and the current path is an approved marketing/legal route.
- Disable Cloudflare SPA measurement so dashboard, admin, account, and customer route transitions are not collected.
- Use a full document navigation when crossing from a measured public route to an unmeasured internal route, ensuring the beacon unloads before the private URL becomes current.
- On mount, remove the old GA consent local-storage key and `_ga` cookies as a migration cleanup.
- Remove all GA4 script loading and data-layer types.

## Assumptions

- The user will create a Cloudflare Web Analytics site for `hostingqr.com` and configure its public token in Netlify.
- Public landing-page and referral analytics are sufficient for the initial acquisition use case.

## Risks

- Cloudflare's beacon can be blocked by privacy tools, so counts will not represent every visitor.
- Client-side transitions between public pages are not counted; the privacy-first setup focuses on public landing visits and referral sources.
- Legal requirements vary by jurisdiction; the implementation and privacy text follow Cloudflare's published cookie-free behavior but are not legal advice.
