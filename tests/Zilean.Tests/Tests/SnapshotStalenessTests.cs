using Zilean.Database.Services.Common;

namespace Zilean.Tests.Tests;

public class SnapshotStalenessTests
{
    [Fact]
    public void IsStale_ReturnsFalse_WhenSnapshotNeverPopulated()
    {
        var now = DateTimeOffset.UtcNow;

        SnapshotStaleness.IsStale(lastPopulatedAt: null, maxAgeHours: 24, now)
            .Should().BeFalse(
                "a never-populated matcher hasn't matched anything yet so there's nothing to warn about");
    }

    [Fact]
    public void IsStale_ReturnsFalse_WhenSnapshotIsFresh()
    {
        var now = DateTimeOffset.UtcNow;
        var loadedAt = now - TimeSpan.FromHours(2);

        SnapshotStaleness.IsStale(loadedAt, maxAgeHours: 24, now).Should().BeFalse();
    }

    [Fact]
    public void IsStale_ReturnsFalse_AtExactlyTheThreshold()
    {
        var now = DateTimeOffset.UtcNow;
        var loadedAt = now - TimeSpan.FromHours(24);

        SnapshotStaleness.IsStale(loadedAt, maxAgeHours: 24, now).Should().BeFalse(
            "the threshold is inclusive; the warning fires only once we exceed it");
    }

    [Fact]
    public void IsStale_ReturnsTrue_WhenOlderThanThreshold()
    {
        var now = DateTimeOffset.UtcNow;
        var loadedAt = now - TimeSpan.FromHours(25);

        SnapshotStaleness.IsStale(loadedAt, maxAgeHours: 24, now).Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsStale_ReturnsFalse_WhenMaxAgeDisabled(int maxAgeHours)
    {
        var now = DateTimeOffset.UtcNow;
        var loadedAt = now - TimeSpan.FromDays(30);

        SnapshotStaleness.IsStale(loadedAt, maxAgeHours, now).Should().BeFalse(
            "a non-positive max-age disables the staleness check");
    }
}
