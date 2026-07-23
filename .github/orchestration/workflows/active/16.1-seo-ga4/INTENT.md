# INTENT

## User Request

Add SEO that supports Google Search, AI Overviews, and AI Mode, and provide a free way to understand traffic sources.

## Requested Outcome

Public marketing pages are crawlable and understandable, private and customer pages are excluded from indexing, and consent-aware Google Analytics 4 tracking is available in production.

## Constraints

- Use Google Analytics 4 as the free analytics provider.
- Keep all `/[slug]` customer pages `noindex` initially.
- Do not load analytics before consent.
- Follow current Google Search guidance; do not add unsupported AI-specific markup.

## Open Questions

None. The user approved the implementation plan and resolved the analytics and customer-page indexing decisions.
