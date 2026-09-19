# IMPLEMENTATION

## Planned Changes

- Added client billing, invoice, and invitation persistence.
- Added admin-only client/menu APIs and owner-or-admin project access.
- Added secure email invitation and transactional acceptance flow.
- Added private local/R2 invoice storage and authorized file delivery.
- Extended the owner workspace, project editor, invitation page, and client account page.
- Added focused backend tests and completed full verification.

## Files Touched

- Backend API, application, infrastructure, migration, configuration, and test files
- `frontend/src/routes/admin/overview/+page.svelte`
- `frontend/src/routes/account/+page.svelte`
- `frontend/src/routes/invitations/[token]/+page.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `frontend/src/lib/stores/auth.ts`
- `frontend/src/lib/types/admin.ts`
- `README.md` and `backend/README.md`

## Verification

- `dotnet test HostingQr.Backend.sln`: 61 passed
- `npm run check`: 0 errors and 0 warnings
- `npm run build`: passed
- `git diff --check`: passed

## Notes

Manual billing entitlements are authoritative over dormant Polar webhook updates. Invoice PDFs use private storage and authenticated downloads only.
