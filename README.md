# Zilean (Maintained Fork)

<img src="docs/Writerside/images/zilean-logo.jpg" alt="zilean logo" width="300" height="300">

Zilean is a Torznab indexer for [DebridMediaManager](https://github.com/debridmediamanager/debrid-media-manager) sourced content shared by users.
It supports films, TV, books, and audiobooks through a single Torznab API, and can be configured as an indexer in Prowlarr, Sonarr, Radarr, Shelfarr, and other *arr applications.
It can also scrape from your running Zurg instance and from other running Zilean instances.

This is an actively maintained fork of [iPromKnight/zilean](https://github.com/iPromKnight/zilean) (v3.5.0, last upstream commit May 2025).

Upstream documentation: [https://ipromknight.github.io/zilean/](https://ipromknight.github.io/zilean/)

## Requirements

Zilean requires only **PostgreSQL 16+**. Elasticsearch is **NOT** required and was removed in v2.0.

## Docker Image

```
ghcr.io/thoroslives/zilean:latest
```

## Fork Changes

This is an actively maintained fork with improvements to reliability, search quality, security, and media type support. Key additions include book/audiobook category detection, graceful degradation, incremental DMM sync, and flexible database configuration.

See [Releases](https://github.com/Thoroslives/zilean/releases) for the full changelog.

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

## Supported Categories

| Category | Torznab ID | Search Type | Detection |
|----------|-----------|-------------|-----------|
| Movies | 2000 | `movie` | RTN media type |
| TV | 5000 | `tvsearch` | RTN media type |
| Books | 7000 | `book-search` | File extension (`.epub`, `.mobi`, `.azw3`, `.cbr`, `.cbz`) and title keywords (`ebook`, `epub`, etc.) |
| Audiobooks | 3030 | `search` with `cat=3030` | File extension (`.m4b`) and title keywords (`audiobook`, `narrated by`, `unabridged`, `abridged`) |
| XXX | 6000 | `xxx` | RTN adult flag |

Book and audiobook detection runs as post-RTN heuristics during ingestion. Existing records are reclassified as they are re-ingested during hourly DMM syncs.

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
- **Book/audiobook searches:** these categories are only populated after torrents are ingested with the v4.0.0+ detection heuristics. Existing records from earlier versions retain their original category until re-ingested during the hourly DMM sync
- **Short queries return fewer results:** Zilean uses trigram similarity matching. Single-word queries like "dune" may not match longer titles like "Dune - Audiobook Collection" because the similarity score is too low. Use more specific queries (e.g. "dune audiobook") for better results. This applies to all categories, not just books

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
