namespace Zilean.Shared.Features.Configuration;

public class ImdbConfiguration
{
    public bool EnableImportMatching { get; set; } = true;
    public bool EnableEndpoint { get; set; } = true;
    public double MinimumScoreMatch { get; set; } = 0.85;

    public bool UseAllCores { get; set; } = false;

    public int NumberOfCores { get; set; } = 2;

    public bool UseLucene { get; set; } = false;

    /// <summary>
    /// Maximum number of (title, year, category) → IMDb ID entries kept in the in-memory match cache.
    /// Each entry costs ~250-350 bytes; the default of 100,000 is ~25-50 MB. Reduce for memory-constrained hosts.
    /// </summary>
    public int MatchCacheSize { get; set; } = 100_000;

    /// <summary>
    /// Hours after which a long-running scraper logs a one-shot warning recommending a restart so the
    /// in-memory IMDb snapshot is rebuilt from the (potentially refreshed) ImdbFiles table.
    /// Set to 0 to disable the warning.
    /// </summary>
    public int SnapshotMaxAgeHours { get; set; } = 24;
}
