# SPEC

## Scope

Implement a real public project page driven by backend data, including active image rendering and unavailable-state handling.

## Affected Areas

- backend public project response and lookup behavior
- frontend public slug route

## Technical Approach

- Extend the backend public lookup endpoint to return project image assets for active projects.
- Return a meaningful unavailable state for disabled projects.
- Update the frontend slug route to fetch from the backend and render a simple image column.
- Replace demo data rendering with backend-backed UI.

## Assumptions

- Only active projects should render their public content.
- Disabled projects should not expose their images.
- Language support remains out of scope for now.

## Risks

- Existing demo slug route logic may need to be fully removed rather than partially adapted.
