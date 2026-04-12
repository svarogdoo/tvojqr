# SPEC

## Scope

Add a Railway-friendly runtime binding fallback and improve deployment guidance while waiting for the exact runtime error.

## Affected Areas

- backend startup/runtime configuration
- deployment docs

## Technical Approach

- Explicitly honor `PORT` in ASP.NET startup when present.
- Keep Docker and deployment docs aligned with that behavior.

## Assumptions

- Railway may be expecting the app to bind to the provided `PORT` env.

## Risks

- The actual failure may still be DB connectivity or environment variable setup rather than port binding.
