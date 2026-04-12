# SPEC

## Scope

Add the minimal deployment configuration needed for the backend to run correctly on Railway.

## Affected Areas

- backend deployment configuration
- optionally backend runtime startup binding if needed

## Technical Approach

- Inspect the current backend project layout and runtime assumptions.
- Add an explicit Railway deployment config under `backend/` so build/start behavior is unambiguous.
- Prefer `dotnet publish` + published output start over `dotnet run` for production.

## Assumptions

- Railway service root directory is `backend/`.

## Risks

- If the real failure is from missing env vars rather than start command/runtime config, the user may still need to adjust Railway variables after this fix.
