using Microsoft.EntityFrameworkCore.Design;

namespace Zilean.Database;

public class ZileanDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ZileanDbContext>
{
    public ZileanDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("Zilean__Database__ConnectionString")
            ?? "Host=localhost;Port=5432;Database=zilean;Username=postgres;Password=postgres;Include Error Detail=true;";

        var optionsBuilder = new DbContextOptionsBuilder<ZileanDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ZileanDbContext(optionsBuilder.Options);
    }
}
