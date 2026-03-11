# .NET SDK Upgrade: 8 → 10

Upgrade completed on 2026-03-11.

## Summary

All projects in `Service/` were upgraded from .NET 8 to .NET 10 (LTS, supported until November 2028). xUnit was upgraded from v2 to v3. All 264 tests pass with 0 build errors.

---

## Files Changed

### `Service/global.json`

Pinned SDK version updated.

| Field | Before | After |
|-------|--------|-------|
| `sdk.version` | `8.0.418` | `10.0.101` |

### Source Projects — `TargetFramework`

All four source projects updated from `net8.0` to `net10.0`.

| Project | File |
|---------|------|
| Domain | `Service/Domain/Domain.csproj` |
| Application | `Service/Application/Application.csproj` |
| Infrastructure | `Service/Infrastructure/Infrastructure.csproj` |
| Api | `Service/Api/Api.csproj` |

### Test Projects — `TargetFramework`

All four test projects updated from `net8.0` to `net10.0`.

| Project | File |
|---------|------|
| Domain.Tests | `Service/Tests/Domain.Tests/Domain.Tests.csproj` |
| Application.Tests | `Service/Tests/Application.Tests/Application.Tests.csproj` |
| Infrastructure.Tests | `Service/Tests/Infrastructure.Tests/Infrastructure.Tests.csproj` |
| Api.Tests | `Service/Tests/Api.Tests/Api.Tests.csproj` |

---

## Package Version Changes

### Infrastructure (`Service/Infrastructure/Infrastructure.csproj`)

| Package | Before | After | Reason |
|---------|--------|-------|--------|
| `Dapper` | 2.1.35 | 2.1.72 | Latest stable; bugfixes and .NET 10 compatibility |
| `Microsoft.Data.Sqlite` | 8.0.0 | 10.0.3 | Aligned with .NET 10 runtime version |
| `Microsoft.Extensions.Hosting.Abstractions` | 8.0.0 | 10.0.3 | Aligned with other `Microsoft.Extensions.*` packages already at 10.0.3 |

### Application.Tests (`Service/Tests/Application.Tests/Application.Tests.csproj`)

| Package | Before | After | Reason |
|---------|--------|-------|--------|
| `Microsoft.Extensions.DependencyInjection` | 10.0.0 | 10.0.3 | Latest patch aligned with other `Microsoft.Extensions.*` packages |
| `Microsoft.Extensions.Logging` | 10.0.0 | 10.0.3 | Latest patch aligned with other `Microsoft.Extensions.*` packages |

### Infrastructure.Tests (`Service/Tests/Infrastructure.Tests/Infrastructure.Tests.csproj`)

| Package | Before | After | Reason |
|---------|--------|-------|--------|
| `Microsoft.Data.Sqlite` | 8.0.0 | 10.0.3 | Aligned with the version used in the Infrastructure project |
| `Microsoft.Extensions.DependencyInjection` | 8.0.0 | 10.0.3 | Aligned with .NET 10 |

### Api.Tests (`Service/Tests/Api.Tests/Api.Tests.csproj`)

| Package | Before | After | Reason |
|---------|--------|-------|--------|
| `NSubstitute` | 5.1.0 | 5.3.0 | Latest stable with improved .NET 10 support |

---

## xUnit v2 → v3 Migration

All four test projects were upgraded from xUnit v2 (`xunit` 2.9.3) to xUnit v3 (`xunit.v3` 3.2.2).

### Package change (all test projects)

| Before | After |
|--------|-------|
| `<PackageReference Include="xunit" Version="2.9.3" />` | `<PackageReference Include="xunit.v3" Version="3.2.2" />` |

`xunit.runner.visualstudio` (3.1.5) and `Microsoft.NET.Test.Sdk` (18.3.0) were already at versions compatible with xUnit v3 — no change required.

### Code changes

**None required.** The test code is fully source-compatible between xUnit v2 and v3:

- `[Fact]` / `[Theory]` / `[InlineData]` attributes are unchanged
- All `Assert.*` methods used in the codebase are unchanged
- `Record.ExceptionAsync()` is unchanged
- The `<Using Include="Xunit" />` global using in each test csproj continues to work (namespace is still `Xunit`)

### xUnit v3 analyzer warnings (xUnit1051)

The build produces 105 `xUnit1051` warnings from the new xUnit v3 static analyzer. These are informational only and do not affect test execution. The warning recommends using `TestContext.Current.CancellationToken` instead of `CancellationToken.None` in test method calls to improve test cancellation responsiveness. These can be addressed in a follow-up if desired.

---

## CI/CD — `.github/workflows/build.yml`

| Field | Before | After |
|-------|--------|-------|
| Job comment | `Build & test the .NET 8 isolated Functions API` | `Build & test the .NET 10 isolated Functions API` |
| `dotnet-version` | `"8.0.x"` | `"10.0.x"` |
| Step name | `Setup .NET 8` | `Setup .NET 10` |

---

## Packages Left Unchanged

The following packages were already at versions compatible with .NET 10 and required no changes:

| Package | Version | Notes |
|---------|---------|-------|
| `FluentValidation` | 12.1.1 | Targets .NET 8+; compatible with .NET 10 |
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | Same |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.3 | Already at .NET 10 release |
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.3 | Already at .NET 10 release |
| `Microsoft.Extensions.Configuration.Abstractions` | 10.0.3 | Already at .NET 10 release |
| `Microsoft.Extensions.Options.ConfigurationExtensions` | 10.0.3 | Already at .NET 10 release |
| `Microsoft.Azure.Functions.Worker` | 2.51.0 | Supports .NET 10 isolated worker |
| `Microsoft.Azure.Functions.Worker.Sdk` | 2.0.7 | Supports .NET 10 |
| `Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore` | 2.1.0 | Supports .NET 10 |
| `Microsoft.Azure.Functions.Worker.Extensions.OpenApi` | 1.6.0 | Supports .NET 10 |
| `Microsoft.Azure.Functions.Worker.ApplicationInsights` | 2.50.0 | Supports .NET 10 |
| `Microsoft.ApplicationInsights.WorkerService` | [2.23.0] | Version locked; 3.0.0 has breaking changes incompatible with Azure Functions |
| `Microsoft.NET.Test.Sdk` | 18.3.0 | Already latest; supports xUnit v3 |
| `xunit.runner.visualstudio` | 3.1.5 | Already v3.x; supports both xunit v2 and v3 |
| `coverlet.collector` | 8.0.0 | Latest stable |
| `Moq` | 4.20.72 | Latest stable; no .NET 10 issues |

---

## Source Code Changes

**None.** No application or test source code required modification. All code patterns used (records, async/await, LINQ, `System.Diagnostics.Metrics`, FluentValidation pipeline behaviors, Dapper ORM, Azure Functions bindings) are fully compatible with .NET 10 without changes.

---

## Test Results

| Project | Tests | Passed | Failed |
|---------|-------|--------|--------|
| Domain.Tests | 65 | 65 | 0 |
| Application.Tests | 82 | 82 | 0 |
| Infrastructure.Tests | 72 | 72 | 0 |
| Api.Tests | 45 | 45 | 0 |
| **Total** | **264** | **264** | **0** |
