# SPEC

## Scope

Refine frontend env loading for API base URL and document the matching backend auth redirect config.

## Affected Areas

- frontend config/env usage
- frontend local env example
- backend deployment docs

## Technical Approach

- Use a standard public env variable pattern in the frontend.
- Add a local `.env.example` showing the expected local value.
- Keep localhost only as a fallback for dev convenience.
- Document that production sign-in redirect depends on backend `Auth__FrontendBaseUrl` and frontend `PUBLIC_API_BASE_URL` being set correctly.

## Assumptions

- Production frontend hosting already supports public env configuration.

## Risks

- If the backend production env still points to localhost, sign-in redirects will remain wrong even after frontend config cleanup.
