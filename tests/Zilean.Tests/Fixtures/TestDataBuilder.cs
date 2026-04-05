using Zilean.Database;

namespace Zilean.Tests.Fixtures;

public static class TestDataBuilder
{
    private static readonly TorrentInfo _theMatrix = new()
    {
        InfoHash = "aabbccdd00112233aabb00112233aabbccdd0011",
        RawTitle = "The.Matrix.1999.2160p.UHD.BluRay.X265-IAMABLE",
        ParsedTitle = "The Matrix",
        NormalizedTitle = "the matrix",
        CleanedParsedTitle = "matrix",
        Category = "movie",
        Year = 1999,
        Resolution = "2160p",
        ImdbId = "tt0133093",
        Seasons = [],
        Episodes = [],
        Languages = ["English"],
        IngestedAt = DateTime.UtcNow,
    };

    private static readonly TorrentInfo _theWitcherS01E01 = new()
    {
        InfoHash = "bbccddee11223344bbcc11223344bbccddeeff22",
        RawTitle = "The.Witcher.S01E01.1080p.WEB.H264-METCON",
        ParsedTitle = "The Witcher",
        NormalizedTitle = "the witcher",
        CleanedParsedTitle = "witcher",
        Category = "tvSeries",
        Year = 2019,
        Resolution = "1080p",
        ImdbId = "tt5180504",
        Seasons = [1],
        Episodes = [1],
        Languages = ["English"],
        IngestedAt = DateTime.UtcNow,
    };

    private static readonly TorrentInfo _breakingBadS05E16 = new()
    {
        InfoHash = "ccddeeff22334455ccdd22334455ccddeeff0033",
        RawTitle = "Breaking.Bad.S05E16.720p.BluRay",
        ParsedTitle = "Breaking Bad",
        NormalizedTitle = "breaking bad",
        CleanedParsedTitle = "breaking bad",
        Category = "tvSeries",
        Year = 2013,
        Resolution = "720p",
        ImdbId = "tt0903747",
        Seasons = [5],
        Episodes = [16],
        Languages = ["English"],
        IngestedAt = DateTime.UtcNow,
    };

    public static async Task SeedAsync(ZileanDbContext dbContext)
    {
        dbContext.Torrents.AddRange(_theMatrix, _theWitcherS01E01, _breakingBadS05E16);
        await dbContext.SaveChangesAsync();
    }
}
