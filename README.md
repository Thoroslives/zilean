# Zilean (Thoroslives Fork)

<img src="docs/Writerside/images/zilean-logo.jpg" alt="zilean logo" width="300" height="300">

Zilean is a service that allows you to search for [DebridMediaManager](https://github.com/debridmediamanager/debrid-media-manager) sourced content shared by users.
This can then be configured as a Torznab indexer in your favorite content application.
Newly added is the ability for Zilean to scrape from your running Zurg instance, and from other running Zilean instances.

Upstream documentation: [https://ipromknight.github.io/zilean/](https://ipromknight.github.io/zilean/)

## Fork Changes

This fork (based on [iPromKnight/zilean](https://github.com/iPromKnight/zilean) v3.5.0) includes:

- **Flexible database configuration** — supports `Zilean__Database__ConnectionString` env var (backwards compat), individual `POSTGRES_*` env vars, or sensible defaults. Uses `NpgsqlConnectionStringBuilder` for proper escaping of special characters in passwords.
- **Incremental DMM sync** — replaces the 1.2GB zip download with `git clone --depth 1` on first run and `git pull` on subsequent runs. Supports `GITHUB_TOKEN` for authenticated requests (5,000 req/hr vs 60). Includes exponential backoff retry.
- **Logging config preservation** — `logging.json` is only written if it doesn't exist, preserving user customizations across restarts.

## Docker Image

```
ghcr.io/thoroslives/zilean:latest
```

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
      - POSTGRES_PASSWORD=your_password
      - GITHUB_TOKEN=ghp_xxxxxxxxxxxx  # optional, recommended

  postgres:
    image: postgres:16-alpine
    container_name: zilean-postgres
    restart: unless-stopped
    shm_size: 256m  # required — default 64m causes "No space left on device" during bulk upserts
    volumes:
      - zilean-pg:/var/lib/postgresql/data
    environment:
      - POSTGRES_DB=zilean
      - POSTGRES_PASSWORD=your_password

volumes:
  zilean-data:
  zilean-pg:
```
