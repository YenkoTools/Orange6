# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Orange6 is a full-stack web application with a .NET 10 Azure Functions backend (`Service/`) and an Astro + React frontend (`Client/`), designed to deploy on Azure Static Web Apps.

## Commands

### Service (run from repo root)

```bash
dotnet restore Service/Service.slnx
dotnet build Service/Service.slnx
dotnet build Service/Service.slnx --configuration Release
dotnet test Service/Service.slnx --configuration Release --verbosity normal

# Run a single test project
dotnet test Service/tests/Api.Tests/Api.Tests.csproj

# Run the API locally (port 7001)
cd Service/src/Api && func host start --port 7001

# Publish for deployment
dotnet publish Service/src/Api/Api.csproj --configuration Release --output publish/api
```

### Client (run from `Client/`)

```bash
npm install
npm run dev        # dev server at localhost:5001
npm run build      # production build to Client/dist
npm run preview
npx astro check    # type checking
```

### Run both together (recommended)

```bash
cd Client && swa start   # SWA CLI proxies client + API, mirrors Azure hosting
```

## Architecture

### Service — Clean Architecture

The dependency flow is strictly: `Api` → `Application` → `Domain`. `Infrastructure` implements `Application` interfaces and is registered at the `Api` layer.

- **`Domain/`** — No dependencies. Contains `Entity`, `User`, `Result<T>`, `Error`, `PagedResult<T>`, and `UserErrors`. `Result`/`Result<T>` are the primary return type throughout the service (never throw for domain errors).
- **`Application/`** — Depends only on `Domain`. Implements CQRS via custom `ICommandDispatcher`/`IQueryDispatcher` with a pipeline behavior chain (Validation → Performance → Metrics). FluentValidation validators live in `Features/Users/Validation/`. Dispatchers resolve `ICommandPipelineBehavior<,>` / `IQueryPipelineBehavior<,>` from DI in registration order.
- **`Infrastructure/`** — Implements `IUserRepository` and `IMetricsService`. Uses Dapper + SQLite (`Microsoft.Data.Sqlite`) via `DatabaseFactory`/`DatabaseInitializer`. Database config is bound from `DatabaseOptions` section in configuration.
- **`Api/`** — Azure Functions v4 isolated worker. Endpoints in `Endpoints/` receive HTTP triggers and delegate to dispatchers. `ResultExtensions.ToProblemDetails()` maps `Result` failures to RFC-compliant `ProblemDetails` responses. OpenAPI annotations are on each Function method.

### Adding a new feature (typical pattern)

1. Add domain types/errors in `Domain/`
2. Add command/query + handler in `Application/Features/<Feature>/`
3. Add FluentValidation validator in `Application/Features/<Feature>/Validation/`
4. Register handler in `Application/Extensions/ServiceCollectionExtensions.cs`
5. Add repository method to `Application/Interfaces/IRepository.cs` and implement in `Infrastructure/Repositories/`
6. Add Azure Function endpoint in `Api/Endpoints/`

### Client

Astro 5 with React 19 islands and Tailwind CSS v4. Pages in `Client/src/pages/`, layouts in `Client/src/layouts/`, components in `Client/src/components/`. The SWA CLI config (`swa-cli.config.json`) points the API proxy to `http://localhost:7001`.

## Key Conventions

- All handlers return `Result` or `Result<T>` — never throw for expected errors
- `Result.IsNotFound` is detected by `Error.Code` starting with `"NotFound"` — follow this convention for new not-found errors
- Pipeline behaviors are order-sensitive; new behaviors are appended after existing ones in `ServiceCollectionExtensions`
- Tests use xUnit + NSubstitute; test projects mirror the source project they cover under `Service/tests/`
- .NET SDK version is pinned to `10.0.x` via `Service/global.json`
