# SPEC

## Scope

Add persistent project view counting on active public page loads.

## Affected Areas

- Backend migration.
- Application project contracts/service.
- Infrastructure repository/DI.
- Frontend API types.
- Backend tests and README task markers.

## Technical Approach

- Add `project_view_counts` keyed by `project_id`.
- Add `IProjectViewRepository` with increment and stats lookup methods.
- Increment when `GetPublicProjectAsync` resolves an active project.
- Add `viewCount` and `lastViewedAt` to private project list/detail API responses.
- Keep public project response free of analytics data.

## Assumptions

- A backend `GET /api/public/{slug}` represents one page view.
- Refreshes count again.

## Risks

- Bot/crawler traffic can inflate counts.
- This is total count only, not unique visitor count.
