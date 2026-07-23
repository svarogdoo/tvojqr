# INTENT

## User Request

Remove the GA4 consent popup and replace Google Analytics with a free analytics direction that does not require analytics-cookie consent.

## Requested Outcome

HostingQr uses Cloudflare Web Analytics for privacy-first aggregate traffic and referral reporting, with no GA4 code or analytics consent popup.

## Constraints

- Cloudflare Web Analytics must remain optional until its public site token is configured.
- Remove GA4 configuration and visitor-facing consent controls.
- Clean up previously stored GA consent state and GA cookies.
- Preserve the existing technical SEO implementation.

## Open Questions

The Cloudflare site token is pending from the user; implementation will support it through a Netlify environment variable.
