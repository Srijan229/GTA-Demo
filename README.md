# GTA Application

Local-first replacement for the George Mason University CEC IST GTA Canvas application. The new system preserves the source application's business workflows while replacing Canvas and Dataverse-specific structures with a maintainable React, ASP.NET Core, and MySQL application.

## Status

The repository is in the assessment and architecture-foundation checkpoint. The original `.msapp` has been inspected, but application code is not yet considered feature-complete.

## Prerequisites

- Node.js 24 or a supported LTS release
- npm 11+
- .NET 10 SDK (pinned by `global.json`)
- Docker Desktop with Docker Compose

## Containerized local services

- Web application and API: `http://localhost:5080`
- MySQL: `localhost:3307`, database `team2_wp`
- Mailpit UI: `http://localhost:8025`

## Start local infrastructure

Create a local environment file from the example and replace its development-only password:

```powershell
Copy-Item .env.example .env
docker compose up -d
```

Compose builds and starts the React/ASP.NET application, MySQL, and Mailpit. The application waits for MySQL, applies EF Core migrations, and uses named volumes for the database, documents, and ASP.NET data-protection keys.

Check the stack with:

```powershell
docker compose ps
curl.exe http://localhost:5080/health/ready
```

Health endpoints are `/health/live` and `/health/ready`. OpenAPI is available at `/openapi/v1.json` in Development.

Restore and verify the backend with:

```powershell
dotnet restore apps/api/Gta.Application.sln
dotnet build apps/api/Gta.Application.sln --configuration Release
dotnet test apps/api/Gta.Application.sln --configuration Release
```

Do not use the planned development authentication mechanism in production.

## Free Render demo

[`render.yaml`](render.yaml) defines a single free Docker web service for a controlled demonstration. It keeps the normal MySQL configuration unchanged and explicitly selects an ephemeral SQLite database for the Render demo only.

[![Deploy to Render](https://render.com/images/deploy-to-render-button.svg)](https://render.com/deploy?repo=https://github.com/Srijan229/GTA-Demo)

To create the service from Render:

1. Connect the private GitHub repository to Render and choose **New > Blueprint**.
2. Select the repository and confirm that the service plan is **Free**.
3. Enter `DemoAccess__Password` when prompted. Use a unique password of at least 16 characters.
4. Do not add a payment method if the requirement is that Render suspend the service instead of billing for overages.
5. After deployment, open the generated `onrender.com` URL. The browser first requests the demo username `gta-demo` and the password entered above; the application then presents its development role selector.

The free demo database and uploaded documents use `/tmp` and are intentionally disposable. They are recreated from non-sensitive development seed data whenever Render replaces or restarts the container. Do not upload real applicant information. The production configuration continues to use MySQL, persistent document storage, and fail-closed authentication.

## Production preparation

Create and verify a production artifact with:

```powershell
.\scripts\publish-production.ps1
.\scripts\smoke-production.ps1
```

The smoke test confirms that the compiled UI and health checks are available, protected APIs fail closed, and development authentication endpoints are absent. See [Deployment guide](docs/deployment.md) before installing anything on the GMU server.

## Documentation

- [Current-state assessment](docs/current-state-assessment.md)
- [Implementation plan](docs/implementation-plan.md)
- [Domain model](docs/domain-model.md)
- [Canvas mapping](docs/canvas-mapping.md)
- [Assumptions](docs/assumptions.md)
- [Security](docs/security.md)
- [Deployment guide](docs/deployment.md)
