using Zilean.Shared.Features.Python;

namespace Zilean.Tests.Tests;

public class CategoryDetectionTests
{
    // --- Book by extension ---

    [Theory]
    [InlineData(".epub")]
    [InlineData(".mobi")]
    [InlineData(".azw3")]
    [InlineData(".cbr")]
    [InlineData(".cbz")]
    public void DetectCategory_BookExtension_ReturnsBook(string extension)
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: extension,
            rawTitle: "Some.Random.Title",
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("book");
    }

    // --- PDF requires keyword ---

    [Fact]
    public void DetectCategory_PdfWithoutKeyword_FallsThrough()
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: ".pdf",
            rawTitle: "Some.Random.Document.2024",
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("movie");
    }

    [Theory]
    [InlineData("Programming.Textbook.2024.pdf")]
    [InlineData("Manga.One.Piece.Vol.100")]
    [InlineData("Some.Ebook.Collection")]
    public void DetectCategory_PdfWithBookKeyword_ReturnsBook(string rawTitle)
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: ".pdf",
            rawTitle: rawTitle,
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("book");
    }

    // --- Book by title keyword (no extension) ---

    [Theory]
    [InlineData("Brandon.Sanderson.Mistborn.EPUB")]
    [InlineData("Collection.AZW3.Pack")]
    [InlineData("Comics.CBZ.Pack")]
    [InlineData("Great.Ebook.Collection.2024")]
    public void DetectCategory_BookKeywordInTitle_NoExtension_ReturnsBook(string rawTitle)
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: null,
            rawTitle: rawTitle,
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("book");
    }

    // --- Substring false positive prevention ---

    [Theory]
    [InlineData("Mobile.Suit.Gundam.S01.1080p")]
    [InlineData("Honda.CBR600.Racing.Documentary")]
    public void DetectCategory_SubstringTrap_DoesNotClassifyAsBook(string rawTitle)
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: null,
            rawTitle: rawTitle,
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("movie");
    }

    // --- Audiobook by extension ---

    [Fact]
    public void DetectCategory_M4bExtension_ReturnsAudiobook()
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: ".m4b",
            rawTitle: "Some.Random.Title",
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("audiobook");
    }

    // --- Audiobook mp3 + keyword ---

    [Theory]
    [InlineData("Brandon.Sanderson.Mistborn.Audiobook")]
    [InlineData("The.Hobbit.Narrated.By.Andy.Serkis")]
    public void DetectCategory_Mp3WithAudiobookKeyword_ReturnsAudiobook(string rawTitle)
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: ".mp3",
            rawTitle: rawTitle,
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("audiobook");
    }

    [Fact]
    public void DetectCategory_Mp3WithoutKeyword_FallsThrough()
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: ".mp3",
            rawTitle: "Some.Music.Album.2024",
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("movie");
    }

    // --- Audiobook by title keyword (no extension) ---

    [Theory]
    [InlineData("Brandon.Sanderson.Mistborn.Audiobook.MP3")]
    [InlineData("The.Hobbit.Narrated.By.Andy.Serkis")]
    [InlineData("Dune.Unabridged.2021")]
    [InlineData("Project.Hail.Mary.Abridged")]
    public void DetectCategory_AudiobookKeywordInTitle_NoExtension_ReturnsAudiobook(string rawTitle)
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: null,
            rawTitle: rawTitle,
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("audiobook");
    }

    // --- Priority: adult beats everything ---

    [Fact]
    public void DetectCategory_AdultWithBookExtension_ReturnsXxx()
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: ".epub",
            rawTitle: "Some.Adult.Content",
            isAdult: true,
            mediaType: "movie");

        result.Should().Be("xxx");
    }

    // --- Priority: audiobook beats book ---

    [Fact]
    public void DetectCategory_AudiobookKeywordWithBookExtension_ReturnsAudiobook()
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: ".mp3",
            rawTitle: "Mistborn.Audiobook.EPUB.MP3",
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("audiobook");
    }

    // --- Fallback to movie/TV ---

    [Fact]
    public void DetectCategory_NoMatch_ReturnsMovieForMovie()
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: null,
            rawTitle: "The.Matrix.1999.2160p",
            isAdult: false,
            mediaType: "movie");

        result.Should().Be("movie");
    }

    [Fact]
    public void DetectCategory_NoMatch_ReturnsTvSeriesForTv()
    {
        var result = ParseTorrentNameService.DetectCategory(
            extension: null,
            rawTitle: "Breaking.Bad.S05E16",
            isAdult: false,
            mediaType: "tvSeries");

        result.Should().Be("tvSeries");
    }
}
