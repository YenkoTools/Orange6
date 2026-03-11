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
│   └── Service.sln           # Solution file
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

**Key contents:**
- `Repositories/` — `InMemoryUserRepository`
- `Services/` — `MetricsService`

---

## Client

The `Client` project is a static frontend built with [Astro 5](https://astro.build), [React](https://react.dev), and [Tailwind CSS v4](https://tailwindcss.com). It is configured to work alongside the Azure Functions API using the [Azure Static Web Apps CLI](https://azure.github.io/static-web-apps-cli/).

**Key technologies:**
- Astro 5
- React 19
- Tailwind CSS v4
- SWA CLI (`swa-cli.config.json`)

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10)
- [Node.js 20+](https://nodejs.org) and npm

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

## Build, Test & Package

### Service

```bash
# Restore dependencies
dotnet restore Service/Service.sln

# Build (Debug)
dotnet build Service/Service.sln

# Build (Release)
dotnet build Service/Service.sln --configuration Release

# Run tests
dotnet test Service/Service.sln --configuration Release --verbosity normal

# Publish Api (Release) — outputs to publish/api
dotnet publish Service/Api/Api.csproj --configuration Release --output publish/api
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
cd Service/Api
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

The workflow is defined in [.github/workflows/azure-static-web-apps.yml](.github/workflows/azure-static-web-apps.yml).

**Trigger:** Manual only (`workflow_dispatch`). Run it from the **Actions** tab → **Run workflow**, choosing a target environment (`production` or `staging`).

**Jobs:**

| Job | Description |
|---|---|
| **Build & Test Service** | Sets up .NET 10, restores dependencies from `Service/Service.sln`, builds in Release mode, runs any test projects found, and publishes the Functions app (`Service/Api`) to a staging artifact. |
| **Build Client** | Sets up Node.js 20, installs dependencies via `npm ci`, runs `astro check` for type checking, and builds the Astro site to a staging artifact. |
| **Deploy to Azure SWA** | Runs only after both build jobs succeed. Downloads the pre-built client and API artifacts and deploys them to Azure Static Web Apps using the `azure/static-web-apps-deploy` action. Both the Oryx client and API build steps are skipped since the artifacts are already built. |

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
