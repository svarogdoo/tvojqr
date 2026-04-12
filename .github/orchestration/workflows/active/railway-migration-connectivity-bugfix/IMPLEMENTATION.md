# IMPLEMENTATION

## Planned Changes

- Fix Railway DB resolver behavior.
- Add test coverage for internal-host connection resolution.

## Files Touched

- `backend/src/HostingQr.Infrastructure/Data/ConnectionStringResolver.cs`
- `backend/tests/HostingQr.Api.Tests/ConnectionStringResolverTests.cs`
- `backend/README.md`
- `backend/src/HostingQr.Api/appsettings.json`
- `backend/src/HostingQr.Api/appsettings.Development.json`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`

## Notes

- The failure was before any migration SQL ran, so the bug was connection policy rather than migration content.
- Railway internal DB hosts now default to `SSL Mode=Disable`, while external/public hosts still default to `Require`.
- Production no longer inherits the localhost development DB fallback from `appsettings.json`.
- This is a deployment/runtime bugfix.
