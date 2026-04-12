# SPEC

## Scope

Fix the current project editor state handling and add a shared snackbar feedback component.

## Affected Areas

- frontend project editor route
- frontend dashboard create flow
- frontend shared components/stores/helpers
- backend only if image URLs or save responses need a minimal adjustment

## Technical Approach

- Change dashboard-created projects to start with an empty name while remaining valid in the backend flow.
- Ensure the project editor only overwrites local form state when initial data loads, not after a failed save.
- Fix image preview rendering for uploaded assets.
- Add a generic snackbar component with a shared store/helper.
- Use snackbar feedback for save/upload results.
- Redirect to dashboard after a successful save.

## Assumptions

- Save should remain explicit; no autosave is needed.
- Snackbar state can be handled client-side.

## Risks

- If save redirects too quickly, the success state may feel abrupt unless the snackbar persists briefly or the dashboard also confirms it.
