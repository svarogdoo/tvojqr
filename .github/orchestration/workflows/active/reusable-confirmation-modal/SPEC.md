# SPEC

## Scope

Add a reusable confirmation modal component and replace the project delete confirmation flow with it.

## Affected Areas

- shared frontend components
- project settings page delete action

## Technical Approach

- Create a shared modal component with props for title, body text, confirm label, cancel label, and destructive styling.
- Keep state local to the project page for this first use.
- Use the component for project delete confirmation.

## Assumptions

- No global modal manager is needed yet.

## Risks

- If the component is made too generic now, it may become harder to read than necessary.
