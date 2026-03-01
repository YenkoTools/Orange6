# Orange6

Orange6 is a full-stack web application consisting of an Azure Functions API backend and an Astro-based frontend client, designed to be hosted on Azure Static Web Apps.

---

## Project Structure

```
Orange6/
├── Api/        # Azure Functions v4 (.NET 8) backend
└── Client/     # Astro + React + Tailwind CSS frontend
```

---

## Api

The `Api` project is an [Azure Functions v4](https://learn.microsoft.com/azure/azure-functions/functions-versions) application running on the **.NET 8 isolated worker model**. It exposes HTTP-triggered functions that serve as the backend for the client application.

**Key technologies:**
- .NET 8
- Azure Functions v4 (isolated worker)
- ASP.NET Core integration (`Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore`)
- Application Insights telemetry

**Functions:**
| Function | Trigger | Method(s) | Description |
|---|---|---|---|
| `HelloFunction` | HTTP | GET, POST | Returns a welcome message. |

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

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
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

### Api

```bash
# Restore dependencies
dotnet restore Api/Api.csproj

# Build (Debug)
dotnet build Api/Api.csproj

# Build (Release)
dotnet build Api/Api.csproj --configuration Release

# Run tests
dotnet test --configuration Release --verbosity normal

# Publish (Release) — outputs to publish/api
dotnet publish Api/Api.csproj --configuration Release --output publish/api
```

### Client

```bash
# Install dependencies
cd Client
npm install

# Type-check (Astro + TypeScript)
npx astro check

# Build for production — outputs to Client/dist
npm run build

# Preview the production build locally
npm run preview
```

---

## Development Mode

The API dev server runs on `http://localhost:7071` and the client dev server runs on `http://localhost:4321`. Both can be started independently or together via the SWA CLI.

### Run the Api

```bash
cd Api
func host start --port 7071
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
| **Build & Test Api** | Sets up .NET 8, restores dependencies, builds in Release mode, runs any test projects found, and publishes the Functions app to a staging artifact. |
| **Build Client** | Sets up Node.js 20, installs dependencies via `npm ci`, runs `astro check` for type checking, and builds the Astro site to a staging artifact. |
| **Deploy to Azure SWA** | Runs only after both build jobs succeed. Downloads the pre-built client and API artifacts and deploys them to Azure Static Web Apps using the `azure/static-web-apps-deploy` action. Both the Oryx client and API build steps are skipped since the artifacts are already built. |

**Required secret:**

| Secret | Description |
|---|---|
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Deployment token from the Azure Static Web Apps resource. Add it under **Settings → Secrets and variables → Actions**. |
