using Zilean.Database.Services;
using Zilean.Database.Services.Lucene;

namespace Zilean.Tests.Tests;

[Collection(nameof(ApiTestCollection))]
[Trait("Category", "Benchmark")]
public class ImdbMatcherThroughputTests
{
    private const int ImdbFileCount = 2_000;
    private const int TorrentsPerBatch = 500;
    private const int BatchCount = 6;

    private readonly ZileanConfiguration _configuration;
    private readonly ZileanWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;

    public ImdbMatcherThroughputTests(PostgresLifecycleFixture fixture, ITestOutputHelper output)
    {
        _configuration = fixture.ZileanConfiguration;
        _factory = fixture.Factory;
        _output = output;
    }

    [Fact]
    public async Task LuceneMatcher_HoistedPopulate_BeatsPerBatchPopulate()
    {
        await SeedImdbFilesIfNeeded();
        var batches = BuildBatches();
        var logger = Substitute.For<ILogger<ImdbLuceneMatchingService>>();

        var hoistedTime = await TimeHoistedPath(
            () => new ImdbLuceneMatchingService(logger, _configuration),
            batches);
        var perBatchTime = await TimePerBatchPath(
            () => new ImdbLuceneMatchingService(logger, _configuration),
            batches);

        _output.WriteLine($"[Lucene] hoisted: {hoistedTime.TotalMilliseconds:F0}ms, per-batch: {perBatchTime.TotalMilliseconds:F0}ms (×{perBatchTime.TotalMilliseconds / hoistedTime.TotalMilliseconds:F1} slower)");

        (hoistedTime * 2).Should().BeLessThan(perBatchTime,
            "hoisting PopulateImdbData out of the per-batch loop is the entire point of this PR; the 2x slack absorbs CI noise");
    }

    private static async Task<TimeSpan> TimeHoistedPath(
        Func<IImdbMatchingService> factory,
        IReadOnlyList<List<TorrentInfo>> batches)
    {
        var matcher = factory();
        try
        {
            await matcher.PopulateImdbData();

            var sw = Stopwatch.StartNew();
            foreach (var batch in batches)
            {
                await matcher.MatchImdbIdsForBatchAsync(batch);
            }
            sw.Stop();
            return sw.Elapsed;
        }
        finally
        {
            matcher.DisposeImdbData();
        }
    }

    private static async Task<TimeSpan> TimePerBatchPath(
        Func<IImdbMatchingService> factory,
        IReadOnlyList<List<TorrentInfo>> batches)
    {
        var sw = Stopwatch.StartNew();
        foreach (var batch in batches)
        {
            var matcher = factory();
            try
            {
                await matcher.PopulateImdbData();
                await matcher.MatchImdbIdsForBatchAsync(batch);
            }
            finally
            {
                matcher.DisposeImdbData();
            }
        }
        sw.Stop();
        return sw.Elapsed;
    }

    private async Task SeedImdbFilesIfNeeded()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZileanDbContext>();

        if (await dbContext.ImdbFiles.AnyAsync())
        {
            return;
        }

        var random = new Random(42);
        var categories = new[] { "movie", "tvSeries", "tvMiniSeries", "tvMovie" };
        var rows = new List<ImdbFile>(ImdbFileCount);

        for (var i = 0; i < ImdbFileCount; i++)
        {
            rows.Add(new ImdbFile
            {
                ImdbId = $"tt{i:D7}",
                Title = $"synthetic title {i:D6}",
                Category = categories[random.Next(categories.Length)],
                Year = 1980 + random.Next(45),
                Adult = false,
            });
        }

        dbContext.ImdbFiles.AddRange(rows);
        await dbContext.SaveChangesAsync();
    }

    private static List<List<TorrentInfo>> BuildBatches()
    {
        var random = new Random(42);
        var categories = new[] { "movie", "tvSeries" };
        var batches = new List<List<TorrentInfo>>(BatchCount);

        for (var b = 0; b < BatchCount; b++)
        {
            var batch = new List<TorrentInfo>(TorrentsPerBatch);
            for (var i = 0; i < TorrentsPerBatch; i++)
            {
                var idx = random.Next(ImdbFileCount);
                var year = 1980 + random.Next(45);
                batch.Add(new TorrentInfo
                {
                    InfoHash = $"{b:D2}{i:D6}{Guid.NewGuid():N}"[..40],
                    RawTitle = $"synthetic title {idx:D6}",
                    ParsedTitle = $"synthetic title {idx:D6}",
                    NormalizedTitle = $"synthetic title {idx:D6}",
                    Year = year,
                    Category = categories[random.Next(categories.Length)],
                });
            }
            batches.Add(batch);
        }

        return batches;
    }
}
