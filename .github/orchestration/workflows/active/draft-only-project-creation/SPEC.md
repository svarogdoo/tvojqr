# SPEC

## Scope

Refactor the new-project flow to use frontend-only draft state until explicit save.

## Affected Areas

- dashboard new project action
- project editor route/component
- save/upload sequencing in the frontend

## Technical Approach

- Route `New project` to a draft editor state instead of creating a backend project immediately.
- Detect draft mode in the project page.
- Keep selected images in browser memory as `File` objects plus local preview URLs.
- On save, create the project first, then upload stored draft files.
- Keep existing edit flow for already-saved projects.

## Assumptions

- Draft mode can use a synthetic route segment like `new`.

## Risks

- Save flow becomes slightly more complex because creation and upload are chained.
