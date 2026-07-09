# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Structure

Located at `src/ProductService/ProductService.sln`, this is a .NET 10.0 (net10.0) ASP.NET Core Web API solution following a layered architecture with 4 projects:

| Project | Path | Purpose |
|---------|------|---------|
| **ProductService.Api** | `ProductService.Api/` | Web API entry point, HTTP endpoints, DI configuration |
| **ProductService.Application** | `ProductService.Application/` | Application layer (business logic, use cases) |
| **ProductService.Domain** | `ProductService.Domain/` | Domain layer (entities, domain models) |
| **ProductService.Infrastructure** | `ProductService.Infrastructure/` | Infrastructure layer (persistence, external services) |

### Dependency Direction

```
ProductService.Api
  ├── ProductService.Application
  │     └── ProductService.Domain
  └── ProductService.Infrastructure
        └── ProductService.Domain
```

Api depends on both Application and Infrastructure. Both Application and Infrastructure depend on Domain. Domain has no project references (pure POCOs).

### Key Files

- `ProductService.Api/Program.cs` — App startup, service registration, endpoint mapping
- `ProductService.Domain/Entities/Products.cs` — Domain entity (currently the only source file)
- `ProductService.Api/appsettings.json` / `appsettings.Development.json` — Configuration
- `ProductService.Api/Properties/launchSettings.json` — Kestrel/IIS Express profiles

## Commands

```bash
# Build the solution
dotnet build src/ProductService/ProductService.sln

# Run the API (from the ProductService directory)
cd src/ProductService && dotnet run --project ProductService.Api

# Run tests (no test projects exist yet)
dotnet test src/ProductService/ProductService.sln
```

## Architecture Notes

- **Target framework:** net10.0
- **Nullable reference types:** enabled
- **Implicit usings:** enabled
- **OpenAPI:** configured via `AddOpenApi()` (development only)
- **Logging:** default + Microsoft.AspNetCore at Warning level
- The Api project uses the `ProductService` root namespace (correct). The Domain project has a typo in its `<RootNamespace>` property (`ProducServiceDomain` instead of `ProductServiceDomain`).
