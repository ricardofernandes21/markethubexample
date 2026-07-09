# MarketplaceHub

A portfolio project demonstrating a backend marketplace integration platform built with .NET 10.
It syncs product data to marketplace channels (Amazon) using event-driven messaging, exposes a REST API,
and ships with full observability, CI, and a React management dashboard.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Browser                                                    │
│  React Dashboard :5173                                      │
└──────────────────────────┬──────────────────────────────────┘
                           │ /api/* (nginx proxy)
                           ▼
┌──────────────────────────────────────────────────────────────┐
│  ProductService.Api  :5102                                   │
│  ASP.NET Core 10 · Minimal APIs · EF Core · MassTransit      │                                   │
└────────┬─────────────────────────────┬───────────────────────┘
         │ PostgreSQL                  │ RabbitMQ publish
         ▼                             ▼
┌─────────────────┐       ┌────────────────────────────────────┐
│  postgres :5432 │       │  AmazonConnector (Worker Service)  │
└─────────────────┘       │  MassTransit consumer              │
                          │  Polly HTTP retry (4×)             │
                          │  XML feed builder                  │
                          └────────────────┬───────────────────┘
                                           │ PUT /products
                                           │ POST /feeds/inventory
                                           ▼
                          ┌────────────────────────────────────┐
                          │  MockAmazonApi  :5200              │
                          │  Simulates 200 / 429 / 500 / 503   │
                          └────────────────────────────────────┘

```

---

## Services

| Service | Host port | Purpose |
|---------|-----------|---------|
| `product-service` | 5102 | REST API - product CRUD |
| `products-dashboard` | 5173 | React management UI |
| `amazon-connector` | | Worker — syncs to Amazon |
| `mock-amazon-api` | 5200 | Fake Amazon API with random failures |
| `postgres` | 5432 | Product data store |
| `rabbitmq` | 5672 / 15672 | Message broker (management UI on 15672) |

---

## Quick start

**Prerequisites:** Docker Desktop (or Docker Engine + Compose v2)

```bash
git clone https://github.com/your-username/markethubexample.git
cd markethubexample

docker compose up --build
```

The first run pulls base images and builds all services — allow ~2 minutes.

| URL | What you see |
|-----|--------------|
| http://localhost:5173 | React dashboard |
| http://localhost:5102/products | Product API (JSON) |
| http://localhost:15672 | RabbitMQ management (guest / guest) |

### Smoke test

```bash
# Create a product
curl -s -X POST http://localhost:5102/products \
  -H "Content-Type: application/json" \
  -d '{"sku":"JJ-001","name":"Slim Fit Jeans","price":79.99,"stock":50}' | jq .

# List products
curl -s http://localhost:5102/products | jq .
```

Creating a product publishes a `ProductUpdated` event to RabbitMQ. The AmazonConnector
consumes it, builds an XML inventory feed, and POSTs it to the MockAmazonApi.
Watch the connector logs to see Polly retries when the mock returns 429 / 500 / 503:

```bash
docker compose logs -f amazon-connector
```

---

## Running tests

```bash
# Unit tests (no Docker required)
dotnet test tests/ProductService.Tests.Unit/ProductService.Tests.Unit.csproj

# Integration tests (spins up postgres:16-alpine via Testcontainers)
dotnet test tests/ProductService.Tests.Integration/ProductService.Tests.Integration.csproj

# All at once
dotnet test src/ProductService/ProductService.sln
```

---

## Project structure

```
markethubexample/
├── src/
│   ├── ProductService/               # .NET solution
│   │   ├── ProductService.Api/       # ASP.NET Core entry point, endpoints, DI
│   │   ├── ProductService.Application/  # Use cases, interfaces, DTOs
│   │   ├── ProductService.Domain/    # Product entity (pure POCO)
│   │   └── ProductService.Infrastructure/  # EF Core, repositories, MassTransit
│   ├── AmazonConnector/              # Worker Service — RabbitMQ consumer
│   ├── MockAmazonApi/                # Minimal API simulating Amazon failures
│   └── ProductsDashboard/            # Vite + React 18 + TypeScript
├── shared/
│   └── Contracts/                    # Shared message types (ProductUpdated)
├── tests/
│   ├── ProductService.Tests.Unit/    # xUnit + Moq + FluentAssertions
│   └── ProductService.Tests.Integration/  # Testcontainers + real PostgreSQL
├── .github/workflows/ci.yml          # GitHub Actions CI
└── docker-compose.yml
```

---

## Tech stack

| Concern | Choice |
|---------|--------|
| API framework | ASP.NET Core 10 Minimal APIs |
| Architecture | Clean Architecture (Domain → Application → Infrastructure/Api) |
| ORM | Entity Framework Core 10 + Npgsql (PostgreSQL) |
| Messaging | MassTransit 9 on RabbitMQ |
| HTTP resilience | `Microsoft.Extensions.Http.Resilience` (Polly 8) |
| XML serialization | `System.Xml.Serialization.XmlSerializer` |
| Logging | Serilog — compact JSON (prod) / human-readable (dev) |
| Unit tests | xUnit + Moq + FluentAssertions |
| Integration tests | Testcontainers (postgres:16-alpine) |
| CI | GitHub Actions — build, unit tests, integration tests, Docker builds |
| Frontend | React 18 + TypeScript + Vite, served by nginx |

---

## Key design decisions

**Clean Architecture layer separation** — `Application` defines `IProductRepository` and
`IEventPublisher` as interfaces. `Infrastructure` implements them. This keeps business logic
free of EF Core and MassTransit dependencies and makes it straightforward to unit-test
`ProductService` with mocks.

**Two-layer retry strategy** — Polly retries the HTTP call to Amazon up to 4 times
(1s / 2s / 4s / 8s backoff) on 429 / 500 / 503. If all HTTP retries fail, MassTransit
retries the entire consume pipeline up to 5 times (1s / 2s / 4s / 8s / 16s). After that
the message lands in `product-updated_error` in RabbitMQ — nothing is lost.

**XML feed alongside JSON REST** — Amazon's SP-API uses both patterns. The connector
sends a JSON `PUT /products` for real-time sync and a structured XML inventory feed
(`POST /feeds/inventory`) to demonstrate the `XmlSerializer`-based transformation layer.

**Repo-root Docker build context** — all Dockerfiles use `.` as the build context so they
can access both `src/<service>/` and `shared/Contracts/`. The Dockerfiles copy only the
specific project files needed for each service, keeping images lean.

**EF Core migrations in Development only** — `MigrateAsync()` runs on startup only when
`ASPNETCORE_ENVIRONMENT=Development`. Production databases are migrated explicitly.
