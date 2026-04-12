# IMPLEMENTATION

## Planned Changes

- Add forwarded-header support to backend startup.

## Files Touched

- `backend/src/HostingQr.Api/Program.cs`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`

## Notes

- Added forwarded-header support so OAuth callback generation respects the original HTTPS scheme behind Railway's proxy.
- Focused auth/proxy bugfix.
