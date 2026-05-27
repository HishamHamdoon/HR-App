# HR Management System

A two-tier HR management application built with ASP.NET Core 8.

- **Emp.Api** — REST API (controllers, EF Core, JWT auth, ASP.NET Identity).
- **EMP.Web** — MVC frontend (Vuexy admin theme) that consumes the API.
- **Emp.Models** — shared domain models and DTOs.
- **EMP.Services** — (placeholder class library).

## Features

- Employees, Departments, Job Titles, Countries, Sections, Leave Types, Leaves, Salaries, Payroll.
- Role-based access (Admin / Employee) with JWT + cookie auth.
- Leave-balance tracking (entitlement vs. taken per leave type).
- Dashboard with live counts and charts.
- PDF / Excel report exports.
- English / Arabic (RTL) localization.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, full, or the Docker image below)
- A trusted HTTPS dev cert: `dotnet dev-certs https --trust`

## Configuration

The API requires a JWT secret (≥ 32 chars). **It is not committed.** Provide it via one of:

```bash
# Option A: user-secrets (recommended for local dev)
cd Emp.Api
dotnet user-secrets set "JwtOptions:Secret" "your-local-dev-secret-at-least-32-chars"

# Option B: environment variable
setx JwtOptions__Secret "your-local-dev-secret-at-least-32-chars"
```

`appsettings.Development.json` ships a dev-only fallback secret so F5 works out of the box.

Connection string lives in `Emp.Api/appsettings.json` (`ConnectionStrings:Default`).

## Run locally (Visual Studio)

1. Set **Multiple startup projects**: `Emp.Api` and `EMP.Web` both **Start**.
2. Select the **https** profile for `Emp.Api` (binds `https://localhost:7031`).
3. Press **F5**. The DB is created and seeded automatically on first run.

Default seeded admin: **`admin` / `admin`**. New employees default to role **Employee** and password **`P@ssw0rd`**.

## Run locally (CLI)

```bash
# Terminal 1
dotnet run --project Emp.Api --launch-profile https   # https://localhost:7031

# Terminal 2 (after the API is listening)
dotnet run --project EMP.Web --launch-profile https    # https://localhost:7026
```

## Run with Docker

```bash
docker compose up --build
```

This starts SQL Server, the API (`http://localhost:7031`), and the Web app (`http://localhost:7026`).

The Web app resolves the API host from a single setting, `ApiUrls:BaseUrl` (`appsettings.json`),
overridable via the `ApiUrls__BaseUrl` environment variable. In compose it's set to
`http://api:8080` so the Web container reaches the API container; run locally it defaults to
`https://localhost:7031`.

## Health check

The API exposes `GET /health` (returns 200 when the database is reachable).

## Tests

```bash
dotnet test
```

## Security notes

- JWT secret must be supplied out-of-band (never commit it).
- Strong password policy + account lockout are enforced for end users.
- API endpoints are role-guarded; mutating endpoints require the `Admin` role.
- CORS is restricted to the origins in `Cors:AllowedOrigins`.
