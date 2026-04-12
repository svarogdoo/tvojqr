# IMPLEMENTATION

## Planned Changes

- Add image conversion dependency.
- Convert uploaded images to WebP during storage.
- Update metadata to match converted files.

## Files Touched

- `backend/src/HostingQr.Infrastructure/Assets/LocalAssetStorageService.cs`
- `backend/src/HostingQr.Infrastructure/HostingQr.Infrastructure.csproj`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`

## Notes

- Uploaded images are now decoded server-side and re-saved as lossy WebP.
- Stored metadata now reflects the converted file name and content type.
- Backend-only upload optimization slice.
