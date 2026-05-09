using Zilean.Database.Services.FuzzyString;
using Zilean.Database.Services.Lucene;

namespace Zilean.Tests.Tests;

[Collection(nameof(ApiTestCollection))]
public class ImdbMatchingServiceIdempotenceTests
{
    private readonly ZileanConfiguration _configuration;

    public ImdbMatchingServiceIdempotenceTests(PostgresLifecycleFixture fixture) =>
        _configuration = fixture.ZileanConfiguration;

    [Fact]
    public async Task ImdbLuceneMatchingService_PopulateImdbData_CalledMultipleTimes_InitializesOnlyOnce()
    {
        var logger = Substitute.For<ILogger<ImdbLuceneMatchingService>>();
        var service = new ImdbLuceneMatchingService(logger, _configuration);

        try
        {
            await service.PopulateImdbData();
            await service.PopulateImdbData();
            await service.PopulateImdbData();
            await service.PopulateImdbData();
            await service.PopulateImdbData();

            service.InitializationCount.Should().Be(
                1,
                "PopulateImdbData should be idempotent so per-batch calls in StoreTorrentInfo do not rebuild the Lucene index every time");
        }
        finally
        {
            service.DisposeImdbData();
        }
    }

    [Fact]
    public async Task ImdbLuceneMatchingService_PopulateImdbData_AfterDispose_ReinitializesOnNextCall()
    {
        var logger = Substitute.For<ILogger<ImdbLuceneMatchingService>>();
        var service = new ImdbLuceneMatchingService(logger, _configuration);

        try
        {
            await service.PopulateImdbData();
            service.InitializationCount.Should().Be(1);

            service.DisposeImdbData();

            await service.PopulateImdbData();
            service.InitializationCount.Should().Be(
                2,
                "Dispose should invalidate the in-memory state so a subsequent Populate (e.g. after ResyncImdbCommand refreshes the IMDb table) actually reloads");
        }
        finally
        {
            service.DisposeImdbData();
        }
    }

    [Fact]
    public async Task ImdbFuzzyStringMatchingService_PopulateImdbData_CalledMultipleTimes_InitializesOnlyOnce()
    {
        var logger = Substitute.For<ILogger<ImdbFuzzyStringMatchingService>>();
        var service = new ImdbFuzzyStringMatchingService(logger, _configuration);

        try
        {
            await service.PopulateImdbData();
            await service.PopulateImdbData();
            await service.PopulateImdbData();
            await service.PopulateImdbData();
            await service.PopulateImdbData();

            service.InitializationCount.Should().Be(
                1,
                "PopulateImdbData should be idempotent so per-batch calls in StoreTorrentInfo do not reload the IMDb dictionaries every time");
        }
        finally
        {
            service.DisposeImdbData();
        }
    }

    [Fact]
    public async Task ImdbFuzzyStringMatchingService_PopulateImdbData_AfterDispose_ReinitializesOnNextCall()
    {
        var logger = Substitute.For<ILogger<ImdbFuzzyStringMatchingService>>();
        var service = new ImdbFuzzyStringMatchingService(logger, _configuration);

        try
        {
            await service.PopulateImdbData();
            service.InitializationCount.Should().Be(1);

            service.DisposeImdbData();

            await service.PopulateImdbData();
            service.InitializationCount.Should().Be(
                2,
                "Dispose should invalidate the in-memory state so a subsequent Populate actually reloads");
        }
        finally
        {
            service.DisposeImdbData();
        }
    }
}
