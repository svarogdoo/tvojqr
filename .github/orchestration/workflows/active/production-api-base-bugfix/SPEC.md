# SPEC

## Scope

Fix frontend API base URL resolution so auth and API routes always build a valid URL.

## Affected Areas

- `frontend/src/lib/config.ts`

## Technical Approach

- Keep `PUBLIC_API_BASE_URL` as the primary source.
- Add a safe fallback that never returns `undefined`.
- Prefer deriving `api.<host>` in production when env is missing.

## Assumptions

- The production API domain follows the `api.<domain>` pattern.

## Risks

- If the final production API domain differs from that pattern, the env var remains the required source of truth.
