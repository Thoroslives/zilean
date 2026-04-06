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
        Size = "15.5 GB",
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
        Size = "2.1 GB",
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
        Size = "1.8 GB",
        Seasons = [5],
        Episodes = [16],
        Languages = ["English"],
        IngestedAt = DateTime.UtcNow,
    };

    private static readonly TorrentInfo _mistbornEpub = new()
    {
        InfoHash = "ddeeff0033445566ddee33445566ddeeff004444",
        RawTitle = "Brandon.Sanderson.Mistborn.The.Final.Empire.EPUB",
        ParsedTitle = "Brandon Sanderson Mistborn The Final Empire",
        NormalizedTitle = "brandon sanderson mistborn the final empire",
        CleanedParsedTitle = "brandon sanderson mistborn final empire",
        Category = "book",
        Year = 2006,
        Resolution = null,
        ImdbId = null,
        Size = "2.1 MB",
        Seasons = [],
        Episodes = [],
        Languages = ["English"],
        Extension = ".epub",
        IngestedAt = DateTime.UtcNow,
    };

    private static readonly TorrentInfo _duneAudiobook = new()
    {
        InfoHash = "eeff001144556677eeff44556677eeff00115555",
        RawTitle = "Frank.Herbert.Dune.Audiobook.Unabridged.M4B",
        ParsedTitle = "Frank Herbert Dune",
        NormalizedTitle = "frank herbert dune",
        CleanedParsedTitle = "frank herbert dune",
        Category = "audiobook",
        Year = 1965,
        Resolution = null,
        ImdbId = null,
        Size = "850 MB",
        Seasons = [],
        Episodes = [],
        Languages = ["English"],
        Extension = ".m4b",
        IngestedAt = DateTime.UtcNow,
    };

    public static async Task SeedAsync(ZileanDbContext dbContext)
    {
        dbContext.Torrents.AddRange(_theMatrix, _theWitcherS01E01, _breakingBadS05E16, _mistbornEpub, _duneAudiobook);
        await dbContext.SaveChangesAsync();
    }
}
