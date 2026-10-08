# Sample project: JobAppTracker

A REST API for tracking job applications: the company, role, status and date applied, plus notes. Built with ASP.NET Core and PostgreSQL, secured with JWT bearer authentication.

## Tech stack

- **.NET 10** / ASP.NET Core Web API (controllers)
- **Entity Framework Core** with **PostgreSQL 16** (Npgsql)
- **JWT bearer authentication**
- **OpenAPI** document with **Swagger UI** (NSwag)
- **Docker Compose** for the API and database

## Getting started

You can run everything in Docker, or run the API locally against a database in Docker. Either way, you need a **JWT signing key** of at least 32 bytes. The app refuses to start without one.

To generate a key:

```powershell
# PowerShell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

```bash
# bash
openssl rand -base64 32
```

### Option A: Docker (API + database)

Requires [Docker](https://www.docker.com/).

```bash
cd JobTrackerApi
echo "JWT_KEY=<your-generated-key>" > .env
docker compose up -d --build
```

- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

`.env` is git-ignored, so your key stays out of the repo.

### Option B: Run locally with `dotnet run`

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) and Docker (for the database).

```bash
cd JobTrackerApi
docker compose up -d db
dotnet user-secrets set "Jwt:Key" "<your-generated-key>"
dotnet run
```

- API: http://localhost:5065
- Swagger UI: http://localhost:5065/swagger

### Database

In the Development environment, EF Core migrations are applied automatically on startup, so you don't need to run `dotnet ef database update`. Data is stored in the `pgdata` Docker volume and survives `docker compose down`. To start over with an empty database, run `docker compose down -v`.

## Authentication

All `/api/applications` endpoints require a JWT. To get one, log in as the built-in demo user:

```http
POST /api/auth/login
Content-Type: application/json

{ "username": "demo", "password": "demo-password" }
```

The response contains a `token`, which is valid for 30 minutes. Send it with each request:

```
Authorization: Bearer <token>
```

**In Swagger UI:** call `POST /api/auth/login`, copy the token, click **Authorize**, and paste it in.

## API

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Get a JWT (anonymous) |
| `GET` | `/api/applications` | List all applications, sorted by company |
| `GET` | `/api/applications?companyName=acme` | Filter by company name (case-insensitive, partial match) |
| `GET` | `/api/applications/{id}` | Get one application |
| `POST` | `/api/applications` | Create an application |
| `PUT` | `/api/applications/{id}` | Replace an application (`id` in the body must match the URL) |
| `DELETE` | `/api/applications/{id}` | Delete an application |

An application looks like this:

```json
{
  "id": 1,
  "companyName": "Acme Corp",
  "roleTitle": "Backend Developer",
  "status": 0,
  "dateApplied": "2026-10-01",
  "sourceUrl": "https://example.com/jobs/123",
  "notes": "Referred by a friend"
}
```

`status` is a number: `0` Applied, `1` Screening, `2` Interview, `3` Offer, `4` Rejected, `5` Withdrawn.

[`JobTrackerApi/JobTrackerApi.http`](JobTrackerApi/JobTrackerApi.http) has ready-to-run requests for every endpoint. It works with the VS Code [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension or in Visual Studio. Run the login request first; the others reuse its token automatically.

## Project structure

```
JobTrackerApi/
├── Controllers/
│   ├── ApplicationsController.cs   # CRUD + company name filter
│   └── AuthController.cs           # Demo login, issues JWTs
├── Models/
│   ├── Application.cs              # Entity + ApplicationStatus enum
│   └── ApplicationContext.cs       # EF Core DbContext
├── Migrations/                     # EF Core migrations
├── Program.cs                      # Services, auth, OpenAPI, pipeline
├── Dockerfile
├── docker-compose.yml
└── JobTrackerApi.http              # Example requests
```

## Known limitations

This is a learning and portfolio project, not production software:

- **Demo login only.** There's a single hard-coded user (`demo` / `demo-password`), with no user store or password hashing.
- **No per-user data.** Applications aren't tied to a user, so any authenticated caller can see and edit all of them.
- **Development credentials.** The database password (`devpassword`) in `appsettings.json` and `docker-compose.yml` is for local use only.
- **No automated tests yet.**

## License

[MIT](LICENSE)
