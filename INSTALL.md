# HR System — Installation (Docker)

This installs the whole system (database + API + web app) on one machine with Docker.
Staff then open it in a browser over the network — nothing is installed on their devices.

## 1. Prerequisites

- A server or PC with **Docker** + **Docker Compose v2** (Docker Desktop on Windows/Mac,
  or Docker Engine on Linux).
- ~4 GB free RAM and ~5 GB disk.

## 2. Configure

From the project folder:

```bash
cp .env.template .env
```

Edit `.env` and set strong, unique values:

| Variable | What it is |
|----------|------------|
| `DB_PASSWORD` | SQL Server `sa` password (8+ chars, mixed case/digit/symbol). |
| `JWT_SECRET` | API token signing secret (32+ random chars). |
| `LICENSING_SECRET` | License signing secret — **must match** the secret you use with `licensegen` for this client's key. |
| `WEB_PORT` | Host port staff browse to (e.g. `8080` → `http://server:8080`). |
| `WEB_ORIGIN` | The browser-facing URL of the app (e.g. `https://hr.client.local`). |
| `ASPNETCORE_ENVIRONMENT` | Leave as `Production`. |

## 3. Start

```bash
docker compose up -d --build
```

First start takes a minute: SQL Server initialises, then the API applies database
migrations and seeds the admin account + a 30-day trial automatically. Check status:

```bash
docker compose ps        # all services should be "running"/"healthy"
docker compose logs -f api
```

## 4. First login & licensing

1. Browse to `http://<server>:<WEB_PORT>`.
2. Sign in with **admin / admin** and change the password immediately.
3. The app runs on the **30-day trial**. To activate a yearly license:
   - On your machine, mint a key with the **same** `LICENSING_SECRET`:
     `dotnet run --project Emp.LicenseGen -- --secret "<LICENSING_SECRET>" --months 12`
   - In the app: **License → Activate**, paste the key.

## 5. HTTPS (recommended)

The containers serve plain **HTTP on the LAN**. For internet-facing or secure installs,
put a TLS-terminating reverse proxy (nginx, Traefik, IIS, Caddy) in front of the `web`
service and forward `X-Forwarded-Proto`. The app already honors forwarded headers, so
auth cookies become Secure automatically once TLS is in front. Set `WEB_ORIGIN` to the
`https://...` address.

## 6. Backups

All persistent data lives in named Docker volumes:

- `mssql-data` — the database
- `api-uploads` — leave attachments
- `web-keys` — auth cookie encryption keys

Back up the database regularly, e.g.:

```bash
docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$DB_PASSWORD" -C \
  -Q "BACKUP DATABASE EmpDB TO DISK='/var/opt/mssql/EmpDB.bak' WITH INIT"
```

(the `.bak` lands inside the `mssql-data` volume).

## 7. Upgrade

```bash
git pull            # or drop in the new build
docker compose up -d --build
```

New database migrations apply automatically on API startup.

## 8. Stop / remove

```bash
docker compose down              # stop (keeps data)
docker compose down -v           # stop AND delete all data volumes — destructive!
```
