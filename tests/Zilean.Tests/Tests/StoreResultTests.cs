using Zilean.Database.Services;

namespace Zilean.Tests.Tests;

public class StoreResultTests
{
    [Fact]
    public void StoreResult_HasExpectedFields()
    {
        var result = new StoreResult(Stored: 100, PopulateMs: 500, MatchMs: 50, UpsertMs: 30);
        result.Stored.Should().Be(100);
        result.PopulateMs.Should().Be(500);
        result.MatchMs.Should().Be(50);
        result.UpsertMs.Should().Be(30);
    }
}
