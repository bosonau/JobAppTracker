# JobAppTracker

A full-stack app for tracking job applications: company, role, status, date applied, source URL and notes.

- **API:** ASP.NET Core REST API with PostgreSQL, JWT bearer authentication, Redis caching and Elasticsearch full-text search.
- **UI:** A Vue 3 frontend. It works today; styling is coming soon.

```
Vue (Vite, :5173) ──► ASP.NET Core API ──► PostgreSQL   (source of truth)
                              ├──────────► Redis        (list cache, 60s)
                              └──────────► Elasticsearch (full-text search)
```

## Tech stack

### API
- **.NET 10** / ASP.NET Core Web API (controllers)
- **Entity Framework Core** with **PostgreSQL 16** (Npgsql)
- **Redis 7** for caching application lists (`IDistributedCache`)
- **Elasticsearch 9** for full-text search across company, role and notes
- **JWT bearer authentication**
- **OpenAPI** document with **Swagger UI** (NSwag)
- **Docker Compose** for the API, PostgreSQL, Redis and Elasticsearch

### UI (`ui-vue/`)
- **Vue 3** + **TypeScript**, built with **Vite**
- **Pinia** for state (auth), **Vue Router** with auth guards
- **Vitest**, ESLint, Oxlint and Prettier

## Prerequisites

- [Docker](https://www.docker.com/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (only for Option B)
- [Node.js](https://nodejs.org/) `^22.18.0` or `>=24.12.0` (for the frontend)
- Free ports: `5432` (Postgres), `6379` (Redis), `9200` (Elasticsearch), `8080` (API in Docker), `5173` (Vue dev server)
- About 1 GB of free memory for Elasticsearch (it's capped at a 512 MB heap)

## Getting started

### 1. Create a JWT signing key

The API won't start without a JWT signing key of at least 32 bytes.

```powershell
# PowerShell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

```bash
# bash
openssl rand -base64 32
```

### 2. Create `api/.env`

```bash
cd api
cp .env.example .env
```

Fill in the values. `JWT_KEY`, `POSTGRES_PASSWORD` and `DEMO_PASSWORD` are required, and Docker Compose fails if any is missing.

```env
JWT_KEY=<your-generated-key>
POSTGRES_USER=jobtracker
POSTGRES_PASSWORD=<choose-a-password>
POSTGRES_DB=jobtracker
DEMO_USERNAME=demo
DEMO_PASSWORD=<choose-a-password>
```

`.env` is git-ignored, so your secrets stay out of the repo.

### 3a. Option A: run everything in Docker

From `api/`:

```bash
docker compose up -d --build
```

The API waits until Postgres, Redis and Elasticsearch report healthy. Elasticsearch can take 30 seconds or more on first start.

- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

### 3b. Option B: run the API locally with `dotnet run`

Start only the backing services in Docker (from `api/`):

```bash
docker compose up -d db redis elasticsearch
```

Then configure user-secrets and run the API:

```bash
dotnet user-secrets set "Jwt:Key" "<your-generated-key>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=jobtracker;Username=jobtracker;Password=<POSTGRES_PASSWORD>"
dotnet user-secrets set "Redis:Connection" "localhost:6379"
dotnet user-secrets set "DemoUser:Username" "demo"
dotnet user-secrets set "DemoUser:Password" "<DEMO_PASSWORD>"
dotnet run
```

Elasticsearch defaults to `http://localhost:9200`. To use another address, set `Elasticsearch:Url`.

- API: http://localhost:5065
- Swagger UI: http://localhost:5065/swagger

### 4. Run the frontend

```bash
cd ui-vue
npm install
```

Create `ui-vue/.env.local` that points to the API:

```env
# Option A (Docker)
VITE_API_BASE_URL=http://localhost:8080
# Option B (dotnet run)
# VITE_API_BASE_URL=http://localhost:5065
```

```bash
npm run dev
```

Open http://localhost:5173 and log in with your `DEMO_USERNAME` / `DEMO_PASSWORD`.

> CORS only allows `http://localhost:5173`. If you run the UI on another port, update the `Frontend` policy in `api/Program.cs`.

Other scripts: `npm run build`, `npm run test:unit`, `npm run lint`, `npm run format`.

## Database

In the Dev environment, EF Core migrations run automatically on startup, so you don't need `dotnet ef database update`. Docker Compose sets `ASPNETCORE_ENVIRONMENT=Development`.

Data lives in the `pgdata` Docker volume and the search index in `esdata`. Both survive `docker compose down`. To start over with empty data, run `docker compose down -v`.

## Caching and search

- **Redis cache:** `GET /api/applications` results are cached for 60 seconds, with a separate entry for each user and filter combination. Every create, update or delete clears the cache. Responses include an `X-Cache: HIT` or `X-Cache: MISS` header. If Redis is unavailable, the API logs a warning and reads from Postgres.
- **Elasticsearch:** Each application is indexed when it's created or updated and removed from the index when it's deleted. `GET /api/applications/search?q=...` searches company, role and notes. Indexing failures are logged but don't block the write. If Elasticsearch is down, search returns `503`.
- **Reindex:** `POST /api/applications/reindex` rebuilds the search index from Postgres. It works only in Development.

## Authentication

All `/api/applications` endpoints require a JWT. To get one, log in as the demo user with the `DEMO_USERNAME` and `DEMO_PASSWORD` from your config:

```http
POST /api/auth/login
Content-Type: application/json

{ "username": "demo", "password": "<DEMO_PASSWORD>" }
```

The response contains a `token`, valid for 30 minutes. Send it with each request:

```
Authorization: Bearer <token>
```

**In Swagger UI:** call `POST /api/auth/login`, copy the token, click **Authorize** and paste it in.

## API

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Get a JWT (anonymous) |
| `GET` | `/api/applications` | List all applications, sorted by company (Redis-cached) |
| `GET` | `/api/applications?companyName=acme` | Filter by company name (case-insensitive, partial match) |
| `GET` | `/api/applications?status=Interview` | Filter by status |
| `GET` | `/api/applications/search?q=backend` | Full-text search on company, role and notes (Elasticsearch) |
| `GET` | `/api/applications/{id}` | Get one application |
| `POST` | `/api/applications` | Create an application |
| `PUT` | `/api/applications/{id}` | Replace an application |
| `DELETE` | `/api/applications/{id}` | Delete an application |
| `POST` | `/api/applications/reindex` | Rebuild the search index (Development only) |

An application looks like this:

```json
{
  "id": 1,
  "companyName": "Acme Corp",
  "roleTitle": "Backend Developer",
  "status": "Applied",
  "dateApplied": "2026-10-01",
  "sourceUrl": "https://example.com/jobs/123",
  "notes": "Referred by a friend"
}
```

`status` is one of `Applied`, `Screening`, `Interview`, `Offer`, `Rejected` or `Withdrawn`. Requests also accept the numbers `0` to `5`.

Validation:
- `companyName` and `roleTitle` are required, max 250 characters.
- `dateApplied` is required.
- `sourceUrl` must be a valid URL, max 2048 characters.
- `notes` has a max of 4000 characters.

### Example requests

[`api/JobTrackerApi.http`](api/JobTrackerApi.http) has ready-to-run requests for every endpoint. It works with the VS Code [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension or in Visual Studio. It reads the demo credentials from `api/.env`.

Run the login request first; the others reuse its token automatically. The file points to `http://localhost:5065` by default. If you're on Docker, change `@JobTrackerApi_HostAddress` to `http://localhost:8080`.

## Project structure

```
api/
├── Controllers/
│   ├── ApplicationsController.cs   # CRUD, filters, caching, search, reindex
│   └── AuthController.cs           # Demo login, issues JWTs
├── Dtos/
│   ├── ApplicationDtos.cs          # Request/response records + mappings
│   └── AuthDtos.cs
├── Models/
│   ├── Application.cs              # Entity + ApplicationStatus enum
│   ├── ApplicationContext.cs       # EF Core DbContext
│   └── ApplicationDoc.cs           # Elasticsearch document
├── Services/
│   └── ApplicationSearchService.cs # Elasticsearch indexing and search
├── Migrations/                     # EF Core migrations
├── Program.cs                      # Services, auth, CORS, OpenAPI, pipeline
├── Dockerfile
├── docker-compose.yml              # API, Postgres, Redis, Elasticsearch
├── .env.example
└── JobTrackerApi.http              # Example requests

ui-vue/
├── src/
│   ├── api/                        # fetch wrapper (http.ts) + applications client
│   ├── components/                 # ApplicationForm
│   ├── views/                      # LoginView, ApplicationsView
│   ├── stores/                     # Pinia auth store
│   └── router/                     # Routes + auth guards
├── vite.config.ts
└── package.json
```

## Known limitations

This is a learning and portfolio project, not production software:

- **Demo login only.** There's one user, configured in settings. There's no user store or password hashing.
- **No per-user data in Postgres.** Applications aren't tied to a user, so any authenticated caller can see and edit all of them. Search results are filtered by user, but the database isn't.
- **Development-only conveniences.** Swagger UI, automatic migrations and `/reindex` only run in Development.
- **Elasticsearch security is turned off** in Docker Compose for local development.
- **Minimal tests.** The API has no automated tests yet, and the UI has only a starter Vitest test.

## TODOs

- UI styling
- Real user accounts with hashed passwords, and applications tied to each user
- API integration tests
- Search in the UI

## License

[MIT](LICENSE)
