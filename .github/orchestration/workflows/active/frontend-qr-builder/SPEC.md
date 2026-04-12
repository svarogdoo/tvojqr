# SPEC

## Scope

Implement a frontend-only QR builder section on the project settings page.

## Affected Areas

- frontend project settings page
- frontend package dependencies

## Technical Approach

- Use a browser-side QR generation library with styling support.
- Build a section that derives the target URL from the current project slug.
- Add a small set of visual presets and a color picker.
- Support direct download from the generated QR preview.

## Assumptions

- Persisting QR preferences is not required yet.
- Downloading one generated variant is enough for this first slice.

## Risks

- The QR preview should remain accurate when the slug changes in the editor.
