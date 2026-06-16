# Spec

Scope:
- Add database-backed user entitlement records.
- Add service/repository abstractions for current user entitlement lookup.
- Gate authenticated project endpoints behind active entitlement checks.
- Update dashboard UX to show pricing encouragement when the user has no active tier.

Out of scope:
- Payment provider checkout.
- Webhook handling.
- Admin UI for manually assigning/removing free tier.
- Tier feature limits beyond active/inactive access.
