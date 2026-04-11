# Backend

This folder contains the `.NET` product backend foundation.

Current stack:

- `ASP.NET Core` Web API
- `Dapper`
- `Npgsql`
- `Swagger`

Current structure:

- `src/HostingQr.Api`
- `src/HostingQr.Application`
- `src/HostingQr.Domain`
- `src/HostingQr.Infrastructure`
- `tests/HostingQr.Api.Tests`

Current endpoint:

- `GET /api/auth/google`
- `GET /api/auth/me`
- `POST /api/auth/sign-out`
- `GET /api/ping`
- `GET /api/projects`
- `POST /api/projects`
- `GET /api/projects/{projectId}`
- `GET /api/slugs/{slug}/availability`
- `POST /api/slugs/generate`
- `GET /api/public/{slug}`

Current notes:

- one user can own multiple projects
- each project currently has one active slug
- random slug generation is server-side and uniqueness-checked
- cookie auth is wired for backend sessions
- session cookies currently use a 14-day idle timeout with sliding expiration
- Google auth is scaffolded and needs real credentials in config/env
- SQL migration support is included through embedded `.sql` scripts
- migrations are disabled by default in `appsettings.json` until a local PostgreSQL database is ready

Required local auth/frontend config:

- backend `appsettings.json` or env/config override for:
  - `GoogleAuth:ClientId`
  - `GoogleAuth:ClientSecret`
  - `Auth:FrontendBaseUrl`
- frontend public env:
  - `PUBLIC_API_BASE_URL` (defaults to `http://localhost:5115` if unset)

Run locally:

```bash
dotnet run --project src/HostingQr.Api
```

Open Swagger:

- `http://localhost:5030/swagger` or the port shown by the app
