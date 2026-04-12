# SPEC

## Scope

Enable proxy-aware scheme handling in the backend so OAuth callback generation uses HTTPS in production.

## Affected Areas

- backend startup middleware

## Technical Approach

- Configure ASP.NET forwarded headers.
- Apply forwarded headers middleware before auth.
- Keep the fix minimal and production-safe.

## Assumptions

- Railway sends standard proxy headers such as `X-Forwarded-Proto`.

## Risks

- Middleware ordering matters for auth callback generation.
