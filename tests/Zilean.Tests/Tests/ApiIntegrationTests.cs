using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Zilean.Tests.Tests;

[Collection(nameof(ApiTestCollection))]
public class ApiIntegrationTests
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(PostgresLifecycleFixture fixture)
    {
        // Fixture handles Postgres startup, migrations, and data seeding.
        // CreateClient() is safe here - host is already started in fixture.
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_Ping_ReturnsPong()
    {
        var response = await _client.GetAsync("/healthchecks/ping");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Pong!");
    }

    [Fact]
    public async Task HealthCheck_Ready_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/healthchecks/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("healthy");
    }

    [Fact]
    public async Task Torznab_Caps_ReturnsValidXml()
    {
        var response = await _client.GetAsync("/torznab/api?t=caps");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();

        var doc = XDocument.Parse(body);
        doc.Root.Should().NotBeNull();
        doc.Root!.Name.LocalName.Should().Be("caps");
        doc.Root!.Element("searching").Should().NotBeNull();
        doc.Root!.Element("categories").Should().NotBeNull();
    }

    [Fact]
    public async Task Torznab_Search_ReturnsResults_WhenDataExists()
    {
        var response = await _client.GetAsync("/torznab/api?t=search&q=The%20Matrix");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();

        var doc = XDocument.Parse(body);
        doc.Root.Should().NotBeNull();
        doc.Root!.Name.LocalName.Should().Be("rss");

        var items = doc.Root!.Element("channel")?.Elements("item");
        items.Should().NotBeNullOrEmpty("seeded data includes The Matrix");
    }

    [Fact]
    public async Task FilteredSearch_ByImdbId_ReturnsCorrectResults()
    {
        var response = await _client.GetAsync("/dmm/filtered?ImdbId=tt0133093");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();

        var results = JsonSerializer.Deserialize<TorrentInfo[]>(body);
        results.Should().NotBeNull();
        results!.Should().Contain(t => t.ImdbId == "tt0133093");
    }

    [Fact]
    public async Task FilteredSearch_EmptyDatabase_ReturnsEmptyArray()
    {
        // Search for something that doesn't exist in seed data
        var response = await _client.GetAsync("/dmm/filtered?ImdbId=tt9999999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();

        var results = JsonSerializer.Deserialize<TorrentInfo[]>(body);
        results.Should().NotBeNull();
        results!.Should().BeEmpty();
    }

    [Fact]
    public async Task Torznab_Caps_ContainsBookAndAudioCategories()
    {
        var response = await _client.GetAsync("/torznab/api?t=caps");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(body);

        var categories = doc.Root!.Element("categories")!.Elements("category");
        categories.Should().Contain(c => c.Attribute("id")!.Value == "7000", "Books category should be present");
        categories.Should().Contain(c => c.Attribute("id")!.Value == "3000", "Audio category should be present");

        var searching = doc.Root!.Element("searching");
        searching!.Element("book-search").Should().NotBeNull("book-search should be in caps");
        searching!.Element("book-search")!.Attribute("available")!.Value.Should().Be("yes");
    }

    [Fact]
    public async Task Torznab_BookSearch_ReturnsBookResults()
    {
        var response = await _client.GetAsync("/torznab/api?t=book-search&q=Mistborn");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(body);

        var items = doc.Root!.Element("channel")?.Elements("item");
        items.Should().NotBeNullOrEmpty("seeded data includes Mistborn EPUB");
    }

    [Fact]
    public async Task Torznab_AudiobookSearch_ByCategory_ReturnsAudiobookResults()
    {
        var response = await _client.GetAsync("/torznab/api?t=search&cat=3030&q=Dune");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(body);

        var items = doc.Root!.Element("channel")?.Elements("item");
        items.Should().NotBeNullOrEmpty("seeded data includes Dune Audiobook");
    }

    [Fact]
    public async Task Torznab_MovieSearch_DoesNotReturnBooks()
    {
        var response = await _client.GetAsync("/torznab/api?t=movie&q=Mistborn");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(body);

        var items = doc.Root!.Element("channel")?.Elements("item");
        items.Should().BeNullOrEmpty("books should not appear in movie search");
    }
}
