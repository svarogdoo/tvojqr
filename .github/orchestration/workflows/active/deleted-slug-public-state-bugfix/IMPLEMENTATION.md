# IMPLEMENTATION

## Planned Changes

- Investigate delete/public lookup mismatch.
- Apply a minimal fix.

## Files Touched

- `backend/src/HostingQr.Infrastructure/Projects/ProjectRepository.cs`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`
- targeted manual delete/public check if needed

## Notes

- Project deletion now explicitly removes dependent asset and slug rows before removing the project row.
- This avoids relying only on historical local DB cascade behavior.
