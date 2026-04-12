# SPEC

## Scope

Implement project settings editing for name and slug, including slug availability checks, random slug generation, and save behavior.

## Affected Areas

- `backend/src/HostingQr.Application`
- `backend/src/HostingQr.Infrastructure`
- `backend/src/HostingQr.Api`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- frontend auth/api helper usage

## Technical Approach

- Add backend update support for a project owned by the current user.
- Extend slug persistence to support replacing the current primary slug while preserving the one-active-slug rule.
- Add a `PUT`-style endpoint for project settings updates.
- Update the frontend project detail page into a real form that loads the current project, checks slug availability, generates random slugs, and saves changes.

## Assumptions

- One project still has one active slug for now.
- Slug changes should stay uniqueness-checked server-side.
- This slice does not include uploads, languages, or preview yet.

## Risks

- Slug replacement logic must not break uniqueness or leave multiple active slugs.
- Frontend state needs to avoid confusing availability messaging while the slug is unchanged.
