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

Run locally:

```bash
dotnet run --project src/HostingQr.Api
```

Open Swagger:

- `http://localhost:5030/swagger` or the port shown by the app
