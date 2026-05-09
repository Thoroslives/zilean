namespace Zilean.Database.Services.Common;

internal static class ImdbCacheConfig
{
    public const int MinCacheSize = 1_000;
    public const int DefaultCacheSize = 100_000;

    public static int ResolveCacheSize(int configured, ILogger logger)
    {
        if (configured >= MinCacheSize)
        {
            return configured;
        }

        logger.LogWarning(
            "ImdbConfiguration.MatchCacheSize={Configured} is below the minimum of {Min}; falling back to {Default}.",
            configured, MinCacheSize, DefaultCacheSize);
        return DefaultCacheSize;
    }
}

internal static class SnapshotStaleness
{
    public static bool IsStale(DateTimeOffset? lastPopulatedAt, int maxAgeHours, DateTimeOffset now)
    {
        if (lastPopulatedAt is null || maxAgeHours <= 0)
        {
            return false;
        }

        return now - lastPopulatedAt.Value > TimeSpan.FromHours(maxAgeHours);
    }
}

internal sealed class SnapshotStalenessTracker(ILogger logger)
{
    private DateTimeOffset? _populatedAt;
    private bool _warned;

    public DateTimeOffset? PopulatedAt => _populatedAt;

    public void MarkPopulated()
    {
        _populatedAt = DateTimeOffset.UtcNow;
        _warned = false;
    }

    public void Reset()
    {
        _populatedAt = null;
        _warned = false;
    }

    public void WarnIfStale(int maxAgeHours)
    {
        if (_warned)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (!SnapshotStaleness.IsStale(_populatedAt, maxAgeHours, now))
        {
            return;
        }

        _warned = true;
        logger.LogWarning(
            "IMDb in-memory snapshot is {AgeHours:F1}h old (loaded at {LoadedAt:O}); restart Zilean to pick up newer entries from the ImdbFiles table.",
            (now - _populatedAt!.Value).TotalHours,
            _populatedAt.Value);
    }
}
