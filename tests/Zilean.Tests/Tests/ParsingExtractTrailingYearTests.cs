using Zilean.Shared.Features.Utilities;

namespace Zilean.Tests.Tests;

public class ParsingExtractTrailingYearTests
{
    [Theory]
    [InlineData("Oppenheimer 2023", "Oppenheimer", 2023)]
    [InlineData("Oppenheimer (2023)", "Oppenheimer", 2023)]
    [InlineData("The Matrix 1999", "The Matrix", 1999)]
    [InlineData("Oppenheimer 2023 ", "Oppenheimer", 2023)]
    [InlineData("  Oppenheimer 2023  ", "  Oppenheimer", 2023)]
    public void ExtractTrailingYear_TrailingYear_StripsAndReturns(string input, string expectedQuery, int expectedYear)
    {
        var (query, year) = Parsing.ExtractTrailingYear(input);

        query.Should().Be(expectedQuery);
        year.Should().Be(expectedYear);
    }

    [Theory]
    [InlineData("1923")]
    [InlineData("The Matrix 1999 Remastered")]
    [InlineData("Akira 2019Remix")]
    [InlineData("Oppenheimer 1899")]
    [InlineData("Oppenheimer 2100")]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractTrailingYear_NoTrailingYear_ReturnsInputUnchanged(string input)
    {
        var (query, year) = Parsing.ExtractTrailingYear(input);

        query.Should().Be(input);
        year.Should().BeNull();
    }

    [Fact]
    public void ExtractTrailingYear_Null_ReturnsNull()
    {
        var (query, year) = Parsing.ExtractTrailingYear(null);

        query.Should().BeNull();
        year.Should().BeNull();
    }
}
