# GqmPlus

> Microservices-based system built with a modular, independently deployable architecture. Each service owns its database, communicates via secured HTTP, and is orchestrated using Docker Compose.

---

## Table of Contents

- [Architecture](#architecture)
- [Repository Structure](#repository-structure)
- [Microservice Internal Structure](#microservice-internal-structure)
- [Code Conventions](#code-conventions)
- [Database Configuration](#database-configuration)
- [HMAC Service-to-Service Security](#hmac-service-to-service-security)
- [API Gateway](#api-gateway)
- [Running the Project](#running-the-project)
- [Version Control](#version-control)
- [CI/CD Pipeline](#cicd-pipeline)
- [Development Rules](#development-rules)
- [Production Considerations](#production-considerations)

---

## Architecture

The system follows a microservices architecture with:

- Independent backend services (.NET 10)
- Separate PostgreSQL 17 database per service
- API Gateway via Nginx reverse proxy
- Container orchestration via Docker Compose

**Total: 13 containers** — 6 services + 6 databases + 1 gateway

---

## Repository Structure

```
root/
│
├── .github/
│
├── services/
│   ├── access-service/
│   │   ├── AccessService.API/
│   │   └── AccessService.Tests/
│   ├── assessment-service/
│   ├── department-service/
│   ├── goal-service/
│   ├── premise-service/
│   └── gqm-goal-service/
│
├── shared/
│   └── Shared.HMAC/
│
├── .gitignore
├── GqmPlus.sln
├── docker-compose.yaml
└── nginx.conf
```

| Path | Description |
|------|-------------|
| `services/` | All microservices |
| `shared/Shared.HMAC/` | Shared HMAC authentication library |
| `docker-compose.yaml` | Container orchestration |
| `nginx.conf` | API Gateway configuration |
| `GqmPlus.sln` | .NET solution file |

---

## Microservice Internal Structure

Each service follows a layered clean architecture:

```
{ServiceName}/
│
├── {ServiceName}.API/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Extensions/
│   └── Program.cs
│
├── {ServiceName}.Application/
│   ├── Services/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Validators/
│   └── Mappings/
│
├── {ServiceName}.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   └── Exceptions/
│
├── {ServiceName}.Infrastructure/
│   ├── Persistence/
│   ├── Configurations/
│   └── Clients/
│
└── Dockerfile
```

### Layer Responsibilities

| Layer | Responsibility |
|-------|---------------|
| **API** | HTTP endpoints, middleware, exception handling — no business logic |
| **Application** | Business logic, service interfaces, DTOs, validation |
| **Domain** | Core entities, value objects, business rules |
| **Infrastructure** | Database config, external service calls, persistence setup |

---

## Code Conventions

### Naming

| Element | Convention |
|---------|-----------|
| Classes | `PascalCase` |
| Interfaces | `IPrefix` |
| Methods | `PascalCase` |
| Variables | `camelCase` |
| Controllers | `{Entity}Controller` |
| DTOs | `{Entity}Request`, `{Entity}Response` |
| Value Objects | Immutable, no public setters |

### Clean Code Rules

- No business logic inside controllers
- One responsibility per class
- Keep methods small and readable
- Avoid magic strings
- Use explicit domain exceptions
- Validation handled in Application layer
- Use dependency injection
- Use consistent formatting across all services

---

## Database Configuration

Each service has its own dedicated PostgreSQL 17 database. No shared databases, no cross-service joins — all communication strictly via HTTP.

### Connection String Format

```
Host={service}-db;Port=5432;Database={service}db;Username=postgres;Password=postgres
```

### Database Mapping

| DB Container | Database Name |
|-------------|--------------|
| `access-db` | `accessdb` |
| `department-db` | `departmentdb` |
| `goal-db` | `goaldb` |
| `premise-db` | `premisedb` |
| `gqm-goal-db` | `gqmgoaldb` |
| `assessment-db` | `assessmentdb` |

### Database Rules

- No shared databases
- No cross-service joins
- Communication strictly via HTTP
- Migrations must be version-controlled

### Seed Data (Development Only)

During early development, seed data mocks initial database content to allow independent service development, early endpoint testing, and integration before all services are fully implemented.

Seed logic must:
- Be isolated in the Infrastructure layer
- Run automatically on startup (if database is empty)
- Be removed or adapted for production

---

## HMAC Service-to-Service Security

Internal communication between services is secured using HMAC authentication via the shared library at `shared/Shared.HMAC/`.

### Outgoing Requests

Signature is calculated from `body + timestamp + secret`, then added as headers:
- `X-HMAC-Signature`
- `X-HMAC-Timestamp`

### Incoming Requests

Middleware validates the signature. Timestamp must be within 5 minutes — `401` is returned if invalid.

### Configuration

```yaml
# In docker-compose.yaml
HMAC_SECRET_KEY=your-secret-key
```

> ⚠️ **Must be changed in production.**

### Whitelisted Paths (HMAC skipped)

- `/health`
- `/swagger`
- `/weatherforecast`

---

## API Gateway

Nginx acts as a reverse proxy and single entry point to the system.

- Routes requests to the correct microservice
- Exposes a unified base URL
- Isolates internal service ports

> All external traffic must go through the API Gateway.

---

## Running the Project

### Prerequisites

- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- .NET 10 SDK *(optional — only needed for local debugging outside Docker)*

---

### Start the Full System

```bash
docker compose up --build
```

This will build all services, spin up all databases, start the API Gateway, and configure the internal Docker network.

### Start Without Rebuilding

```bash
docker compose up
```

### Stop the System

```bash
docker compose down
```

### Stop and Remove Volumes (reset databases)

```bash
docker compose down -v
```

---

### Rebuild and Restart a Single Service

If you've made changes to only one service, you don't need to rebuild everything:

```bash
# Rebuild and restart a specific service
docker compose up --build --no-deps -d <service-name>

# Example
docker compose up --build --no-deps -d access-service
```

> `--no-deps` prevents restarting dependent containers. `-d` runs in detached mode.

### View Logs for a Single Service

```bash
docker compose logs -f <service-name>

# Example
docker compose logs -f access-service
```

### Restart a Service Without Rebuilding

```bash
docker compose restart <service-name>
```

### Stop a Single Service

```bash
docker compose stop <service-name>
```

---

### Running a Single Service Locally (Outside Docker)

From inside a service folder:

```bash
dotnet run
```

Make sure:
- The corresponding database container is running (`docker compose up <service-name>-db -d`)
- The connection string matches the container name
- Required environment variables are set

---

### Required Local Configuration

Before running the system:

1. Ensure Docker is running
2. Ensure no port conflicts on your machine
3. Verify `docker-compose.yaml` contains:
   - `HMAC_SECRET_KEY`
   - Correct database container names
4. Ensure connection strings match DB container names
5. All services must use the same HMAC secret locally

---

## Version Control

### Branching Strategy

| Branch | Purpose |
|--------|---------|
| `main` | Stable, production-ready code |
| `dev` | Integration branch |
| `feature/{feature-name}` | New features |

### Workflow

```
feature/my-feature
       │
       ▼
      dev   ──── (when stable) ────▶   main
```

1. Create a feature branch from `dev`
2. Develop and commit your changes
3. Open a PR and merge `feature/...` → `dev`
4. Merge `dev` → `main` when stable and reviewed

---

## CI/CD Pipeline

### Continuous Integration

The project uses GitHub Actions for automated build and test validation. The CI pipeline is designed to be efficient — **only services with actual code changes are built and tested**.

**Location:** `.github/workflows/ci.yml`

### How It Works

1. **Change Detection** — On every push or PR, the pipeline detects which services have changed by analyzing modified file paths
2. **Selective Builds** — Only affected services are built (e.g., if you modify `access-service`, only that service builds)
3. **Automated Testing** — Each service's test project (`.Tests`) runs automatically
4. **Test Results** — Test results are published to the GitHub Actions UI for easy review
5. **Fail-Fast** — If tests fail, the pipeline stops immediately to prevent broken code from progressing

### Path-Based Triggers

The CI pipeline monitors these paths:

- `services/{service-name}/**` — Individual service changes
- `shared/**` — Shared library changes (triggers all services)
- `docker-compose.yaml` — Infrastructure changes
- `nginx.conf` — Gateway configuration changes

### Performance Optimizations

- **NuGet Package Caching** — Speeds up builds by ~2-3x after the first run
- **Parallel Builds** — All services build simultaneously
- **Smart Change Detection** — Skips unchanged services entirely

### Triggers

- Push to `main` or `dev` branches
- Pull requests to `main` or `dev`

### CI vs CD

This pipeline focuses on **Continuous Integration** (validation only):
- ✅ Build verification
- ✅ Test execution
- ✅ Code quality checks

**Continuous Deployment** (Docker image publishing, container deployment) is intentionally separated and will be added later as a dedicated CD pipeline.

---

## Development Rules

- Do not access another service's database directly
- All service-to-service communication must use HTTP clients
- Shared logic must be placed in `/shared`
- Each service must be independently deployable
- Keep services loosely coupled
- Follow defined folder and naming conventions strictly

---
