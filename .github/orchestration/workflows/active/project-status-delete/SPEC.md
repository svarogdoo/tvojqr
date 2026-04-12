# SPEC

## Scope

Add project status tracking plus disable/delete actions across backend and frontend.

## Affected Areas

- backend schema and project service/repository
- dashboard project list response
- project detail page actions

## Technical Approach

- Add a simple `status` field to projects.
- Use a small enum/string set for now: active, disabled.
- Return status in list/detail responses.
- Add an update path for status changes.
- Add a delete endpoint scoped to the current user.
- Add a clear confirmation step before delete in the frontend.

## Assumptions

- Delete means full removal of the project and linked records.
- Disable should keep the record but mark it inactive.

## Risks

- Delete should not be easy to trigger accidentally.
- Public page behavior for disabled projects may need a later rule if not covered in this slice.
