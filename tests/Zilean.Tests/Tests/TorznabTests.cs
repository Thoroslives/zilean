using Zilean.Shared.Features.Torznab;
using Zilean.Shared.Features.Torznab.Info;

namespace Zilean.Tests.Tests;

public class TorznabTests
{
    [Fact]
    public void Caps_ToXml_ReturnsValidXml()
    {
        var xml = TorznabCapabilities.ToXml();

        xml.Should().NotBeNullOrWhiteSpace();

        var doc = XDocument.Parse(xml);
        doc.Root.Should().NotBeNull();
        doc.Root!.Name.LocalName.Should().Be("caps");
    }

    [Fact]
    public void Caps_ToXml_ContainsSearchTypes()
    {
        var xml = TorznabCapabilities.ToXml();
        var doc = XDocument.Parse(xml);

        var searching = doc.Root!.Element("searching");
        searching.Should().NotBeNull();
        searching!.Element("tv-search").Should().NotBeNull();
        searching!.Element("movie-search").Should().NotBeNull();
        searching!.Element("tv-search")!.Attribute("available")!.Value.Should().Be("yes");
        searching!.Element("movie-search")!.Attribute("available")!.Value.Should().Be("yes");
    }

    [Fact]
    public void Query_CanHandleQuery_ValidQuery_ReturnsTrue()
    {
        var query = new TorznabQuery
        {
            QueryType = "search",
            SearchTerm = "The Matrix",
        };

        query.IsSearch.Should().BeTrue();
        query.SearchTerm.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Query_CanHandleQuery_EmptyQuery_ReturnsFalse()
    {
        var query = new TorznabQuery
        {
            QueryType = "search",
            SearchTerm = null,
        };

        query.IsRssSearch.Should().BeTrue();
        query.SearchTerm.Should().BeNull();
    }

    [Fact]
    public void Query_GetQueryString_SanitizesDashes()
    {
        var query = new TorznabQuery
        {
            SearchTerm = "Spider\u2010Man: No\u2013Way Home",
        };

        var result = query.SanitizedSearchTerm;

        // Unicode dashes should be standardized to ASCII dash
        result.Should().NotContain("\u2010");
        result.Should().NotContain("\u2013");
        result.Should().Contain("-");
    }

    [Fact]
    public void ResultPage_ToXml_ValidRssWithTorznabNamespace()
    {
        var page = new ResultPage
        {
            Releases =
            [
                new ReleaseInfo
                {
                    Title = "Test.Movie.2024.1080p",
                    InfoHash = "aabbccdd00112233aabb00112233aabbccdd0011",
                    Size = 1500000000,
                    Category = [2000],
                    PublishDate = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
                    Imdb = 1234567,
                    Year = 2024,
                    Magnet = new Uri("magnet:?xt=urn:btih:aabbccdd00112233aabb00112233aabbccdd0011"),
                }
            ]
        };

        var xml = page.ToXml(new Uri("http://localhost/torznab/api"));

        xml.Should().NotBeNullOrWhiteSpace();

        var doc = XDocument.Parse(xml);
        doc.Root.Should().NotBeNull();
        doc.Root!.Name.LocalName.Should().Be("rss");
        doc.Root!.Attribute("version")!.Value.Should().Be("2.0");

        // Torznab namespace must be declared
        var torznabNs = doc.Root!.Attributes()
            .FirstOrDefault(a => a.Value == "http://torznab.com/schemas/2015/feed");
        torznabNs.Should().NotBeNull();

        // Should have at least one item
        var channel = doc.Root!.Element("channel");
        channel.Should().NotBeNull();
        channel!.Elements("item").Should().HaveCount(1);
    }
}
