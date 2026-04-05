namespace Zilean.Tests.Fixtures;

public class PostgresLifecycleFixture : IAsyncLifetime
{
    private PostgreSqlContainer PostgresContainer { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:16.3-alpine3.20")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", "zilean")
        .Build();

    public ZileanConfiguration ZileanConfiguration { get; } = new();
    public ZileanWebApplicationFactory Factory { get; private set; } = null!;

    public PostgresLifecycleFixture() =>
        DerivePathInfo(
            (_, projectDirectory, type, method) => new(
                directory: Path.Combine(projectDirectory, "Verification"),
                typeName: type.Name,
                methodName: method.Name));

    public async Task InitializeAsync()
    {
        await PostgresContainer.StartAsync();
        var connectionString = PostgresContainer.GetConnectionString();
        ZileanConfiguration.Database.ConnectionString = connectionString;
        Factory = new ZileanWebApplicationFactory(connectionString);

        // Force host startup (runs migrations via StartupService)
        // CreateClient() blocks until the host is fully started
        using var client = Factory.CreateClient();

        // Seed test data once, after migrations are applied
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZileanDbContext>();
        await TestDataBuilder.SeedAsync(dbContext);

        // Update pg_trgm statistics so trigram similarity search works on seeded data
        await dbContext.Database.ExecuteSqlRawAsync("ANALYZE \"Torrents\";");
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await PostgresContainer.DisposeAsync();
    }
}
