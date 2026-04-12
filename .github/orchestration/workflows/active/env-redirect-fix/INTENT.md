# INTENT

## User Request

The current sign-in flow still redirects to localhost. Update the app to use `.env`/public env correctly so the frontend picks up the right API path and production does not bounce to localhost.

## Requested Outcome

Frontend API config should cleanly use environment values with a local fallback, and the deployment docs should make the production auth redirect requirements explicit.

## Constraints

- Keep the fix focused on environment/config behavior.
- Verification is required.

## Open Questions

- Production backend `Auth__FrontendBaseUrl` still needs to be set correctly outside the repo.
