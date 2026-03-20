<a href="https://github.com/jkulba/Orange6/">
    <img alt="The Orange Service" src="https://github.com/jkulba/Orange6/blob/main/orange6.png"
    width="150" height="175">
</a>

# The Orange6 Client and Service

Orange6 is a full-stack web application consisting of an Azure Functions API backend and an Astro-based frontend client, designed to be hosted on Azure Static Web Apps.

---

## Project Structure

```
Orange6/
├── Service/                  # .NET 10 backend (Clean Architecture)
│   ├── Api/                  # Azure Functions v4 entry point
│   ├── Application/          # Use cases, commands, queries, and behaviors
│   ├── Domain/               # Entities, value objects, and domain errors
│   ├── Infrastructure/       # Repositories and external services
│   └── Service.slnx          # Solution file
└── Client/                   # Astro + React + Tailwind CSS frontend
```

---

## Service

The `Service` directory contains the .NET 10 backend, organized following **Clean Architecture** principles. Each layer has a well-defined responsibility and depends only on layers closer to the domain.

### Api

The `Service/Api` project is an [Azure Functions v4](https://learn.microsoft.com/azure/azure-functions/functions-versions) application running on the **.NET 10 isolated worker model**. It is the entry point for all HTTP requests and delegates work to the `Application` layer.

**Key technologies:**
- .NET 10
- Azure Functions v4 (isolated worker)
- ASP.NET Core integration (`Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore`)
- OpenAPI support (`Microsoft.Azure.Functions.Worker.Extensions.OpenApi`)
- Application Insights telemetry

**Endpoints:**
| Function | Trigger | Method(s) | Route | Description |
|---|---|---|---|---|
| `CreateUser` | HTTP | POST | `/api/users` | Creates a new user in the system. |
| `UpdateUser` | HTTP | PUT | `/api/users/{id}` | Updates an existing user by their unique identifier. |
| `GetUserById` | HTTP | GET | `/api/users/{id}` | Retrieves a single user by their unique identifier. |
| `GetUsers` | HTTP | GET | `/api/users` | Retrieves a paginated list of users (`pageNumber`, `pageSize` query params). |
| `DeleteUser` | HTTP | DELETE | `/api/users/{id}` | Deletes an existing user by their unique identifier. |
| `Version` | HTTP | GET | `/api/version` | Returns the current API version information. |

### Application

The `Service/Application` project contains the application use cases implemented as commands and queries (CQRS pattern). It defines abstractions for dispatching, pipeline behaviors (e.g., logging, validation), and repository interfaces.

**Key technologies:**
- CQRS with `ICommandDispatcher` / `IQueryDispatcher`
- FluentValidation pipeline behavior
- No dependency on infrastructure or framework concerns

### Domain

The `Service/Domain` project contains the core business entities and domain primitives. It has no dependencies on any other project.

**Key contents:**
- `Entities/` — `User`, base `Entity`
- `Common/` — `Result<T>`, `Error`, `PagedResult<T>`

### Infrastructure

The `Service/Infrastructure` project implements the interfaces defined in `Application`. It contains repository implementations and external service integrations.

**Key technologies:**
- Dapper ORM for data access
- SQLite (`Microsoft.Data.Sqlite`) as the database engine
- `DatabaseFactory` / `DatabaseInitializer` for connection management and schema setup
- Database path is configured via the `Database` section in application settings (`DatabaseOptions`)

**Key contents:**
- `Repositories/` — `UserRepository` (SQLite-backed via Dapper)
- `Services/` — `MetricsService`
- `Data/` — `DatabaseFactory`, `DatabaseInitializer`
- `Configuration/` — `DatabaseOptions`

---

## Client

The `Client` project is a static frontend built with [Astro 6](https://astro.build), [React](https://react.dev), and [Tailwind CSS v4](https://tailwindcss.com). It is configured to work alongside the Azure Functions API using the [Azure Static Web Apps CLI](https://azure.github.io/static-web-apps-cli/).

**Key technologies:**
- Astro 6
- React 19
- Tailwind CSS v4
- SWA CLI (`swa-cli.config.json`)

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10)
- [Node.js 22+](https://nodejs.org) and npm

### Install Azure Functions Core Tools

The Azure Functions Core Tools provide a local development experience for creating, running, and deploying Azure Functions.

**macOS (Homebrew):**
```bash
brew tap azure/functions
brew install azure-functions-core-tools@4
```

**Linux (APT):**
```bash
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-$(lsb_release -cs)-prod $(lsb_release -cs) main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update
sudo apt-get install azure-functions-core-tools-4
```

**Windows (npm):**
```bash
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

Verify the installation:
```bash
func --version
```

### Install Azure Static Web Apps CLI

The SWA CLI provides a local development server that emulates the Azure Static Web Apps environment, proxying requests between the client and the API.

```bash
npm install -g @azure/static-web-apps-cli
```

Verify the installation:
```bash
swa --version
```

---

## Code Quality & Static Analysis

### Service — SonarAnalyzer (.NET)

The Service uses [SonarAnalyzer.CSharp](https://github.com/SonarSource/sonar-dotnet) which runs automatically as part of every build. There is nothing extra to install — it is included as a NuGet package and the compiler surfaces findings as warnings in the build output.

**Run analysis:**
```bash
# Findings appear inline during any build
dotnet build Service/Service.slnx

# Run with full verbosity to see all analyzer diagnostics
dotnet build Service/Service.slnx --verbosity normal 2>&1 | grep -E "warning S[0-9]+"
```

**How to read the output:**

Findings appear as standard compiler warnings with an `S`-prefixed rule code:
```
warning S6667: Logging in a catch clause should pass the caught exception as a parameter.
warning S1075: Refactor your code not to use hardcoded absolute paths or URIs.
```

Each rule code links to the [SonarSource rule catalog](https://rules.sonarsource.com/csharp) where the rationale and compliant/non-compliant examples are documented.

**Suppressing a finding** when it is a known false positive:
```csharp
#pragma warning disable S1075 // reason why this suppression is intentional
Url = new Uri("https://opensource.org/licenses/MIT"),
#pragma warning restore S1075
```

---

### Client — ESLint with SonarJS

The Client uses ESLint with three plugins configured in [`Client/eslint.config.js`](Client/eslint.config.js):

| Plugin | What it detects |
|---|---|
| `@eslint/js` + `typescript-eslint` | Undeclared variables, unsafe `any`, unused imports, type misuse |
| `eslint-plugin-sonarjs` | Cognitive complexity, duplicate code, always-true conditions — the same S-rule family as the .NET analyser |
| `eslint-plugin-astro` | Invalid Astro component syntax, prop misuse, accessibility issues in `.astro` files |

**Run analysis (from `Client/`):**
```bash
# Report all findings
npm run lint

# Auto-fix safe violations (formatting, simple rules)
npm run lint:fix

# Lint a single file
node_modules/.bin/eslint src/pages/index.astro
```

**How to read the output:**

Each finding shows the file, line, rule ID, and description:
```
src/components/UserList.tsx
  14:5  warning  Cognitive Complexity of this function is too high  sonarjs/cognitive-complexity
  27:3  error    Unexpected any. Specify a different type           @typescript-eslint/no-explicit-any
```

**Suppressing a finding** when it is a known false positive:
```ts
// eslint-disable-next-line sonarjs/cognitive-complexity
function complexButNecessary() {
```

---

## Build, Test & Package

### Service

```bash
# Restore dependencies
dotnet restore Service/Service.slnx

# Build (Debug)
dotnet build Service/Service.slnx

# Build (Release)
dotnet build Service/Service.slnx --configuration Release

# Run tests
dotnet test Service/Service.slnx --configuration Release --verbosity normal

# Publish Api (Release) — outputs to publish/api
dotnet publish Service/src/Api/Api.csproj --configuration Release --output publish/api
```

### Client

```bash
# Install dependencies
cd Client
npm install

# Build for production — outputs to Client/dist
npm run build

# Preview the production build locally
npm run preview
```

---

## Development Mode

The API dev server runs on `http://localhost:7001` and the client dev server runs on `http://localhost:5001`. Both can be started independently or together via the SWA CLI.

### Run the Service Api

```bash
cd Service/src/Api
func host start --port 7001
```

### Run the Client

```bash
cd Client
npm run dev
```

### Run both together with the SWA CLI (recommended)

The SWA CLI proxies the client and API together, mirroring the Azure Static Web Apps hosting environment.

```bash
cd Client
swa start
```

---

## GitHub Action

The workflow is defined in [.github/workflows/build.yml](.github/workflows/build.yml).

**Trigger:** Manual only (`workflow_dispatch`). Run it from the **Actions** tab → **Run workflow**, choosing a target environment (`production`, `staging`, or `testing`).

**Jobs:**

| Job | Description |
|---|---|
| **Build & Test Api** | Sets up .NET 10, restores and builds in Release mode, runs all test projects, and publishes the Functions app to a staging artifact. SonarAnalyzer runs automatically as part of the build and will fail the job on any error-level findings. |
| **Build Client** | Sets up Node.js 22, installs dependencies via `npm ci`, runs ESLint (`npm run lint`), runs `astro check` for TypeScript type checking, and builds the Astro site to a staging artifact. |
| **Approve Deployment** | Manual approval gate — configure required reviewers in **Settings → Environments**. |
| **Deploy to Azure SWA** | Runs only after approval. Downloads the pre-built artifacts and deploys to Azure Static Web Apps. Both Oryx build steps are skipped since artifacts are already built. |

**Required secret:**

| Secret | Description |
|---|---|
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Deployment token from the Azure Static Web Apps resource. Add it under **Settings → Secrets and variables → Actions**. |

---

## Contributors

<!-- readme: collaborators,contributors -start -->
<table>
	<tbody>
		<tr>
            <td align="center">
                <a href="https://github.com/jkulba">
                    <img src="https://avatars.githubusercontent.com/u/830075?v=4" width="100;" alt="jkulba"/>
                    <br />
                    <sub><b>Jim Kulba</b></sub>
                </a>
            </td>
		</tr>
	<tbody>
</table>
<!-- readme: collaborators,contributors -end -->
