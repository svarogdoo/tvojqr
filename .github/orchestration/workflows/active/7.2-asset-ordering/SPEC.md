# SPEC

## Scope

Add saved-asset reordering to the project editor and backend.

## Affected Areas

- Backend asset service/repository/endpoints
- Frontend project editor image list
- README task list

## Technical Approach

- Add `PUT /api/projects/{projectId}/assets/order`.
- Validate project ownership and that submitted asset IDs belong to the project.
- Update `sort_order` according to submitted ID order.
- Keep an editor-local saved-asset order array.
- Save the order as part of the existing save flow.

## Assumptions

- Existing `sort_order` controls public page ordering.

## Risks

- Native drag-and-drop behavior can vary by browser/device.
