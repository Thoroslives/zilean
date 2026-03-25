namespace Zilean.ApiService.Features.Bootstrapping;

public class StartupService(
    ZileanConfiguration configuration,
    IShellExecutionService executionService,
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : IHostedLifecycleService
{
    private const int MaxRetries = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger<StartupService>();

        // Security check — warn about insecure Postgres credentials
        if (configuration.Database.HasInsecurePassword())
        {
            logger.LogWarning("SECURITY WARNING: PostgreSQL password is empty or set to the default 'postgres'. " +
                "This is a security risk — if your database port is exposed, attackers can connect and compromise your system. " +
                "Set a strong password via POSTGRES_PASSWORD or Zilean__Database__ConnectionString.");
        }

        // Validate configuration before proceeding
        var validationErrors = configuration.Validate();
        if (validationErrors.Count > 0)
        {
            foreach (var error in validationErrors)
            {
                logger.LogError("Configuration error: {Error}", error);
            }
            throw new InvalidOperationException($"Zilean configuration is invalid: {string.Join("; ", validationErrors)}");
        }

        // Wait for database with retry
        await WaitForDatabaseAsync(logger, cancellationToken);

        logger.LogInformation("Applying Migrations...");
        await using var asyncScope = serviceProvider.CreateAsyncScope();
        var dbContext = asyncScope.ServiceProvider.GetRequiredService<ZileanDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migrations Applied.");
    }

    private async Task WaitForDatabaseAsync(ILogger logger, CancellationToken cancellationToken)
    {
        var connectionString = configuration.Database.ConnectionString;

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                logger.LogInformation("Database connection established.");
                return;
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                logger.LogWarning("Database connection attempt {Attempt}/{MaxRetries} failed: {Message}. Retrying in {Delay}s...",
                    attempt, MaxRetries, ex.Message, RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to database after {MaxRetries} attempts. " +
                    "Connection string: Host={Host}, Database={Database}. " +
                    "Check that PostgreSQL is running, the database exists, and credentials are correct.",
                    MaxRetries, GetConnectionHost(connectionString), GetConnectionDatabase(connectionString));
                throw;
            }
        }
    }

    private static string GetConnectionHost(string connectionString)
    {
        try { return new Npgsql.NpgsqlConnectionStringBuilder(connectionString).Host ?? "unknown"; }
        catch { return "unknown"; }
    }

    private static string GetConnectionDatabase(string connectionString)
    {
        try { return new Npgsql.NpgsqlConnectionStringBuilder(connectionString).Database ?? "unknown"; }
        catch { return "unknown"; }
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger<StartupService>();

        if (configuration.Dmm.EnableScraping)
        {
            await using var asyncScope = serviceProvider.CreateAsyncScope();
            var dbContext = asyncScope.ServiceProvider.GetRequiredService<ZileanDbContext>();
            var dmmJob = new DmmSyncJob(executionService, loggerFactory.CreateLogger<DmmSyncJob>(), dbContext);
            var pagesExist = await dmmJob.ShouldRunOnStartup();
            if (!pagesExist)
            {
                await dmmJob.Invoke();
            }
        }

        logger.LogInformation("Zilean Running: Startup Complete.");
    }
}
