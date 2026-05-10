using Zilean.Shared.Features.Configuration;

namespace Zilean.Tests.Tests;

public class ParserConcurrencyTests
{
    [Fact]
    public void ResolveMaxConcurrentTasks_IsCappedAtEight()
    {
        var resolved = ParserConcurrency.ResolveMaxConcurrentTasks();
        resolved.Should().BeGreaterThan(0);
        resolved.Should().BeLessThanOrEqualTo(ParserConcurrency.MaxThreadPoolWorkers);
        resolved.Should().BeLessThanOrEqualTo(Environment.ProcessorCount,
            "we never want more workers than the box has cores");
    }

    [Fact]
    public void MaxThreadPoolWorkers_IsEight()
    {
        ParserConcurrency.MaxThreadPoolWorkers.Should().Be(8,
            "8 is the empirical sweet spot before GIL contention dominates threadpool dispatch");
    }
}
