namespace Zilean.Database;

public class ZileanDbContext : DbContext
{
    public ZileanDbContext(DbContextOptions<ZileanDbContext> options): base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TorrentInfoConfiguration());
        modelBuilder.ApplyConfiguration(new ImdbFileConfiguration());
        modelBuilder.ApplyConfiguration(new ParsedPagesConfiguration());
        modelBuilder.ApplyConfiguration(new ImportMetadataConfiguration());
        modelBuilder.ApplyConfiguration(new BlacklistedItemConfiguration());
    }

    public DbSet<TorrentInfo> Torrents => Set<TorrentInfo>();
    public DbSet<ImdbFile> ImdbFiles => Set<ImdbFile>();
    public DbSet<ParsedPages> ParsedPages => Set<ParsedPages>();
    public DbSet<ImportMetadata> ImportMetadata => Set<ImportMetadata>();
    public DbSet<BlacklistedItem> BlacklistedItems => Set<BlacklistedItem>();
}
