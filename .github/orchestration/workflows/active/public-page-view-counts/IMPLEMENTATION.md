# IMPLEMENTATION

## Completed Changes

- Added `project_view_counts` migration.
- Added `ProjectViewStats`, `IProjectViewRepository`, and Dapper `ProjectViewRepository`.
- Registered view repository in infrastructure DI.
- Incremented counts when an active public project is loaded through `GetPublicProjectAsync`.
- Exposed `viewCount` and `lastViewedAt` in private project list/detail API responses.
- Updated frontend project types for the new response fields.
- Added tests for active and disabled public page counting behavior.
- Updated README/backend docs.

## Verification

- Passed: `dotnet test backend/HostingQr.Backend.sln`.
- Passed: `npm run build --prefix frontend`.
