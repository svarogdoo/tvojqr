# Spec

Scope:
- Prefer R2 storage automatically when valid R2 settings are present.
- Keep local storage as the development fallback.
- Avoid changing public page rendering unless needed for the URL shape.

Affected areas:
- `backend/src/HostingQr.Infrastructure/Configuration/StorageOptions.cs`
- `backend/src/HostingQr.Infrastructure/DependencyInjection.cs` if needed
- README task tracking

Out of scope:
- Migrating already-lost local files.
- Presigned direct browser uploads.
