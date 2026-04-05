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
}
