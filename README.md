# EventHub 🎫 — SaaS Event Management Platform

**EventHub** is a production-grade Software-as-a-Service event management platform built with Clean Architecture. It provides end-to-end event lifecycle management: creation, ticketing with QR code validation, staff coordination, and real-time analytics.

![.NET](https://img.shields.io/badge/.NET_8-512BD4?logo=dotnet&logoColor=fff)
![React](https://img.shields.io/badge/React_19-61DAFB?logo=react&logoColor=000)
![TypeScript](https://img.shields.io/badge/TypeScript_6-3178C6?logo=typescript&logoColor=fff)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL_16-4169E1?logo=postgresql&logoColor=fff)
![License](https://img.shields.io/badge/license-MIT-blue)

---

## 📦 Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Runtime** | .NET 8 (LTS) / ASP.NET Core 8 | Backend API — REST + CQRS |
| **Architecture** | Clean Architecture, CQRS, MediatR | Separation of concerns |
| **ORM** | Entity Framework Core 8 + Npgsql | PostgreSQL data access |
| **Auth** | JWT Bearer (access) + opaque Refresh Tokens (rotation) | Stateless auth with revocation |
| **Template** | React 19 + TypeScript 6 + Vite 8 | SPA with HMR |
| **Styling** | TailwindCSS 4 | Utility-first CSS |
| **State** | TanStack Query (React Query) | Server-state management |
| **Forms** | React Hook Form + Zod | Client validation |
| **Infrastructure** | Docker Compose | Postgres, Redis, Seq + orchestration |
| **CI/CD** | GitHub Actions (planned) | Build → Test → Deploy |

---

## 🧱 Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────┐
│               EventHub.API (Web)                 │  Controllers, Middleware, Swagger
├─────────────────────────────────────────────────┤
│          EventHub.Application (Use Cases)         │  Commands, Queries, Handlers, DTOs, Validators
├─────────────────────────────────────────────────┤
│         EventHub.Infrastructure (Persistence)     │  EF Core, Repositories, JWT, Email, QR, Cache
├─────────────────────────────────────────────────┤
│           EventHub.Domain (Enterprise Business)   │  Entities, Enums, Exceptions
├─────────────────────────────────────────────────┤
│            EventHub.Shared (Cross-Cutting)        │  ApiResponse, PagedResult, Helpers
└─────────────────────────────────────────────────┘
```

### Dependency Direction

```
API → Application → Domain
     ↘            ↗
       Infrastructure
            ↗
         Shared (no external deps)
```

### CQRS + MediatR Pipeline

```
HTTP Request
    ↓
Controller → MediatR.Send(TRequest)
    ↓
[ValidationBehavior] ← FluentValidation
    ↓
[PerformanceBehavior] (warns if > 500ms)
    ↓
[LoggingBehavior] (structured enter/exit)
    ↓
Handler (IRequestHandler<TRequest, TResponse>)
    ↓
Repository → DbContext (Npgsql PostgreSQL)
    ↓
ApiResponse<T> → JSON → Client
```

---

## 📂 Project Structure

```
EventHub/
├── src/
│   ├── EventHub.Domain/          # 27 entities, 9 enums
│   │   ├── Entities/             # User, Event, Ticket, Order, Venue…
│   │   └── Enums/                # EventStatus, TicketStatus, OrderStatus…
│   ├── EventHub.Shared/          # ApiResponse<T>, PagedResult, AppExceptions
│   ├── EventHub.Application/     # CQRS handlers + validators
│   │   ├── Auth/                 # Register, Login, RefreshToken, Logout, Profile
│   │   ├── Events/               # CRUD, Publish, Cancel, Favorite, Queries
│   │   ├── Tickets/              # Reservation, Confirm, Validate, MyTickets/Orders
│   │   ├── Dashboard/            # Organizer Metrics
│   │   ├── Categories/           # Active Categories Query
│   │   ├── Venues/               # List + GetById
│   │   ├── Common/               # Interfaces, Behaviors, Mappings
│   │   └── DependencyInjection.cs
│   ├── EventHub.Infrastructure/  # EF Core, JWT, Email, QR, Repos
│   │   ├── Persistence/          # DbContext, 30 Configurations, Migrations
│   │   ├── Repositories/         # Generic + 7 specialized repos
│   │   └── Services/             # JwtToken, CurrentUser, Email, QR, RefreshStore
│   └── EventHub.API/             # 7 Controllers, Middleware, Program.cs
│       ├── Controllers/          # Auth, Events, Tickets, Dashboard, Categories, Venues, Profile
│       └── Middleware/           # GlobalExceptionHandlingMiddleware
├── frontend/
│   ├── src/
│   │   ├── components/           # UI, Layout, Auth, Events, Tickets, Dashboard
│   │   ├── hooks/                # useAuth (AuthContext + React Query)
│   │   ├── pages/                # Home, Events, Detail, Login, Register, Dashboard, Tickets
│   │   ├── services/             # api (Axios), auth, events, tickets
│   │   └── types/                # auth, event, ticket, venue, common
│   ├── vite.config.ts            # Proxy /api → localhost:5277
│   └── package.json
├── deploy/
│   ├── Dockerfile.api            # .NET 8 multi-stage → runtime
│   ├── Dockerfile.frontend       # Node 22 → nginx
│   └── nginx.conf                # SPA fallback, /api reverse-proxy
├── docker-compose.yml            # postgres + redis + seq + api + frontend
└── .dockerignore
```

---

## 🔐 Security Design

| Concern | Mechanism |
|---------|-----------|
| **Authentication** | JWT Bearer (access token, 15–60 min expiry) + opaque refresh token (7 days) |
| **Token Rotation** | Every refresh invalidates the previous token; a new pair is issued |
| **Reuse Detection** | Using an already-rotated token triggers descendant-revocation (chain walk) |
| **Password Hashing** | BCrypt with work factor 12 |
| **Account Lockout** | 5 failed login attempts → 30-minute lockout |
| **Email Verification** | 32-byte random token, 24-hour expiry |
| **QR Code Integrity** | `base64(payload).base64(HMAC-SHA256(payload))` with server-side secret |
| **Refresh Token Storage** | In `RefreshTokens` table with FK → `Users`, unique index on token |
| **CORS** | Configurable via `appsettings.json` |
| **HTTPS** | `RequireHttpsMetadata` conditional by environment |

### JWT Claim Structure

```json
{
  "sub": "guid (user id)",
  "email": "user@email.com",
  "jti": "guid (token id)",
  "http://.../claims/role": ["Admin", "Organizer", "Staff", "Customer"],
  "nbf": 1715000000,
  "exp": 1715010000,
  "iss": "EventHub",
  "aud": "EventHub"
}
```

---

## 🧪 Data Model — 27 Tables

### Core Entities

| Entity | Table | Key Relationships |
|--------|-------|-------------------|
| `User` | `Users` | `→ UserRoles, RefreshTokens, Events, Tickets, Orders, EventFavorites` |
| `Role` | `Roles` | `← UserRoles` |
| `Event` | `Events` | `→ TicketTypes, Tickets, EventFavorites, EventImages, EventTags, StaffList, DiscountCodes` |
| `TicketType` | `TicketTypes` | `→ OrderItems, Reservations` |
| `Ticket` | `Tickets` | `→ Events, Orders, Users` |
| `Order` | `Orders` | `→ OrderItems, Tickets, DiscountCodes` |
| `OrderItem` | `OrderItems` | `→ Tickets, TicketTypes` |
| `TicketReservation` | `TicketReservations` | `→ TicketTypes, Users, Orders` |
| `Venue` | `Venues` | `→ Events` |
| `Category` | `Categories` | `→ Events` |

### Concurrency & Ordering

| Mechanism | Where |
|-----------|-------|
| Sequence `order_number_seq` | PostgreSQL sequence for `Orders.OrderNumber` |
| `HasQueryFilter(e => e.DeletedAt == null)` | Soft-delete for Events + Venues |
| `CHECK` constraints | Status fields (VARCHAR with enum-like validation) |
| Partial indexes | `WHERE "DeletedAt IS NULL"` for filtered queries |
| Global retry | `CommandTimeout(30)` with `EnableRetryOnFailure(3)` |

---

## ⚡ API Endpoints

### Auth (`/api/auth`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/register` | Anon | Create account, sends email verification |
| POST | `/login` | Anon | Login → access + refresh tokens |
| POST | `/refresh` | Anon | Rotate refresh token (old revoked, new issued) |
| POST | `/logout` | JWT | Revoke refresh token |
| POST | `/forgot-password` | Anon | Send password-reset email |
| POST | `/reset-password` | Anon | Reset password via token |
| POST | `/verify-email` | Anon | Verify email via token |
| POST | `/change-password` | JWT | Change password (requires current) |
| GET  | `/me` | JWT | Current user profile + roles |

### Events (`/api/events`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | Anon | Paged published events (filter by category, date, featured) |
| GET | `/{id}` | Anon | Event detail by ID |
| GET | `/slug/{slug}` | Anon | Event detail by slug (increments view count) |
| GET | `/mine` | JWT | Current user's events (requires Organizer) |
| POST | `/` | JWT+Org | Create event (Draft) |
| PUT | `/{id}` | JWT+Org | Update event |
| POST | `/{id}/publish` | JWT+Org | Publish (validates TicketTypes exist) |
| POST | `/{id}/cancel` | JWT+Org | Cancel (cascades to tickets) |
| DELETE | `/{id}` | JWT+Org | Soft-delete |
| POST | `/{id}/favorite` | JWT | Toggle favorite |

### Tickets (`/api/tickets`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/reservations` | JWT | Create 15-minute reservation |
| POST | `/orders/confirm` | JWT | Confirm → generates tickets with QR |
| POST | `/orders/{id}/cancel` | JWT | Cancel order |
| POST | `/validate` | JWT+Staff | Validate QR, set ticket to `Used` |
| GET  | `/mine` | JWT | Current user's tickets |
| GET  | `/orders` | JWT | Current user's orders |

### Dashboard (`/api/dashboard`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/organizer/metrics` | JWT+Org | Active/completed events, sold tickets, revenue, attendance |

### Categories (`/api/categories`) — Anon, read-only
### Venues (`/api/venues`) — Anon, read-only
### Profile (`/api/profile`) — JWT, ready-only (alias of `/auth/me`)

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later)
- [Node.js 22+](https://nodejs.org/)
- [PostgreSQL 16+](https://www.postgresql.org/download/) (or Docker)

### 1. Clone & restore

```bash
git clone https://github.com/your-org/eventhub.git
cd eventhub

# Backend
dotnet restore src/EventHub.API/EventHub.API.csproj

# Frontend
cd frontend && npm install && cd ..
```

### 2. Configure the database

```bash
# Option A — Docker
docker run -d --name eventhub-postgres \
  -e POSTGRES_DB=eventhub_db \
  -e POSTGRES_USER=eventhub \
  -e POSTGRES_PASSWORD=eventhub_dev_2024 \
  -p 5432:5432 \
  postgres:16-alpine

# Option B — Local PostgreSQL
createdb eventhub_db
createuser eventhub -P  # password: eventhub_dev_2024
psql -c "ALTER DATABASE eventhub_db OWNER TO eventhub;"
```

### 3. Configure secrets

Edit `src/EventHub.API/appsettings.Development.json` (or set env vars):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=eventhub_db;Username=eventhub;Password=eventhub_dev_2024"
  },
  "Jwt": {
    "Secret": "your-32-char-min-secret-here-must-be-long",
    "Issuer": "EventHub",
    "Audience": "EventHub",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "QRCode": {
    "Secret": "your-qr-signing-secret-32-chars-min"
  },
  "Email": {
    "Provider": "Console"
  }
}
```

### 4. Run — Backend + Frontend

```bash
# Terminal 1 — Backend (auto-bootstraps DB on first launch)
dotnet run --project src/EventHub.API/EventHub.API.csproj --urls http://0.0.0.0:5277

# Terminal 2 — Frontend
cd frontend && npm run dev
```

**Open:** [http://localhost:5173](http://localhost:5173)

### 5. Docker production stack

```bash
docker compose up --build -d
```

Services:
| Service | Port | URL |
|---------|------|-----|
| Frontend (nginx) | 5173 | http://localhost:5173 |
| API | 8080 | http://localhost:8080 |
| PostgreSQL | 5432 | `localhost:5432` |
| Seq (logs) | 5341 | http://localhost:5341 |

---

## 🧑‍💻 Development

### Dev Admin User

After the first startup, a default admin user is auto-seeded:

| Email | Password | Role |
|-------|----------|------|
| `admin@eventhub.local` | `DevAdmin!2026` | Admin |

### Smoke Test (curl)

```bash
# Register
curl -X POST http://localhost:5277/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"firstName":"Test","lastName":"User","email":"test@x.com","password":"DemoPass123!"}'

# Login (mark email verified first via psql)
curl -X POST http://localhost:5277/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"test@x.com","password":"DemoPass123!"}'
```

### Build Verification

```bash
# Backend
dotnet build EventHub.slnx

# Frontend (type-check + production bundle)
cd frontend && npm run build
```

Both must exit with **0 errors**.

---

## 📐 Design Rationale

### Clean Architecture with CQRS

- **Commands** mutate state (`IRequest<TResponse>`); **Queries** read state (`IRequest<IReadOnlyList<TResponse>>`).
- **MediatR** decouples controllers from handlers. Behaviors (validation, logging, performance) are pipeline cross-cuts — no AOP frameworks needed.
- **FluentValidation** runs BEFORE the handler via `ValidationBehavior` — malformed requests are rejected at the pipeline level.

### Why Strings for Status Fields (not EF Enums)?

Status columns (`Event.Status`, `Ticket.Status`, `Order.Status`) are persisted as `VARCHAR` with `CHECK` constraints instead of EF‑converted enums. This:
- Allows adding new statuses without a DB migration (backward compat).
- Makes SQL queries readable in logs (`WHERE "Status" = 'Published'`).
- Keeps the schema portable (no Postgres-specific enum types).

The domain enums (`EventStatus`, `TicketStatus`, `OrderStatus`) are still used in C# code via `DomainStringExtensions` helpers that convert `.Value()`/`.ToEventStatus()`.

### Refresh Token Rotation & Reuse Detection

1. Every `/refresh` revokes the old token and issues a new pair.
2. The old token's `ReplacedByToken` column stores the new token.
3. If someone attempts to use a revoked token, the system walks the `ReplacedByToken` chain and revokes all descendants — cutting off a hijacker's session.

### Why Raw SQL Initializer (not EF Migrations)?

The schema includes Postgres-specific constructs (`pgcrypto`, `order_number_seq`, partial unique indexes, CHECK constraints) that EF migrations handle poorly. `PostgresDatabaseInitializer` runs `InitialCreate.sql` idempotently — if the public schema already has tables, it's skipped.

---

## 🧪 Test Coverage (Planned)

| Project | Framework | Targets |
|---------|-----------|---------|
| `tests/EventHub.UnitTests` | xUnit + Moq + FluentAssertions | Handlers, Validators, Pipeline behaviors |
| `tests/EventHub.IntegrationTests` | xUnit + TestContainers | Controllers, Repositories, Auth flow |
| `tests/EventHub.FunctionalTests` | Playwright | Front-to-back E2E |

---

## 📈 CI/CD (GitHub Actions — Planned)

```yaml
jobs:
  backend:
    - dotnet restore → build → test
  frontend:
    - npm ci → lint → build
  docker:
    - docker compose build
  deploy:
    - docker login → push images → deploy via SSH / k8s
```

---

## 📊 Project Metrics

| Metric | Value |
|--------|-------|
| C# files | 175 |
| Lines of C# | ~5,700 |
| TS/TSX files | 29 |
| Lines of TS/TSX | ~1,700 |
| Database tables | 27 |
| API endpoints | ~30 |
| Docker services | 5 |
| Commits | 13 |

---

## 📄 License

MIT — see [LICENSE](LICENSE).
