# IMPLEMENTATION

## Planned Changes

1. Update pricing cards, translated copy, annual discount, and free-trial prompt.
2. Add menu type to the database and project contracts.
3. Add menu-type selection before new-project editing.
4. Add structured Digital Menu persistence and APIs.
5. Build the Digital Menu editor and public renderer.
6. Add tests and run frontend/backend verification.

## Files Touched

- Pricing and localized marketing copy under `frontend/src/routes/pricing/` and `frontend/src/lib/homepageCopy.ts`.
- Project creation, dashboard, account labels, shared types, and the public slug page under `frontend/src/`.
- New focused `frontend/src/lib/components/DigitalMenuEditor.svelte`.
- Project contracts, menu services, endpoints, persistence, migration, entitlement limits, and dependency registration under `backend/src/`.
- Backend tests under `backend/tests/HostingQr.Api.Tests/`.
- Product direction and completed task markers in `README.md`.

## Verification

- `npm run check` in `frontend/`.
- `npm run build` in `frontend/`.
- `dotnet test` in `backend/`.
- `git diff --check`.
- Migration runner completed against local PostgreSQL and the new project columns/menu tables were verified directly.

## Notes

- User approved the full scope on 2026-09-09.
- Existing projects default to Image Menu.
- Internal billing identifiers remain `standard` and `plus` for provider compatibility while user-facing labels use Image Menu and Digital Menu.
- Digital Menu edits are transactionally saved; availability has a focused immediate endpoint.
- Pricing follow-up centered both product identities with icons, removed the popularity badge, added the live Image Menu example and Digital Menu placeholder, matched the final QR benefit, and softened the trial panel.
- Pricing follow-up equalized both cards with monochrome styling and added free redesign/setup plus translations messaging.
