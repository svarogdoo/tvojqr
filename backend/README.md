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
- SQL migration support is included through embedded `.sql` scripts
- migrations are disabled by default in `appsettings.json` until a local PostgreSQL database is ready

Run locally:

```bash
dotnet run --project src/HostingQr.Api
```

Open Swagger:

- `http://localhost:5030/swagger` or the port shown by the app
