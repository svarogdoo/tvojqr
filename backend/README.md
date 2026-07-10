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
- `GET /api/billing/entitlement`
- `POST /api/billing/checkout`
- `POST /api/billing/polar/webhook`

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
- uploaded files use local disk only when local storage is explicitly selected or R2 is not configured
- production uploads should use Cloudflare R2 by setting the R2 settings below; `Storage__Provider=R2` can be set explicitly, but complete R2 settings are enough to select R2 automatically

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
- local development uses local disk storage via `appsettings.Development.json`

```bash
Storage__Provider=R2
Storage__R2__AccountId=your-cloudflare-account-id
Storage__R2__AccessKeyId=your-r2-access-key-id
Storage__R2__SecretAccessKey=your-r2-secret-access-key
Storage__R2__BucketName=your-r2-bucket-name
Storage__R2__PublicBaseUrl=https://your-public-r2-url-or-custom-domain
```

When R2 is enabled, the backend stores the R2 object key in Postgres and returns public image URLs built from `Storage__R2__PublicBaseUrl`. For example, the database stores `projects/.../image.webp`, while API responses return `https://assets.hostingqr.com/projects/.../image.webp`.

Pricing entitlement tiers:

- supported customer tiers are `free`, `standard`, `world_cup`, and `plus`
- internal `admin` tier is available for unrestricted owner/admin access and should not be shown publicly
- users without an active row in `user_entitlements` receive tier `none` and cannot use project tools
- until checkout/admin tooling exists, a tier can be assigned manually in SQL:

```sql
insert into user_entitlements (user_id, tier, is_active, granted_manually)
values ('user-id-here', 'free', true, true)
on conflict (user_id) do update set
    tier = excluded.tier,
    is_active = excluded.is_active,
    granted_manually = excluded.granted_manually,
    updated_at = now();
```

To remove access, set `is_active = false` for that user's entitlement row.

To grant unrestricted internal access, use tier `admin` instead of `free`.

Polar checkout config:

- keep the Polar access token and product IDs only in backend env/user-secrets/Railway variables, never in frontend env
- `Polar__AccessToken` should be an API key with `checkouts:write`; `checkouts:read` is optional
- `Polar__SuccessUrl` is where Polar redirects after successful checkout
- `Polar__CancelUrl` is used as Polar's checkout return URL/back destination
- checkout creates Polar sessions; the basic webhook updates `user_entitlements` after trusted Polar payment/subscription events
- `Polar__WebhookSecret` is required for the Polar Raw webhook endpoint and must match the endpoint secret in Polar

```bash
Polar__AccessToken=your-polar-api-key
Polar__WebhookSecret=your-polar-webhook-secret
Polar__SuccessUrl=https://hostingqr.com/dashboard
Polar__CancelUrl=https://hostingqr.com/pricing
Polar__Products__StandardMonthly=b0e6fd14-2373-4a77-99aa-ee9dc4b8bc92
Polar__Products__StandardAnnual=0f0ea548-09da-4eea-95ad-38423b1184ab
Polar__Products__WorldCupMonthly=8c2551a2-5048-41e5-a6b2-5823e3b20351
Polar__Products__WorldCupAnnual=a90aa5ae-e458-4df8-952c-3266607bc4d5
Polar__Products__PlusMonthly=817a9d13-84c2-4d8-9cf5-1a3caaf7e627
Polar__Products__PlusAnnual=387c57c1-abde-44f3-1b511d27c34b
```

For local development, use user-secrets instead of committing real values:

```bash
dotnet user-secrets --project src/HostingQr.Api set "Polar:AccessToken" "your-polar-api-key"
dotnet user-secrets --project src/HostingQr.Api set "Polar:WebhookSecret" "your-polar-webhook-secret"
```

Polar webhook setup:

- endpoint URL: `https://api.hostingqr.com/api/billing/polar/webhook`
- format: `Raw`
- selected events for the basic integration: `order.paid`, `subscription.active`, `subscription.created`, `subscription.updated`, `subscription.canceled`, and `subscription.revoked`
- webhook requests are verified using Standard Webhooks headers before entitlement changes are applied
- activation events update `user_entitlements` for `standard` and `plus` tiers from checkout metadata
- cancellation events only change access when Polar includes a clear period end or immediate revocation event

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
