# Release Notes — v0.0.3

## Overview

This release builds on v0.0.2 with significant enhancements across all Service assemblies, including a full CRUD REST API for Users, a migration from in-memory storage to SQLite with seed data, comprehensive unit test coverage, and client-side configuration corrections.

---

## Service Assembly Changes

### Api

- Added `CreateUser` POST endpoint (`/api/users`) with validation and result handling
- Added `UpdateUser` PUT endpoint for updating an existing user by ID
- Added `DeleteUser` DELETE endpoint for removing a user by ID (`/api/users/{id}`)
- Added `GetUsers` GET endpoint with pagination support (`/api/users`)
- Added `GetUserById` GET endpoint for retrieving a single user by ID (`/api/users/{id}`)
- Added OpenAPI documentation for the paged users retrieval endpoint
- Added `local.settings.json` and updated README with Azure Functions local development instructions
- Updated FluentValidation and `Microsoft.Extensions.Logging.Abstractions` package versions

### Application

- Added `CreateUserCommand`, handler, and FluentValidation validator for user creation
- Added `UpdateUserCommand`, handler, and FluentValidation validator for user updates
- Added `DeleteUserCommand` and handler for user deletion
- Added `GetUsersQuery` and handler implementing paged user retrieval
- Added `GetUserByIdQuery` and handler for single user lookup

### Domain

- No new domain entities or value objects introduced; existing `User`, `Entity`, `Result<T>`, `Error`, and `PagedResult<T>` primitives support new use cases

### Infrastructure

- Replaced `InMemoryUserRepository` with a SQLite-backed `UserRepository` using Dapper
- Introduced `DatabaseFactory` for SQLite connection management
- Introduced `DatabaseInitializer` to create the schema and seed sample data on startup
- Seeded the database with 25 Harry Potter characters as initial sample data
- Extended seed data with 10 additional Star Wars characters

---

## Tests Assembly Changes

- Added XUnit test projects for all four Service layers: Api, Application, Domain, and Infrastructure
- Added unit tests for `Api` Azure Functions (HTTP trigger handlers)
- Added unit tests for `Application` command and query handlers
- Added unit tests for `Domain` entities and value objects
- Added unit tests for `Infrastructure` repositories and database helpers
- Updated test project NuGet dependencies to the latest compatible versions

---

## Client Changes

- Corrected project name in `package.json` and `package-lock.json`
- Corrected the API location entry in `swa-cli.config.json` to align with the Azure Static Web Apps configuration

---

## Documentation Changes

- Updated root `README.md` to reflect the Clean Architecture reorganization into the `Service/` directory
- Updated `README.md` endpoint table and project structure sections to include all new REST endpoints

---

## Summary

| Assembly | Changes |
|---|---|
| **Api** | 5 new HTTP endpoints, OpenAPI docs, local dev config, dependency updates |
| **Application** | 5 new command/query handlers with validators |
| **Domain** | No structural changes; existing primitives extended to support new use cases |
| **Infrastructure** | SQLite + Dapper repository replacing in-memory store; seed data (Harry Potter + Star Wars) |
| **Tests** | Full XUnit test suite across all 4 layers |
| **Client** | Config and package metadata corrections |
| **Docs** | README updated for new structure and endpoints |
