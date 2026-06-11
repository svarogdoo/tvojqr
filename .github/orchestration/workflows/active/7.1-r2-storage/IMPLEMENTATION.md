# IMPLEMENTATION

## Planned Changes

- Add R2 config options.
- Add an R2 asset storage implementation.
- Register storage implementation by config.
- Document required environment variables.
- Update README task status for R2 storage support.

## Files Touched

- `backend/src/HostingQr.Infrastructure/Configuration/StorageOptions.cs`
- `backend/src/HostingQr.Infrastructure/Assets/R2AssetStorageService.cs`
- `backend/src/HostingQr.Infrastructure/DependencyInjection.cs`
- `backend/src/HostingQr.Infrastructure/HostingQr.Infrastructure.csproj`
- `backend/src/HostingQr.Api/Program.cs`
- `backend/README.md`
- `README.md`

## Verification

- `dotnet test backend/HostingQr.Backend.sln`

## Notes

- No credentials will be committed.
- Local storage remains the default when `Storage__Provider` is omitted.
- R2 uploads disable AWS SDK chunk encoding because Cloudflare R2 rejects signed streaming payload trailers.
