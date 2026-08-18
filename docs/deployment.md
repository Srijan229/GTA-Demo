# GMU deployment preparation

This guide prepares the GTA Application for `team2.it492.cecnet.gmu.edu`. It does not authorize or perform changes on the GMU server.

## Current readiness

- MySQL 8.4 is the supported database engine.
- Application tables use the `gta_` prefix so they can coexist with unrelated tables in `team2_wp`.
- EF Core migrations are MySQL-specific and tracked in `Gta.Application.Infrastructure/Persistence/Migrations`.
- The ASP.NET Core process serves both the API and the compiled React application.
- A production publish directory and Linux container image can be built locally.
- Production authentication fails closed. Protected APIs return `401`, and development login endpoints are not mapped.

Production deployment remains blocked until GMU confirms the authentication mechanism: GMU OIDC/SSO, a trusted upstream identity proxy, or an explicitly approved local account design.

## Local verification

```powershell
Copy-Item .env.example .env
# Replace local-only example passwords in .env.
docker compose up -d
docker compose ps
curl.exe http://localhost:5080/health/ready
```

The Compose stack contains the combined React/ASP.NET application, MySQL, and Mailpit. Named volumes persist the database, documents, and ASP.NET data-protection keys. Development is the local default; deployment must set `ASPNETCORE_ENVIRONMENT=Production`.

Expected smoke results:

| Check | Expected |
|---|---:|
| `/health/ready` | 200 |
| `/` compiled React UI | 200 |
| `/api/v1/admin/access` without identity | 401 |
| `/api/v1/development/users` | 404 |

## GMU host findings and prerequisite

The read-only inspection on August 16, 2026 found Rocky Linux 9.5, Apache, and MySQL 8.0.36. Docker and Podman were not installed. The `team2` account can write to `/srv/www/team2/wordpress`, but unattended sudo is unavailable. Apache has the required proxy modules, although its generated virtual-host configuration requires an administrator-managed change.

Before container deployment, the GMU administrator must provide a supported container engine with Compose, arrange persistent startup, and proxy HTTPS traffic from Apache to the application container on loopback port 5080. Do not copy the stack to the server until those prerequisites are confirmed.

## Configuration

Start from `infrastructure/deployment/gta.env.example`. Never copy a real password, SSH key, applicant document, or production environment file into Git.

Required server-owned values:

- `ConnectionStrings__GtaDatabase`
- production authentication settings, once selected
- `Email__SmtpHost`, port, sender, and any credentials
- `DocumentStorage__RootPath`

Set `Database__ApplyMigrations=true` only for the controlled migration start. Return it to `false` after the migration succeeds.

## Reverse proxy and service

- The inspected server uses Apache, not Nginx.
- Apache already loads its HTTP proxy, headers, rewrite, and SSL modules.
- The `team2` virtual host is generated from centrally managed templates, so permanent proxy changes must be made by the GMU administrator.

Do not directly edit the generated virtual-host file. The administrator should validate Apache configuration before reloading it.

## First server inspection

After VPN and SSH access are available, perform read-only checks first:

1. Record OS, CPU architecture, disk space, memory, and installed runtimes.
2. Record active web server, virtual-host configuration, certificate management, and listening ports.
3. Record current processes, services, and application files.
4. Inspect MySQL version and table names without selecting applicant data.
5. Confirm whether `team2` has `sudo` and whether `team2app` may create/alter prefixed tables.
6. Confirm SMTP and authentication integration expectations.

Back up the existing application directory, web-server configuration, and `team2_wp` database before the first write.

## Release and rollback

Deploy each build into a new timestamped release directory. Apply migrations before switching `current`. Keep the previous release and database backup until smoke tests pass.

Rollback procedure:

1. Stop `gta-application`.
2. Point `current` to the previous release.
3. Restore the database only if the applied migration is not backward compatible.
4. Start the service and verify health, UI, authentication, and an authorized workflow.

Never automatically roll back a database without first verifying the exact migration and backup target.
