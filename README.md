# Zilean (Maintained Fork)

<img src="docs/Writerside/images/zilean-logo.jpg" alt="zilean logo" width="300" height="300">

Zilean is a service that allows you to search for [DebridMediaManager](https://github.com/debridmediamanager/debrid-media-manager) sourced content shared by users.
This can then be configured as a Torznab indexer in your favorite content application.
Newly added is the ability for Zilean to scrape from your running Zurg instance, and from other running Zilean instances.

This is an actively maintained fork of [iPromKnight/zilean](https://github.com/iPromKnight/zilean) (v3.5.0, last upstream commit May 2025).

Upstream documentation: [https://ipromknight.github.io/zilean/](https://ipromknight.github.io/zilean/)

## Requirements

Zilean requires only **PostgreSQL 16+**. Elasticsearch is **NOT** required and was removed in v2.0.

## Docker Image

```
ghcr.io/thoroslives/zilean:latest
```

## Fork Changes

All changes beyond upstream v3.5.0:

### v3.6.0
- **Flexible database configuration** - supports `Zilean__Database__ConnectionString` env var (backwards compat), individual `POSTGRES_*` env vars, or sensible defaults. Uses `NpgsqlConnectionStringBuilder` for proper escaping of special characters in passwords.
- **Incremental DMM sync** - replaces the 1.2GB zip download with `git clone --depth 1` on first run and `git pull` on subsequent runs. Supports `GITHUB_TOKEN` for authenticated requests (5,000 req/hr vs 60). Includes exponential backoff retry.
- **Logging config preservation** - `logging.json` is only written if it doesn't exist, preserving user customizations across restarts.

### v3.8.0
- **Increased MaxFilteredResults default** - bumped from 200 to 500. The previous default caused season packs and higher-quality releases to be excluded from search results when a show has many indexed torrents across qualities, languages and groups.
- **Auto-release CI/CD** - push to main now automatically bumps version, creates a GitHub release with generated notes, and builds/pushes the Docker image.

### v3.7.0
- **Security hardening** - warns at startup if PostgreSQL password is empty or set to default "postgres". Docker-compose example no longer exposes Postgres ports.
- **Database startup resilience** - retries database connection up to 5 times with 5-second delays before running migrations. Clear error messages on failure including host and database name.
- **Filtered search fix** - `/dmm/filtered` with short query strings (e.g., "1923") combined with season/episode filters no longer returns 0 results. Similarity threshold is automatically lowered when structured filters provide precision.
- **Scraping toggle fix** - setting `EnableScraping=false` now correctly hides the on-demand-scrape endpoint while keeping search endpoints functional.
- **Timezone support** - set `TZ` env var (e.g., `TZ=Australia/Sydney`) to display log timestamps in your local timezone. `tzdata` package included in the image.
- **Readiness health check** - new `/healthchecks/ready` endpoint that verifies database connectivity. Used by the Dockerfile HEALTHCHECK for orchestrator integration.
- **HEALTHCHECK instruction** - Docker image includes a built-in health check (30s interval, 60s start period) so orchestrators can detect readiness.
- **Graceful error handling** - database errors no longer kill the process immediately (`Process.Kill()` replaced with proper exception propagation). Search errors are logged instead of silently swallowed.
- **Startup config validation** - validates configuration values (cron syntax, numeric ranges, required fields) at startup with clear error messages.
- **DMM sync progress reporting** - periodic progress logs during sync showing files processed, percentage complete, and new torrents found.
- **ISystemClock deprecation fix** - removed deprecated `ISystemClock` usage in authentication handler.

## Configuration

### Database Connection

Three ways to configure the database connection (checked in this order):

#### 1. Full Connection String (recommended for existing setups)

```yaml
environment:
  - Zilean__Database__ConnectionString=Host=postgres;Database=zilean;Username=postgres;Password=mypass;Include Error Detail=true;Timeout=30;CommandTimeout=3600;
```

#### 2. Individual Environment Variables

```yaml
environment:
  - POSTGRES_HOST=postgres      # default: localhost
  - POSTGRES_PORT=5432          # default: 5432
  - POSTGRES_DB=zilean          # default: zilean
  - POSTGRES_USER=postgres      # default: postgres
  - POSTGRES_PASSWORD=mypass    # default: (empty)
```

#### 3. Defaults

If no database env vars are set, connects to `localhost:5432/zilean` as `postgres` with no password (suitable for trust auth).

### DMM Sync

Set `GITHUB_TOKEN` to avoid GitHub API rate limiting during DMM hashlist sync:

```yaml
environment:
  - GITHUB_TOKEN=ghp_xxxxxxxxxxxx
```

The initial DMM sync is **resumable** - if interrupted, it picks up where it left off on next startup. Expected initial sync duration varies by hardware (typically 30min-2hrs for parsing, longer for IMDB matching).

### Timezone

Set the `TZ` environment variable to display log timestamps in your local timezone:

```yaml
environment:
  - TZ=Australia/Sydney
```

### PostgreSQL Shared Memory

PostgreSQL's default shared memory (`shm_size`) of 64MB is too small for Zilean's bulk DMM upserts. You'll get errors like:

```
could not resize shared memory segment "/PostgreSQL.xxx" to 67146560 bytes: No space left on device
```

Set `shm_size: 256m` on your PostgreSQL container to fix this. See the docker-compose example below.

## Security

**Never expose your PostgreSQL port to the internet.** Multiple users have been compromised with crypto miners after exposing Postgres with default credentials. Zilean will warn you at startup if your database password is empty or set to the default "postgres".

Best practices:
- Always set a strong `POSTGRES_PASSWORD`
- Do NOT add `ports:` to your Postgres container unless you need external access
- If you must expose Postgres, use a firewall to restrict access to trusted IPs
- Use Docker's internal networking - Zilean connects to Postgres by container name

## Resource Usage

- **Initial sync:** Expect high CPU for 10-30 minutes during the first DMM sync. This is normal - Zilean is parsing ~1.2M HTML files and performing bulk database upserts. Progress is logged periodically.
- **Subsequent syncs:** Lightweight. Only pulls new/changed files via `git pull` and processes the diff.
- **If high usage persists** after the initial sync completes: check for security compromise (see Security section above). Persistent high CPU with unfamiliar processes is a red flag.
- PostgreSQL requires `shm_size: 256m` for bulk operations (see PostgreSQL Shared Memory section).

## Multi-Instance Deployment

For high-availability or high-traffic setups, you can run multiple Zilean instances:

- **1 scraper instance** (`Zilean__Dmm__EnableScraping=true`) - handles DMM sync and data ingestion
- **N API instances** (`Zilean__Dmm__EnableScraping=false`, `Zilean__Dmm__EnableEndpoint=true`) - serve search queries only
- All instances share the same PostgreSQL database
- `PreventOverlapping("SyncJobs")` prevents concurrent scraping within an instance
- PostgreSQL's default `max_connections=100` is sufficient for typical deployments

## Health Checks

- `/healthchecks/ping` - lightweight liveness check (always returns 200)
- `/healthchecks/ready` - readiness check that verifies database connectivity (returns 503 if DB is unreachable)

## Troubleshooting

### Database not found / "does not exist"

Common causes:
- PostgreSQL hasn't finished initializing - Zilean now retries 5 times with 5-second delays
- Wrong credentials - check `POSTGRES_PASSWORD` matches between Zilean and Postgres containers
- Volume permissions - on Unraid/Synology, ensure the Postgres data volume has correct ownership

### "could not resize shared memory segment"

Set `shm_size: 256m` on your PostgreSQL container. See the docker-compose example.

### Search returns 0 results

- Ensure the initial DMM sync has completed (check logs for "DMM sync complete")
- For filtered searches with short titles, the similarity threshold is automatically adjusted

## Docker Compose Example

```yaml
services:
  zilean:
    image: ghcr.io/thoroslives/zilean:latest
    container_name: zilean
    restart: unless-stopped
    ports:
      - "8181:8181"
    volumes:
      - zilean-data:/app/data
    environment:
      - POSTGRES_HOST=postgres
      - POSTGRES_PASSWORD=your_strong_password_here
      - GITHUB_TOKEN=ghp_xxxxxxxxxxxx  # optional, recommended
      - TZ=UTC                         # optional, set your timezone
    depends_on:
      postgres:
        condition: service_healthy

  postgres:
    image: postgres:16-alpine
    container_name: zilean-postgres
    restart: unless-stopped
    shm_size: 256m  # required - default 64m causes "No space left on device" during bulk upserts
    # Do NOT expose ports unless you need external access - see Security section
    volumes:
      - zilean-pg:/var/lib/postgresql/data
    environment:
      - POSTGRES_DB=zilean
      - POSTGRES_PASSWORD=your_strong_password_here
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d zilean"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  zilean-data:
  zilean-pg:
```
