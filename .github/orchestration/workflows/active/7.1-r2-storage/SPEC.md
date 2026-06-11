# SPEC

## Scope

Add backend R2 storage support behind `IAssetStorageService`.

## Affected Areas

- Backend infrastructure configuration
- Backend asset storage implementation
- Dependency injection
- Project documentation
- README TODO status

## Technical Approach

- Add storage provider and R2 settings to `StorageOptions`.
- Add AWS S3 SDK package as the S3-compatible client for Cloudflare R2.
- Implement `R2AssetStorageService` that compresses images to webp, uploads to R2, generates public URLs, and deletes objects.
- Register local or R2 storage based on `Storage:Provider`.
- Keep object keys in the existing `stored_file_name` column.

## Assumptions

- R2 bucket has public development URL or custom domain enabled.
- The configured R2 token can write/delete objects in the bucket.

## Risks

- Misconfigured credentials cause upload failures.
- Public URL without matching public access causes broken image loads.
