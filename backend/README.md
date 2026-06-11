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
- `POST /api/projects/{projectId}/languages`
- `DELETE /api/projects/{projectId}/languages/{languageCode}`
- `GET /api/slugs/{slug}/availability`
- `POST /api/slugs/generate`
- `GET /api/public/{slug}`

Current notes:

- one user can own multiple projects
- each project currently has one active slug
- each project has one default language variant and can have additional language variants
- random slug generation is server-side and uniqueness-checked
- cookie auth is wired for backend sessions
- session cookies currently use a 14-day idle timeout with sliding expiration
- Google auth is scaffolded and needs real credentials in config/env
- SQL migration support is included through embedded `.sql` scripts
- migrations run automatically in the Development environment once local PostgreSQL is available
- local Docker PostgreSQL is expected on `localhost:5433`
- uploaded files are currently stored on local disk unless a persistent `Storage__UploadsRootPath` is configured
- production uploads can be stored in Cloudflare R2 by setting `Storage__Provider=R2` and the R2 settings below

Required local auth/frontend config:

- backend user-secrets or env/config override for:
  - `GoogleAuth:ClientId`
  - `GoogleAuth:ClientSecret`
  - `Auth:FrontendBaseUrl`
- frontend public env:
  - `PUBLIC_API_BASE_URL` (defaults to `http://localhost:5115` if unset)

Production auth/frontend alignment:

- backend `Auth__FrontendBaseUrl` must be your real frontend origin, for example `https://hostingqr.com`
- frontend `PUBLIC_API_BASE_URL` must be your real backend API origin, for example `https://api.hostingqr.com`
- if backend `Auth__FrontendBaseUrl` still points to localhost, Google sign-in will redirect back to localhost even if the frontend API URL is correct

Cloudflare R2 storage config:

- keep these values only in backend env/user-secrets/Railway variables, never in frontend env
- local development defaults to local disk storage when `Storage__Provider` is omitted

```bash
Storage__Provider=R2
Storage__R2__AccountId=your-cloudflare-account-id
Storage__R2__AccessKeyId=your-r2-access-key-id
Storage__R2__SecretAccessKey=your-r2-secret-access-key
Storage__R2__BucketName=your-r2-bucket-name
Storage__R2__PublicBaseUrl=https://your-public-r2-url-or-custom-domain
```

When R2 is enabled, the backend stores the R2 object key in Postgres and returns public image URLs built from `Storage__R2__PublicBaseUrl`.

Recommended local setup:

```bash
dotnet user-secrets --project src/HostingQr.Api set "GoogleAuth:ClientId" "your-google-client-id"
dotnet user-secrets --project src/HostingQr.Api set "GoogleAuth:ClientSecret" "your-google-client-secret"
dotnet user-secrets --project src/HostingQr.Api set "Auth:FrontendBaseUrl" "http://localhost:5173"
```

Google OAuth redirect URI for local development:

- `http://localhost:5115/api/auth/google/callback`

If Google credentials are still placeholders, `GET /api/auth/google` will now return `503` instead of attempting a broken sign-in.

Run locally:

```bash
npm run dev
```

If you want to run the backend by itself:

```bash
dotnet run --project src/HostingQr.Api
```

Railway deployment recommendation:

- set the service root directory to `backend`
- use the included `backend/Dockerfile` as the Railway builder
- the Docker container listens on port `8080`
- the app also honors Railway's `PORT` environment variable explicitly at runtime
- `Database__ConnectionString` may be either a standard Npgsql connection string or a Railway-style `postgres://` / `postgresql://` URL
- if `Database__ConnectionString` is not set, the app can also build a connection string from Railway `PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`, and `PGPASSWORD` variables
- Railway internal DB hosts such as `*.railway.internal` now default to `SSL Mode=Disable`; external/public hosts still default to `SSL Mode=Require`
- for persistent uploaded files on Railway, mount a volume and set `Storage__UploadsRootPath` to that mounted directory (for example `/data/uploads`)

Config note:

- localhost Postgres defaults now live only in `appsettings.Development.json`
- production no longer falls back to `127.0.0.1:5433`

Open Swagger:

- `http://localhost:5030/swagger` or the port shown by the app
